namespace AXitUnityTemplate.UI.Classic.Async
{
#if VCONTAINER
    using UnityEngine;
    using VContainer;

    public class ScreenInstaller
    {
        public static void Install(IContainerBuilder builder)
        {
            builder.Register<ScreenFactory>(Lifetime.Singleton);
            
            var sceneManagerObject = Object.FindFirstObjectByType(typeof(ScreenManager));

            builder.RegisterInstance(sceneManagerObject)
                   .As<ScreenManager>();
        }
    }
#endif
}