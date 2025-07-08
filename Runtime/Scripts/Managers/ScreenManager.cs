namespace AXitUnityTemplate.UI.Classic.Async
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

    public class ScreenManager : MonoBehaviour
    {
        [field: SerializeField] public Transform OpenedScreenParent { get; private set; }

        [field: SerializeField] public Transform ClosedScreenParent { get; private set; }

#if ZENJECT
#elif VCONTAINER
        [VContainer.Inject] private ScreenFactory screenFactory;
#else
        private readonly ScreenFactory screenFactory = new();
#endif

        public static ScreenManager Instance { get; private set; }

        private readonly Dictionary<Type, IScreenPresenter> screensPresenterLoaded = new(10);

        private IScreenPresenter CurrentScreen { get; set; }

        private readonly List<(IScreenPresenter screen, IScreenPresenter lastScreen)> historyScreen = new();

        private readonly Dictionary<Type, IPopupPresenter> popupsPresenterLoaded = new(10);

        public static Func<ScreenManager> Resolve = () => Instance;

        private void Awake()
        {
            if (ScreenManager.Instance != null && ScreenManager.Instance != this)
            {
                Debug.LogWarning("Multiple instances of ScreenManager detected. Destroying the new instance.");
                GameObject.Destroy(this.gameObject);
                return;
            }

            ScreenManager.Instance = this;
        }

        private void Start()
        {
            this.FindScreensInScene();
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
        
        public async UniTask<TPresenter> OpenScreen<TPresenter, TModel>(TModel model = default) where TPresenter : IScreenPresenter where TModel : IScreenModel
        {
            await this.CloseScreen(this.CurrentScreen?.GetType(), openLastScreen: false);

            var presenter = await this.GetScreen<TPresenter>();

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

            presenter.SetViewParent(this.OpenedScreenParent);
            this.CurrentScreen = presenter;
            this.CurrentScreen.SetModel(model);
            await this.CurrentScreen.OpenView();
            return (TPresenter)this.CurrentScreen;
        }

        private async UniTask CloseScreen(Type typeScreenPresenter, bool openLastScreen = true)
        {
            if (typeScreenPresenter == null) return;

            if (!this.screensPresenterLoaded.TryGetValue(typeScreenPresenter, out var presenter))
            {
                Debug.LogError($"The {typeScreenPresenter.Name} screen does not exist");
                return;
            }

            await presenter.CloseView();
            presenter.SetViewParent(this.ClosedScreenParent);
            
            if (openLastScreen) return;
            
            var lastScreen = this.historyScreen.Find(x => x.screen.GetType() == typeScreenPresenter).lastScreen;

            if (lastScreen != null)
            {
                this.CurrentScreen = lastScreen;
                if (this.CurrentScreen == null) return;

                this.CurrentScreen.SetViewParent(this.OpenedScreenParent);
                await this.CurrentScreen.OpenView();
            }
        }

        public async UniTask<T> GetScreen<T>() where T : IScreenPresenter
        {
            var screenType = typeof(T);

            if (this.screensPresenterLoaded.TryGetValue(screenType, out var screenPresenter))
            {
                return (T)screenPresenter;
            }

            screenPresenter = this.screenFactory.CreateUiPresenter<T>() as IScreenPresenter;

            if (screenPresenter == null)
            {
                Debug.LogError($"The {screenType.Name} screen presenter does not exist");
                return default;
            }

#if SCREEN_CLASSIC_ADDRESSABLE
            var loadOperation = Addressables.LoadAssetAsync<GameObject>(screenPresenter.ScreenPath);
            await loadOperation.ToUniTask();
            var loadedAsset = loadOperation.Result;
#else
            var loadOperation = Resources.LoadAsync<GameObject>(screenPresenter.ScreenPath);
            await loadOperation.ToUniTask();
            var loadedAsset = loadOperation.asset as GameObject;
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

            screenPresenter.OnCloseView += () => this.CloseScreen(screenPresenter.GetType(), openLastScreen: true).Forget();
            screenPresenter.SetView(viewInstance);
            this.screensPresenterLoaded[screenType] = screenPresenter;
            return (T)screenPresenter;
        }
        
        #endregion

        #region POPUP
        
        public async UniTask<TPresenter> OpenPopup<TPresenter, TModel>(TModel model = default) where TPresenter : IPopupPresenter where TModel : IPopupModel
        {
            var presenter = await this.GetPopup<TPresenter>();
            presenter.SetViewParent(this.OpenedScreenParent);
            presenter.SetModel(model);
            await presenter.OpenView();
            return presenter;
        }

        public async UniTask ClosePopup<TPresenter>() where TPresenter : IPopupPresenter
        {
            await this.ClosePopup(typeof(TPresenter));
        }

        public async UniTask ClosePopup(Type typePresenter)
        {
            if (!this.popupsPresenterLoaded.TryGetValue(typePresenter, out var presenter))
            {
                Debug.LogError($"The {typePresenter.Name} popup does not exist");
                return;
            }

            await presenter.CloseView();
            presenter.SetViewParent(this.ClosedScreenParent);
        }

        public async UniTask<T> GetPopup<T>() where T : IPopupPresenter
        {
            var popupType = typeof(T);

            if (this.popupsPresenterLoaded.TryGetValue(popupType, out var popupPresenter))
            {
                return (T)popupPresenter;
            }

            popupPresenter = this.screenFactory.CreateUiPresenter<T>() as IPopupPresenter;

            if (popupPresenter == null)
            {
                Debug.LogError($"The {popupType.Name} popup presenter does not exist");
                return default;
            }

#if SCREEN_CLASSIC_ADDRESSABLE
            var loadOperation = Addressables.LoadAssetAsync<GameObject>(popupPresenter.PopupPath);
            await loadOperation.ToUniTask();
            var loadedAsset = loadOperation.Result;
#else
            var loadOperation = Resources.LoadAsync<GameObject>(popupPresenter.PopupPath);
            await loadOperation.ToUniTask();
            var loadedAsset = loadOperation.asset as GameObject;
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

            popupPresenter.OnCloseView += () => this.ClosePopup(typeof(T)).Forget();
            popupPresenter.SetView(viewInstance);
            this.popupsPresenterLoaded[popupType] = popupPresenter;
            return (T)popupPresenter;
        }

        #endregion
        
        public void CloseAllScreensAndPopups()
        {
            foreach (var screen in this.screensPresenterLoaded)
            {
                this.CloseScreen(screen.Value.GetType(), openLastScreen: false).Forget();
            }

            foreach (var popup in this.popupsPresenterLoaded)
            {
                this.ClosePopup(popup.Value.GetType()).Forget();
            }

            this.CurrentScreen = null;
        }

        private void FindScreensInScene()
        {
            var allScreens = this.OpenedScreenParent.GetComponentsInChildren<IScreenView>(true)
                                 .Concat(this.ClosedScreenParent.GetComponentsInChildren<IScreenView>(true)).ToArray();

            var allPopups = this.OpenedScreenParent.GetComponentsInChildren<IPopupView>(true)
                                .Concat(this.ClosedScreenParent.GetComponentsInChildren<IPopupView>(true)).ToArray();

            var firstScreen = this.OpenedScreenParent.childCount > 0
                ? this.OpenedScreenParent.GetChild(0)
                      .GetComponent<IScreenView>()
                      .GetType()
                      .GetCustomAttribute<ViewInitInSceneAttribute>(inherit: false)
                      ?.PresenterType
                : null;

            foreach (var view in allScreens)
            {
                var viewType = view.GetType();
                var attr     = viewType.GetCustomAttribute<ViewInitInSceneAttribute>(inherit: false);

                if (attr == null)
                {
                    Debug.LogError($"The {viewType.Name} does not have a ViewInitInSceneAttribute, skipping initialization.");
                    continue;
                }

                var presenterType   = attr.PresenterType;
                var screenPresenter = this.screenFactory.CreateUiPresenter(presenterType);

                switch (screenPresenter)
                {
                    case null:
                        Debug.LogError($"The {presenterType.Name} screen presenter does not exist for view {viewType.Name}");
                        continue;
                    case IScreenPresenter presenter:
                        this.screensPresenterLoaded[presenterType] = presenter;
                        presenter.SetModel(null);

                        presenter.OnCloseView += () => this.CloseScreen(presenter.GetType(), openLastScreen: true).Forget();
                        presenter.SetView(view);
                        this.historyScreen.Add((presenter, null));

                        if (firstScreen != null && screenPresenter.GetType() == firstScreen)
                        {
                            presenter.OpenView().Forget();
                            this.CurrentScreen = presenter;
                        }

                        break;
                    default:
                        Debug.LogError($"The {presenterType.Name} screen presenter is not a valid type for view {viewType.Name}");
                        break;
                }
            }

            foreach (var view in allPopups)
            {
                var viewType = view.GetType();
                var attr     = viewType.GetCustomAttribute<ViewInitInSceneAttribute>(inherit: false);

                if (attr == null)
                {
                    Debug.LogError($"The {viewType.Name} does not have a ViewInitInSceneAttribute, skipping initialization.");
                    continue;
                }

                var presenterType  = attr.PresenterType;
                var popupPresenter = this.screenFactory.CreateUiPresenter(presenterType);

                switch (popupPresenter)
                {
                    case null:
                        Debug.LogError($"The {presenterType.Name} popup presenter does not exist for view {viewType.Name}");
                        continue;
                    case IPopupPresenter presenter:
                        this.popupsPresenterLoaded[presenterType] = presenter;
                        presenter.SetModel(null);

                        presenter.OnCloseView += () => this.ClosePopup(presenter.GetType()).Forget();
                        presenter.SetView(view);

                        break;
                    default:
                        Debug.LogError($"The {presenterType.Name} popup presenter is not a valid type for view {viewType.Name}");
                        break;
                }
            }
        }
    }
}