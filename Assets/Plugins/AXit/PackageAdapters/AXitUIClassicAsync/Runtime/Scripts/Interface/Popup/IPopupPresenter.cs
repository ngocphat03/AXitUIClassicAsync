namespace AXitUnityTemplate.UI.Classic.Async
{
    using System;

    public interface IPopupPresenter : IUiPresenter
    {
        public string PopupPath { get; }

        public void SetView(IPopupView viewInstance, Action<IUiPresenter> onClose = null);
    }
}