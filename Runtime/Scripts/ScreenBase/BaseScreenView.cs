namespace AXitUnityTemplate.UI.Classic.Async
{
    using System;
    using UnityEngine;
    using Cysharp.Threading.Tasks;

    [RequireComponent(typeof(CanvasGroup), typeof(UiTransition))]
    public class BaseScreenView : MonoBehaviour, IScreenView
    {

        #region Public Properties

        [field: SerializeField] public CanvasGroup ViewRoot { get; protected set; }

        [field: SerializeField] public UiTransition UiTransition { get; protected set; }

        public RectTransform RectTransform { get; private set; }

        public bool blockRaycastHit = true;

        public event Action OnViewReady;

        public event Action OnOpen;

        public event Action OnClose;

        public event Action OnDestroy;

        #endregion

        public void Init()
        {
            if (!this.ViewRoot) this.ViewRoot           = this.GetComponent<CanvasGroup>();
            if (!this.UiTransition) this.UiTransition   = this.transform.GetComponent<UiTransition>();
            if (!this.RectTransform) this.RectTransform = this.GetComponent<RectTransform>();

            this.OnViewReady?.Invoke();
        }

        public async UniTask Open()
        {
            this.UpdateAlpha(1f);

            await this.UiTransition.PlayIntroAnimation();

            this.OnOpen?.Invoke();
        }

        public async UniTask Close()
        {
            await this.UiTransition.PlayOutroAnimation();

            this.UpdateAlpha(0);
            this.OnClose?.Invoke();
        }

        public void DestroySelf()
        {
            this.OnDestroy?.Invoke();
            UnityEngine.Object.Destroy(this.gameObject);
        }

        private void UpdateAlpha(float value)
        {
            if (!this.ViewRoot) return;
            this.ViewRoot.alpha          = value;
            this.ViewRoot.blocksRaycasts = this.blockRaycastHit && value >= 1;
        }
    }
}