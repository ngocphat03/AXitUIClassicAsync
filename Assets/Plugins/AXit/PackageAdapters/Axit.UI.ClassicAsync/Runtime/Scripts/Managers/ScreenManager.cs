namespace AxitUnityTemplate.UI.Classic.Async
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Collections.Generic;
    using UnityEngine;
    using Cysharp.Threading.Tasks;

#if SCREEN_CLASSIC_ADDRESSABLE
    using UnityEngine.AddressableAssets;
#endif

    public sealed class ScreenManager : MonoBehaviour
    {
        [field: SerializeField] public Transform OpenedScreenParent { get; private set; }

        [field: SerializeField] public Transform ClosedScreenParent { get; private set; }
        
        [field: SerializeField] public bool AutoCreateScreenFactory { get; private set; } = false;

#if ZENJECT
#elif VCONTAINER
        [VContainer.Inject] private ScreenFactory screenFactory;
#else
        private readonly ScreenFactory screenFactory = new();
#endif

        public static ScreenManager Instance { get; private set; }

        private readonly Dictionary<Type, IScreenPresenter> screensPresenterLoaded = new(10);
        private readonly List<GameObject> uisViewLoaded = new(10);

        private IScreenPresenter CurrentScreen { get; set; }

        private readonly List<(IScreenPresenter screen, IScreenPresenter lastScreen)> historyScreen = new();

        private readonly Dictionary<Type, IPopupPresenter> popupsPresenterLoaded = new(10);

        public static Func<ScreenManager> Resolve = () => Instance;

#if VCONTAINER
        [VContainer.Inject]
        private void VContainerAwake()
        {
            this.Initialize();
        }
#else
        private void Awake()
        {
            this.Initialize();
        }
#endif
        
        private void Initialize()
        {
            if (ScreenManager.Instance != null && ScreenManager.Instance != this)
            {
                Debug.LogWarning("Multiple instances of ScreenManager detected. Destroying the new instance.");
                GameObject.Destroy(this.gameObject);
                return;
            }

            ScreenManager.Instance = this;

#if VCONTAINER
            // Check if the screen factory is set, if not and AutoCreateScreenFactory is true, create a new instance
            if (this.screenFactory == null && this.AutoCreateScreenFactory)
            {
                this.screenFactory = new ScreenFactory(null);
            }
#endif

            _ = UniTask.DelayFrame(1).ContinueWith(this.FindScreensInScene);
        }

        private void OnDestroy()
        {
            if (ScreenManager.Instance == this)
            {
                ScreenManager.Instance = null;
            }
            foreach (var screen in this.screensPresenterLoaded)
            {
                screen.Value.OnDestroy();
            }

            foreach (var popup in this.popupsPresenterLoaded)
            {
                popup.Value.OnDestroy();
            }
        }

        #region SCREEN
        
        public async UniTask<TPresenter> OpenScreenAsync<TPresenter, TModel>(TModel model = default) where TPresenter : IScreenPresenter where TModel : IScreenModel
        {
            var presenter = await this.GetScreenAsync<TPresenter>(model);

            await this.CloseScreenAsync(this.CurrentScreen?.GetType(), openLastScreen: false);

            var screenOpen = this.historyScreen.Find(x => x.screen.GetType() == presenter.GetType());

            if (screenOpen == default)
            {
                screenOpen = (presenter, this.CurrentScreen);
                this.historyScreen.Add(screenOpen);
            }
            else
            {
                screenOpen.lastScreen = this.CurrentScreen;
            }
            ((IUiManagerAccess)presenter).SetViewParent(this.OpenedScreenParent);
            this.CurrentScreen = presenter;
            await ((IUiManagerAccess)this.CurrentScreen).OpenViewAsync();
            return (TPresenter)this.CurrentScreen;
        }

        public UniTask CloseScreenAsync<TPresenter>() where TPresenter : IScreenPresenter
        {
            return this.CloseScreenAsync(typeof(TPresenter), openLastScreen: true);
        }
        
        public async UniTask CloseScreenAsync(Type typeScreenPresenter, bool openLastScreen = true)
        {
            if (typeScreenPresenter == null) return;

            if (!this.screensPresenterLoaded.TryGetValue(typeScreenPresenter, out var presenter))
            {
                Debug.LogError($"The {typeScreenPresenter.Name} screen does not exist");
                return;
            }

            await ((IUiManagerAccess)presenter).CloseViewAsync();
            ((IUiManagerAccess)presenter).SetViewParent(this.ClosedScreenParent);
            
            if (!openLastScreen) return;
            
            var lastScreen = this.historyScreen.Find(x => x.screen.GetType() == typeScreenPresenter).lastScreen;

            if (lastScreen != null)
            {
                this.CurrentScreen = lastScreen;
                if (this.CurrentScreen == null) return;

                ((IUiManagerAccess)this.CurrentScreen).SetViewParent(this.OpenedScreenParent);
                await ((IUiManagerAccess)this.CurrentScreen).OpenViewAsync();
            }
        }

        public async UniTask<T> GetScreenAsync<T>(IScreenModel model = null) where T : IScreenPresenter
        {
            var screenType = typeof(T);

            if (this.screensPresenterLoaded.TryGetValue(screenType, out var screenPresenter))
            {
                ((IUiManagerAccess)screenPresenter).SetModel(model);
                return (T)screenPresenter;
            }

            screenPresenter = this.screenFactory.CreateUiPresenter<T>() as IScreenPresenter;

            if (screenPresenter == null)
            {
                Debug.LogError($"The {screenType.Name} screen presenter does not exist");
                return default;
            }

#if SCREEN_CLASSIC_ADDRESSABLE
            var loadedAsset = await Addressables.LoadAssetAsync<GameObject>(screenPresenter.ScreenPath);
#else
            var loadedAsset = await Resources.LoadAsync<GameObject>(screenPresenter.ScreenPath).ToUniTask() as GameObject;
#endif

            if (loadedAsset == null)
            {
                Debug.LogError($"Failed to load screen at path: {screenPresenter.ScreenPath}");
                return default;
            }

            var viewObject = UnityEngine.Object.Instantiate(loadedAsset, this.OpenedScreenParent);

            if (!viewObject.TryGetComponent<IScreenView>(out var viewInstance))
            {
                Debug.LogError($"The {screenPresenter.ScreenPath} does not have a view component");
                return default;
            }

            this.uisViewLoaded.Add(viewObject);

            ((IUiManagerAccess)screenPresenter).SetModel(model);
            screenPresenter.OnCloseView += () => this.CloseScreenAsync(screenPresenter.GetType(), openLastScreen: true).Forget();
            ((IUiManagerAccess)screenPresenter).SetView(viewInstance);
            this.screensPresenterLoaded[screenType] = screenPresenter;
            return (T)screenPresenter;
        }
        
        #endregion

        #region POPUP
        
        public async UniTask<TPresenter> OpenPopupAsync<TPresenter, TModel>(TModel model = default) where TPresenter : IPopupPresenter where TModel : IPopupModel
        {
            var presenter = await this.GetPopupAsync<TPresenter>(model);
            ((IUiManagerAccess)presenter).SetViewParent(this.OpenedScreenParent);
            await ((IUiManagerAccess)presenter).OpenViewAsync();
            return presenter;
        }

        public async UniTask ClosePopupAsync<TPresenter>() where TPresenter : IPopupPresenter
        {
            await this.ClosePopupAsync(typeof(TPresenter));
        }

        public async UniTask ClosePopupAsync(Type typePresenter)
        {
            if (!this.popupsPresenterLoaded.TryGetValue(typePresenter, out var presenter))
            {
                Debug.LogError($"The {typePresenter.Name} popup does not exist");
                return;
            }

            await ((IUiManagerAccess)presenter).CloseViewAsync();
            ((IUiManagerAccess)presenter).SetViewParent(this.ClosedScreenParent);
        }

        public async UniTask<T> GetPopupAsync<T>(IPopupModel model) where T : IPopupPresenter
        {
            var popupType = typeof(T);

            if (this.popupsPresenterLoaded.TryGetValue(popupType, out var popupPresenter))
            {
                ((IUiManagerAccess)popupPresenter).SetModel(model);
                return (T)popupPresenter;
            }

            popupPresenter = this.screenFactory.CreateUiPresenter<T>() as IPopupPresenter;

            if (popupPresenter == null)
            {
                Debug.LogError($"The {popupType.Name} popup presenter does not exist");
                return default;
            }

#if SCREEN_CLASSIC_ADDRESSABLE
            var loadedAsset = await Addressables.LoadAssetAsync<GameObject>(popupPresenter.PopupPath);
#else
            var loadedAsset = await Resources.LoadAsync<GameObject>(popupPresenter.PopupPath).ToUniTask() as GameObject;
#endif

            if (loadedAsset == null)
            {
                Debug.LogError($"Failed to load popup at path: {popupPresenter.PopupPath}");
                return default;
            }

            var viewObject = UnityEngine.Object.Instantiate(loadedAsset, this.OpenedScreenParent);

            if (!viewObject.TryGetComponent<IPopupView>(out var viewInstance))
            {
                Debug.LogError($"The {popupPresenter.PopupPath} does not have a view component");
                return default;
            }
            
            this.uisViewLoaded.Add(viewObject);

            ((IUiManagerAccess)popupPresenter).SetModel(model);
            popupPresenter.OnCloseView += () => this.ClosePopupAsync(typeof(T)).Forget();
            ((IUiManagerAccess)popupPresenter).SetView(viewInstance);
            this.popupsPresenterLoaded[popupType] = popupPresenter;
            return (T)popupPresenter;
        }

        #endregion
        
        public void CloseAllScreensAndPopups()
        {
            foreach (var screen in this.screensPresenterLoaded)
            {
                this.CloseScreenAsync(screen.Value.GetType(), openLastScreen: false).Forget();
            }

            foreach (var popup in this.popupsPresenterLoaded)
            {
                this.ClosePopupAsync(popup.Value.GetType()).Forget();
            }

            this.CurrentScreen = null;
        }

        private void FindScreensInScene()
        {
            var allScreens = this.OpenedScreenParent.GetComponentsInChildren<IScreenView>(true)
                .Concat(this.ClosedScreenParent.GetComponentsInChildren<IScreenView>(true)).ToArray();

            var allPopups = this.OpenedScreenParent.GetComponentsInChildren<IPopupView>(true)
                .Concat(this.ClosedScreenParent.GetComponentsInChildren<IPopupView>(true)).ToArray();

            var loadedViews = new HashSet<GameObject>(this.uisViewLoaded);

            var firstScreenType = GetFirstScreenType();

            foreach (var view in allScreens)
            {
                if (loadedViews.Contains(((MonoBehaviour)view).gameObject)) continue;
                InitializeScreenView(view, firstScreenType);
            }

            foreach (var view in allPopups)
            {
                InitializePopupView(view);
            }
        }

        private Type GetFirstScreenType()
        {
            return this.OpenedScreenParent.childCount > 0
                ? this.OpenedScreenParent.GetChild(0)
                    .GetComponent<IScreenView>()?
                    .GetType()
                    .GetCustomAttribute<ViewInitInSceneAttribute>(inherit: false)
                    ?.PresenterType
                : null;
        }

        private void InitializeScreenView(IScreenView view, Type firstScreenType)
        {
            var viewType = view.GetType();
            var attr = viewType.GetCustomAttribute<ViewInitInSceneAttribute>(inherit: false);

            if (attr == null)
            {
                Debug.LogError($"The {viewType.Name} does not have a ViewInitInSceneAttribute, skipping initialization.");
                return;
            }

            var presenterType = attr.PresenterType;
            var presenterInstance = this.screenFactory.CreateUiPresenter(presenterType);

            if (presenterInstance is not IScreenPresenter presenter)
            {
                Debug.LogError(presenterInstance == null
                    ? $"The {presenterType.Name} screen presenter does not exist for view {viewType.Name}"
                    : $"The {presenterType.Name} screen presenter is not a valid type for view {viewType.Name}");
                return;
            }

            this.screensPresenterLoaded[presenterType] = presenter;
            ((IUiManagerAccess)presenter).SetModel(null);
            ((IUiManagerAccess)presenter).SetView(view);

            presenter.OnCloseView += () => this.CloseScreenAsync(presenter.GetType(), openLastScreen: true).Forget();
            this.historyScreen.Add((presenter, null));

            if (firstScreenType != null && presenter.GetType() == firstScreenType)
            {
                ((IUiManagerAccess)presenter).OpenViewAsync().Forget();
                this.CurrentScreen = presenter;
            }
            else
            {
                _ = this.CloseScreenAsync(presenter.GetType(), false);
            }
        }

        private void InitializePopupView(IPopupView view)
        {
            var viewType = view.GetType();
            var attr = viewType.GetCustomAttribute<ViewInitInSceneAttribute>(inherit: false);

            if (attr == null)
            {
                Debug.LogError($"The {viewType.Name} does not have a ViewInitInSceneAttribute, skipping initialization.");
                return;
            }

            var presenterType = attr.PresenterType;
            var presenterInstance = this.screenFactory.CreateUiPresenter(presenterType);

            if (presenterInstance is not IPopupPresenter presenter)
            {
                Debug.LogError(presenterInstance == null
                    ? $"The {presenterType.Name} popup presenter does not exist for view {viewType.Name}"
                    : $"The {presenterType.Name} popup presenter is not a valid type for view {viewType.Name}");
                return;
            }

            this.popupsPresenterLoaded[presenterType] = presenter;
            ((IUiManagerAccess)presenter).SetModel(null);
            ((IUiManagerAccess)presenter).SetView(view);

            presenter.OnCloseView += () => this.ClosePopupAsync(presenter.GetType()).Forget();
            _ = this.ClosePopupAsync(presenter.GetType());
        }
    }
}