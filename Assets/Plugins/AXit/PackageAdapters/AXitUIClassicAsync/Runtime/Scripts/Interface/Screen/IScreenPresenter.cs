namespace AXitUnityTemplate.UI.Classic.Async
{
    using System;

    public interface IScreenPresenter : IUiPresenter
    {
        public string ScreenPath { get; }

        public void SetView(IScreenView viewInstance, Action<IUiPresenter> onClose = null);
    }
}