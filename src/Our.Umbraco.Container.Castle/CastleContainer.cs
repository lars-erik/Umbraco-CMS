using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Lifestyle;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Castle.Windsor.Diagnostics.Helpers;
using Castle.Windsor.Installer;
using Umbraco.Core.Components;
using Umbraco.Core.Composing;
using Umbraco.Web.Mvc;
using IDependencyResolver = System.Web.Mvc.IDependencyResolver;

namespace Our.Umbraco.Container.Castle
{
    public class CastleContainer : IContainer
    {
        // fixme - creates a cyclic dependency with castle
        public static IContainer Create()
        {
            var castleContainer = new CastleContainer();
            return castleContainer;
        }

        private Dictionary<Lifetime, LifestyleType> lifetimes = new Dictionary<Lifetime, LifestyleType>
        {
            { Lifetime.Request, LifestyleType.PerWebRequest },
            { Lifetime.Scope, LifestyleType.Scoped },
            { Lifetime.Singleton, LifestyleType.Singleton },
            { Lifetime.Transient, LifestyleType.Transient },
        };

        private WindsorContainer container;

        public object ConcreteContainer => container;

        public CastleContainer()
        {
            container = new WindsorContainer();
            container.Register(Component.For<ILazyComponentLoader>().ImplementedBy<LazyOfTComponentLoader>());
            container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel));
        }

        public void Dispose()
        {
            container.Dispose();
        }

        public object GetInstance(Type type)
        {
            return container.Resolve(type);
        }

        public object GetInstance(Type type, string name)
        {
            return container.Resolve(name, type);
        }

        // fixme - figure out what castle actually does vs. xml doc
        public object TryGetInstance(Type type)
        {
            return container.Resolve(type);
        }

        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return container.ResolveAll(serviceType).Cast<object>();
        }

        public IEnumerable<TService> GetAllInstances<TService>()
        {
            return container.ResolveAll<TService>();
        }

        // fixme - Translate to Registration object
        public IEnumerable<Registration> GetRegistered(Type serviceType)
        {
            return container.Kernel.GetAssignableHandlers(serviceType)
                .Select(x => new Registration(serviceType, x.ComponentModel.Name)
            );
        }

        // fixme - Refactor to use Dictionary. Castle does not support nameless parameters. !?!?
        public object CreateInstance(Type type, IDictionary<string, object> args)
        {
            return container.Resolve(type, args);
        }

        public void Register(Type serviceType, Lifetime lifetime = Lifetime.Transient)
        {
            container.Register(
                Component
                    .For(serviceType)
                    .LifeStyle.Is(lifetimes[lifetime])
            );
        }

        public void Register(Type serviceType, Type implementingType, Lifetime lifetime = Lifetime.Transient)
        {
            container.Register(
                Component
                    .For(serviceType)
                    .ImplementedBy(implementingType)
                    .LifeStyle.Is(lifetimes[lifetime])
            );
        }

        public void Register(Type serviceType, Type implementingType, string name, Lifetime lifetime = Lifetime.Transient)
        {
            container.Register(
                Component
                    .For(serviceType)
                    .Named(name)
                    .ImplementedBy(implementingType)
                    .LifeStyle.Is(lifetimes[lifetime])
            );
        }

        // fixme - Must make TService reference type in interface
        // fixme - Umbraco re-registers IProfiler for instance. Must set new registrations as default. (?)
        public void Register<TService>(Func<IContainer, TService> factory, Lifetime lifetime = Lifetime.Transient)
        {
            container.Register(
                Component
                    .For(typeof(TService))
                    .UsingFactory<IContainer, TService>(f => factory(this))
                    .LifeStyle.Is(lifetimes[lifetime])
                    .IsDefault()
            );

        }

        // fixme - Must make TService reference type in interface
        // fixme - Umbraco re-registers IProfiler for instance. Must set new registrations as default. (?)
        public void Register<TService>(Func<IContainer, TService> factory, string name, Lifetime lifetime = Lifetime.Transient)
        {
            container.Register(
                Component
                    .For(typeof(TService))
                    .UsingFactory<IContainer, TService>(f => factory(this))
                    .Named(name)
                    .LifeStyle.Is(lifetimes[lifetime])
                    .IsDefault()
            );

        }

        public void RegisterInstance(Type serviceType, object instance)
        {
            container.Register(
                Component
                    .For(serviceType)
                    .Instance(instance));
        }

        public void RegisterAuto(Type serviceBaseType)
        {
            container.Register(
                Classes
                .FromAssemblyContaining(serviceBaseType)
                .BasedOn(serviceBaseType)
            );
        }

        // fixme Figure out if Castle returns things in same order as registered
        public void RegisterOrdered(Type serviceType, Type[] implementingTypes, Lifetime lifetime = Lifetime.Transient)
        {
            foreach(var type in implementingTypes)
            { 
                container.Register(
                    Component
                        .For(serviceType)
                        .ImplementedBy(type)
                        .LifeStyle.Is(lifetimes[lifetime])
                );
            }
        }

        public IDisposable BeginScope()
        {
            return container.BeginScope();
        }

        // fixme - Figure out requirements
        public IContainer ConfigureForWeb()
        {

            //Register<IFilteredControllerFactory>(x => new WindsorControllerFactory(this), Lifetime.Singleton);

            return this;
        }

        // fixme - Figure out if this is on by default and how to set it.
        public IContainer EnablePerWebRequestScope()
        {
            return this;
        }
    }

    public class CastleWindsorComponent : IUmbracoUserComponent
    {
        public void Compose(Composition composition)
        {
            
        }

        public Type InitializerType => typeof(CastleInitializer);

        public class CastleInitializer : IComponentInitializer
        {
            private readonly IContainer container;

            public CastleInitializer(IContainer container)
            {
                this.container = container;
            }

            public void Initialize()
            {
                container.GetInstance<FilteredControllerFactoryCollectionBuilder>()
                    .Insert<WindsorControllerFactory>();
            }
        }

        public void Terminate()
        {
        }
    }

    public class WindsorDependencyResolver : IDependencyResolver
    {
        private readonly WindsorContainer container;

        public WindsorDependencyResolver()
        {
            container = (WindsorContainer)((CastleContainer)Current.Container).ConcreteContainer;
        }

        public WindsorDependencyResolver(WindsorContainer container)
        {
            this.container = container;
        }

        public object GetService(Type serviceType)
        {
            return container.Resolve(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return container.ResolveAll(serviceType).Cast<object>();
        }
    }

    /// <summary>
    /// Possibly not needed at all?
    /// </summary>
    public class WindsorControllerFactory : DefaultControllerFactory, IFilteredControllerFactory
    {
        private readonly IContainer container;

        public WindsorControllerFactory(IContainer container)
        {
            this.container = container;
        }

        public bool CanHandle(RequestContext request)
        {
            return true;
        }

        public override void ReleaseController(IController controller)
        {
            ((WindsorContainer)container.ConcreteContainer).Kernel.ReleaseComponent(controller);
        }

        protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        {
            if (controllerType == null)
            {
                throw new HttpException(404, string.Format("The controller for path '{0}' could not be found.", requestContext.HttpContext.Request.Path));
            }
            return (IController)container.GetInstance(controllerType);
        }
    }
}

