namespace AXitUnityTemplate.UI.Classic.Async
{
    using System;
    using UnityEngine;

    public interface IUiPresenter
    {
        public EUiStatus EUiStatus { get; }

        public Transform CurrentTransform { get; }
        
        public Action OnCloseView { get; set; }
        
        /// <summary>
        /// Called when the view is initialized.
        /// </summary>
        public virtual void Awake(){}

        /// <summary>
        /// Called when the view is enabled.
        /// </summary>
        public virtual void OnEnable(){}

        /// <summary>
        /// Called when the view is disabled.
        /// </summary>
        public virtual void OnDisable(){}

        /// <summary>
        /// Called when the view is destroyed.
        /// </summary>
        public virtual void OnDestroy(){}
    }
}