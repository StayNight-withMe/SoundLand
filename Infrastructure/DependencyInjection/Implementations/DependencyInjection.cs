// Infrastructure/DependencyInjection.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Threading;

public class DependencyInjection
{
    private Dictionary<Type, Type> _registrations = new Dictionary<Type, Type>();
    private Dictionary<Type, object> _singletons = new Dictionary<Type, object>();
    private readonly Dispatcher _uiDispatcher;

    public DependencyInjection(Dispatcher uiDispatcher)
    {
        _uiDispatcher = uiDispatcher ?? throw new ArgumentNullException(nameof(uiDispatcher));
    }

 
    public void Register<TInterface, TImplementation>()
        where TImplementation : class, TInterface
    {
        _registrations[typeof(TInterface)] = typeof(TImplementation);
    }


    public void RegisterSingleton<TInterface, TImplementation>()
        where TImplementation : class, TInterface
    {
        _registrations[typeof(TInterface)] = typeof(TImplementation);
        _singletons[typeof(TInterface)] = null;  
    }

 
    public void RegisterInstance<T>(T instance) where T : class
    {
        _registrations[typeof(T)] = instance.GetType();
        _singletons[typeof(T)] = instance;  
    }

    public T Resolve<T>() where T : class
    {
        return (T)GetDependency(typeof(T));
    }

    private object GetDependency(Type serviceType)
    {
        Type concreteType;
        
        if (_singletons.ContainsKey(serviceType))
        {
        
            if (_singletons[serviceType] == null)
            {
                 concreteType = _registrations[serviceType];
                _singletons[serviceType] = CreateInstance(concreteType);
            }
            return _singletons[serviceType];
        }

        
    
        if (_registrations.TryGetValue(serviceType, out Type implementationType))
        {
            concreteType = implementationType;
        }
        else
        {
            concreteType = serviceType;
        }

        
        if (concreteType == typeof(Dispatcher))
        {
            return _uiDispatcher;
        }

        return CreateInstance(concreteType);
    }

    private object CreateInstance(Type type)
    {
        
        if (type.IsAbstract || type.IsInterface)
        {
            throw new InvalidOperationException($"Нельзя создать экземпляр абстрактного класса или интерфейса: {type.FullName}");
        }

        
        if (type == typeof(Dispatcher))
        {
            return _uiDispatcher;
        }

        ConstructorInfo constructor = type.GetConstructors()
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault();

        if (constructor == null)
        {
            // Для типов без конструкторов:
            return Activator.CreateInstance(type);
        }

        ParameterInfo[] parameters = constructor.GetParameters();
        object[] args = new object[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            Type paramType = parameters[i].ParameterType;

            
            if (paramType == typeof(Dispatcher))
            {
                args[i] = _uiDispatcher;
            }
            else
            {
                args[i] = GetDependency(paramType);
            }
        }

        return constructor.Invoke(args);
    }
}