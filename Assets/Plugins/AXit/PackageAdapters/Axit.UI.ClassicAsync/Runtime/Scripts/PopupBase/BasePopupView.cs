namespace AxitUnityTemplate.UI.Classic.Async
{
    using System;
    using UnityEngine;
    using Cysharp.Threading.Tasks;

    [RequireComponent(typeof(CanvasGroup), typeof(UiTransition))]
    public class BasePopupView : MonoBehaviour, IPopupView
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

        public async UniTask OpenAsync()
        {
            this.UpdateAlpha(1f);
            await this.UiTransition.PlayIntroAnimationAsync();
            this.OnOpen?.Invoke();
        }

        public async UniTask CloseAsync()
        {
            await this.UiTransition.PlayOutroAnimationAsync();
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
        
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            this.UiTransition = this.GetComponent<UiTransition>();
            this.RectTransform = this.GetComponent<RectTransform>();
            this.ViewRoot = this.GetComponent<CanvasGroup>();
        }
#endif
    }
}