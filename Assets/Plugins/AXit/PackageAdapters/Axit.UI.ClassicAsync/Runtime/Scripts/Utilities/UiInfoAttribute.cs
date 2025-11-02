namespace AxitUnityTemplate.UI.Classic.Async
{
    using System;

    /// <summary>
    /// Marks a View class that should be initialized within the Scene.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ViewInitInSceneAttribute : Attribute
    {
        /// <summary>
        /// Gets the Presenter type linked to the View decorated with this attribute.
        /// </summary>
        public Type PresenterType { get; }
    
        /// <summary>
        /// Initializes the attribute with the required Presenter type.
        /// </summary>
        /// <param name="presenterType">The type of the Presenter associated with this View.</param>
        public ViewInitInSceneAttribute(Type presenterType)
        {
            this.PresenterType = presenterType;
        }
    }
}