using System;
using System.Linq;
using AXitUnityTemplate.UI.Classic.Async;

#if VCONTAINER
using VContainer;
#endif

public class ScreenFactory
{
#if ZENJECT
    private readonly DiContainer diContainer;

    public ScreenFactory(DiContainer diContainer)
    {
        this.diContainer = diContainer;
    }
#elif VCONTAINER
    private readonly IObjectResolver resolver;

    public ScreenFactory(IObjectResolver resolver)
    {
        this.resolver = resolver;
    }
#else
    public ScreenFactory()
    {
    }
#endif

    public IUiPresenter CreateUiPresenter(Type screenPresenterType)
    {
#if ZENJECT
        return this.diContainer.Instantiate(screenPresenterType) as IUiPresenter;

#elif VCONTAINER
        var constructor = screenPresenterType
                          .GetConstructors()
                          .OrderByDescending(c => c.GetParameters().Length)
                          .FirstOrDefault();

        if (constructor == null)
        {
            throw new InvalidOperationException($"No public constructor found for {screenPresenterType.FullName}");
        }

        var parameters = constructor.GetParameters()
                                    .Select(p => this.resolver.Resolve(p.ParameterType))
                                    .ToArray();

        var instance = constructor.Invoke(parameters) as IUiPresenter;

        if (instance == null)
        {
            throw new InvalidOperationException($"Failed to instantiate {screenPresenterType.FullName}");
        }

        return instance;

#else
        return Activator.CreateInstance(screenPresenterType) as IUiPresenter;
#endif
    }

    public IUiPresenter CreateUiPresenter<TPresenter>() where TPresenter : IUiPresenter
    {
#if ZENJECT
        return this.diContainer.Instantiate<TPresenter>();

#elif VCONTAINER
        var screenPresenterType = typeof(TPresenter);

        var constructor = screenPresenterType
                          .GetConstructors()
                          .OrderByDescending(c => c.GetParameters().Length)
                          .FirstOrDefault();

        if (constructor == null)
        {
            throw new InvalidOperationException($"No public constructor found for {screenPresenterType.FullName}");
        }

        var parameters = constructor.GetParameters()
                                    .Select(p => this.resolver.Resolve(p.ParameterType))
                                    .ToArray();

        var instance = constructor.Invoke(parameters) as IUiPresenter;

        if (instance == null)
        {
            throw new InvalidOperationException($"Failed to instantiate {screenPresenterType.FullName}");
        }

        return instance;
#else
        return Activator.CreateInstance<TPresenter>();
#endif
    }
#if VCONTAINER
    
    public sealed class Parameter : IInjectParameter
    {
        private readonly object value;

        public Parameter(object value)
        {
            this.value = value;
        }

        bool IInjectParameter.Match(Type parameterType, string _)
        {
            return parameterType.IsInstanceOfType(this.value);
        }

        object IInjectParameter.GetValue(IObjectResolver _)
        {
            return this.value;
        }
    }

#endif
    
}