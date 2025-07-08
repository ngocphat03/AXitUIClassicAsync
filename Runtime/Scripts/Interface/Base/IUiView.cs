namespace AXitUnityTemplate.UI.Classic.Async
{
    using System;
    using UnityEngine;
    using Cysharp.Threading.Tasks;

    public interface IUiView
    {
        public RectTransform RectTransform { get; }
        public event Action  OnViewReady;
        public event Action  OnOpen;
        public event Action  OnClose;
        public event Action  OnDestroy;

        public UniTask Open();

        public UniTask Close();

        public void DestroySelf();
    }
}