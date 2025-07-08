namespace AXitUnityTemplate.UI.Classic.Async
{
    using System;
    using Cysharp.Threading.Tasks;
    using UnityEngine;

    public interface IUiPresenter
    {
        public EUiStatus EUiStatus { get; }

        public Transform CurrentTransform { get; }
        public Action OnCloseView { get; set; }

        public void SetViewParent(Transform parent);

        public UniTask OpenView();

        public UniTask CloseView();

        public void SetModel(object modelObject);

        public void Awake();

        public void OnEnable();

        public void OnDisable();

        public void OnDestroy();
    }
}