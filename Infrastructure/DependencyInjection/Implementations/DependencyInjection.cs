using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace test.Infrastructure
{
    public class DependencyInjection<T> 
    {

        private Dictionary<Type, Type> DependencyType = new Dictionary<Type, Type>();

        private object[] DependencyObj;

        public void Register<TInterface, TImplementation>()
     where TImplementation : class, TInterface
        {
            DependencyType.Add(typeof(TInterface),typeof(TImplementation));
        }


        public TService Resolve<TService>()
        {
          
            return (TService)GetDependency(typeof(TService));
        }

        public object GetDependency(Type serviceType)
        {

            Type concreteType;


            if (DependencyType.TryGetValue(serviceType, out Type implementationType))
            {
                concreteType = implementationType; // Нашли реализацию
            }
            else
            {
               
                concreteType = serviceType;
            }

            ConstructorInfo constructor = concreteType.GetConstructors().FirstOrDefault();

            if (constructor == null) { throw new InvalidOperationException(); }

            ParameterInfo[] parameters = constructor.GetParameters();


            List<object> resolvedDependencies = new List<object>();


            foreach (ParameterInfo param in parameters)
            {
             
                object dependencyInstance = GetDependency(param.ParameterType);
                resolvedDependencies.Add(dependencyInstance);
            }

  
            return constructor.Invoke(resolvedDependencies.ToArray());



           
        }


    }
}
