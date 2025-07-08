namespace AXitUnityTemplate.UI.Classic.Async
{
    using System;
    using UnityEngine;
    using Cysharp.Threading.Tasks;

    public abstract class BaseScreenPresenter<TView, TModel> : IScreenPresenter where TView : BaseScreenView where TModel : BaseScreenModel, new()
    {
        private Action<IUiPresenter> onCloseView;

        public EUiStatus EUiStatus { get; private set; }

        public Action OnCloseView { get; set; }

        public TView View { get; private set; }

        public TModel Model { get; private set; }

        public Transform CurrentTransform => (this.View as GameObject)?.transform
                                          ?? throw new Exception("View is not a game object");

        public abstract string ScreenPath { get; }

        public void SetViewParent(Transform parent) { this.View.transform.SetParent(parent); }

        public void SetView(IScreenView viewInstance, Action<IUiPresenter> onClose = null)
        {
            this.View = viewInstance as TView;

            if (!this.View) throw new Exception("View is not of type TView");

            this.View.Init();
            this.Awake();
        }

        public async UniTask OpenView()
        {
            if (this.EUiStatus is EUiStatus.Opened or EUiStatus.Opening)
            {
                Debug.LogWarning("Screen is already opened");
                return;
            }

            this.View.ViewRoot.blocksRaycasts = true;
            this.EUiStatus                    = EUiStatus.Opening;
            this.OnEnable();
            await this.View.Open();
            this.EUiStatus = EUiStatus.Opened;
        }

        public async UniTask CloseView()
        {
            if (this.EUiStatus is EUiStatus.Closed or EUiStatus.Closing)
            {
                Debug.LogWarning("Screen is already closed");

                return;
            }

            this.View.ViewRoot.blocksRaycasts = false;
            this.EUiStatus                    = EUiStatus.Closing;

            await this.View.Close();
            this.EUiStatus = EUiStatus.Closed;
            this.OnDisable();
        }

        public void SetModel(object modelObject)
        {
            switch (modelObject)
            {
                case null:
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

        public abstract void Awake();

        public abstract void OnEnable();

        public abstract void OnDisable();

        public abstract void OnDestroy();
    }
}