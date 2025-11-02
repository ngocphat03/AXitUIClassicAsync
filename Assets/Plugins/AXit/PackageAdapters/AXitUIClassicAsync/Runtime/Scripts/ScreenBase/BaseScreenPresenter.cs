namespace AXitUnityTemplate.UI.Classic.Async
{
    using System;
    using UnityEngine;
    using Cysharp.Threading.Tasks;

    public abstract class BaseScreenPresenter<TView, TModel> : IScreenPresenter, IUiManagerAccess where TView : BaseScreenView where TModel : BaseScreenModel, new()
    {
        private Action<IUiPresenter> onCloseView;

        public EUiStatus EUiStatus { get; private set; }

        public Action OnCloseView { get; set; }

        public TView View { get; private set; }

        public TModel Model { get; private set; }

        public Transform CurrentTransform => (this.View as GameObject)?.transform
                                          ?? throw new Exception("View is not a game object");

        public abstract string ScreenPath { get; }

        void IUiManagerAccess.SetViewParent(Transform parent)
        {
            this.View.transform.SetParent(parent);
        }

        void IUiManagerAccess.SetView(object viewInstance)
        {
            this.View = viewInstance as TView;

            if (!this.View) throw new Exception("View is not of type TView");

            this.View.Init();
            this.Awake();
        }

        async UniTask IUiManagerAccess.OpenViewAsync()
        {
            if (this.EUiStatus is EUiStatus.Opened or EUiStatus.Opening)
            {
                Debug.LogWarning("Screen is already opened");
                return;
            }

            this.View.ViewRoot.blocksRaycasts = true;
            this.EUiStatus                    = EUiStatus.Opening;
            this.OnEnable();
            await this.View.OpenAsync();
            this.EUiStatus = EUiStatus.Opened;
        }

        async UniTask IUiManagerAccess.CloseViewAsync()
        {
            if (this.EUiStatus is EUiStatus.Closed or EUiStatus.Closing)
            {
                Debug.LogWarning("Screen is already closed");

                return;
            }

            this.View.ViewRoot.blocksRaycasts = false;
            this.EUiStatus                    = EUiStatus.Closing;

            await this.View.CloseAsync();
            this.EUiStatus = EUiStatus.Closed;
            this.OnDisable();
        }

        void IUiManagerAccess.SetModel(object modelObject)
        {
            switch (modelObject)
            {
                case null:
                    if (this.Model != null) break;
                    
                    this.Model = new TModel();
                    break;
                case TModel tModel:
                    this.Model = tModel;
                    break;
                default:
                    Debug.LogError($"Model object is not of type {typeof(TModel).Name}. Expected: {typeof(TModel).Name}, Received: {modelObject.GetType().Name}");
                    return;
            }
        }

        public virtual void Awake(){}

        public virtual void OnEnable(){}

        public virtual void OnDisable(){}

        public virtual void OnDestroy(){}
    }
}