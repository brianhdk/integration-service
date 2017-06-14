using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core;
using Castle.DynamicProxy;
using Castle.MicroKernel;
using Castle.MicroKernel.ModelBuilder;
using Castle.Windsor;
using Vertica.Utilities.Patterns;

namespace Vertica.Integration.Infrastructure
{
    public class ServicesInterceptorsConfiguration : IInitializable<IWindsorContainer>
    {
        private readonly List<Registration> _registrations;

        public ServicesInterceptorsConfiguration(ServicesConfiguration services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            _registrations = new List<Registration>();

            Services = services;
        }

        public ServicesConfiguration Services { get; }

        public ServicesInterceptorsConfiguration InterceptService<TService, TInterceptor>()
            where TService : class
            where TInterceptor : class, IInterceptor
        {
            return AddInterceptor<TInterceptor>(new ComponentServiceIs<TService>());
        }

        public ServicesInterceptorsConfiguration AddInterceptor<TInterceptor>(Func<ComponentModel, bool> predicate)
            where TInterceptor : class, IInterceptor
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            Services.Advanced(advanced => advanced.Register<TInterceptor>());

            _registrations.Add(new Registration(
                InterceptorReference.ForType<TInterceptor>(),
                typeof(TInterceptor),
                predicate));

            return this;
        }

        void IInitializable<IWindsorContainer>.Initialized(IWindsorContainer container)
        {
            container.Kernel.ComponentModelBuilder.AddContributor(new InterceptorConstruction((kernel, model) =>
            {
                foreach (Registration registration in _registrations)
                    registration.Evaluate(model);
            }));
        }

        private class ComponentServiceIs<TService> : PredicateSpecification<ComponentModel>
            where TService : class
        {
            public ComponentServiceIs()
                : base(model => model.Services.Any(service => service == typeof(TService)))
            {
            }
        }

        private class Registration
        {
            private readonly InterceptorReference _interceptorReference;
            private readonly Type _interceptorType;
            private readonly Func<ComponentModel, bool> _predicate;

            public Registration(InterceptorReference interceptorReference, Type interceptorType, Func<ComponentModel, bool> predicate)
            {
                _interceptorReference = interceptorReference;
                _interceptorType = interceptorType;
                _predicate = predicate;
            }

            public void Evaluate(ComponentModel model)
            {
                if (model.Implementation != _interceptorType && _predicate(model))
                    model.Interceptors.AddIfNotInCollection(_interceptorReference);
            }
        }

        private class InterceptorConstruction : IContributeComponentModelConstruction
        {
            private readonly Action<IKernel, ComponentModel> _processModel;

            public InterceptorConstruction(Action<IKernel, ComponentModel> processModel)
            {
                _processModel = processModel;
            }

            public void ProcessModel(IKernel kernel, ComponentModel model)
            {
                _processModel(kernel, model);
            }
        }
    }
}