using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Composing;

namespace Our.Umbraco.Container.Castle
{
    public class CastleContainer : IContainer
    {
        public object ConcreteContainer { get; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public object GetInstance(Type type)
        {
            throw new NotImplementedException();
        }

        public object GetInstance(Type type, string name)
        {
            throw new NotImplementedException();
        }

        public object TryGetInstance(Type type)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TService> GetAllInstances<TService>()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Registration> GetRegistered(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public object CreateInstance(Type type, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Register(Type serviceType, Lifetime lifetime = Lifetime.Transient)
        {
            throw new NotImplementedException();
        }

        public void Register(Type serviceType, Type implementingType, Lifetime lifetime = Lifetime.Transient)
        {
            throw new NotImplementedException();
        }

        public void Register(Type serviceType, Type implementingType, string name, Lifetime lifetime = Lifetime.Transient)
        {
            throw new NotImplementedException();
        }

        public void Register<TService>(Func<IContainer, TService> factory, Lifetime lifetime = Lifetime.Transient)
        {
            throw new NotImplementedException();
        }

        public void RegisterInstance(Type serviceType, object instance)
        {
            throw new NotImplementedException();
        }

        public void RegisterAuto(Type serviceBaseType)
        {
            throw new NotImplementedException();
        }

        public void RegisterOrdered(Type serviceType, Type[] implementingTypes, Lifetime lifetime = Lifetime.Transient)
        {
            throw new NotImplementedException();
        }

        public IDisposable BeginScope()
        {
            throw new NotImplementedException();
        }

        public IContainer ConfigureForWeb()
        {
            throw new NotImplementedException();
        }

        public IContainer EnablePerWebRequestScope()
        {
            throw new NotImplementedException();
        }
    }
}
