namespace AXitUnityTemplate.UI.Classic.Async
{
    using UnityEngine;
    using Cysharp.Threading.Tasks;

    /// <summary>
    /// Internal interface for ScreenManager access only.
    /// These methods should only be called by ScreenManager.
    /// </summary>
    internal interface IUiManagerAccess
    {
        /// <summary>
        /// Sets the view parent transform.
        /// IMPORTANT: This method should only be called from ScreenManager.
        /// </summary>
        void SetViewParent(Transform parent);

        /// <summary>
        /// Sets the view instance and initializes it.
        /// IMPORTANT: This method should only be called from ScreenManager.
        /// </summary>
        void SetView(object viewInstance);

        /// <summary>
        /// Opens the view.
        /// IMPORTANT: This method should only be called from ScreenManager.
        /// </summary>
        UniTask OpenViewAsync();

        /// <summary>
        /// Closes the view.
        /// IMPORTANT: This method should only be called from ScreenManager.
        /// </summary>
        UniTask CloseViewAsync();

        /// <summary>
        /// Sets the model for the presenter.
        /// IMPORTANT: This method should only be called from ScreenManager.
        /// </summary>
        void SetModel(object modelObject);
    }
}
