﻿using Stashbox.Lifetime;
using Stashbox.Registration.Fluent;
using Stashbox.Utils;
using System;

namespace Stashbox.Registration
{
    internal class RegistrationBuilder
    {
        public IServiceRegistration BuildServiceRegistration(IContainerContext containerContext, RegistrationConfiguration registrationConfiguration, bool isDecorator)
        {
            this.PreProcessExistingInstanceIfNeeded(containerContext, registrationConfiguration.Context, registrationConfiguration.ImplementationType);
            registrationConfiguration.Context.Lifetime = this.ChooseLifeTime(containerContext, registrationConfiguration.Context);

            return new ServiceRegistration(registrationConfiguration.ImplementationType, this.DetermineRegistrationType(registrationConfiguration),
                containerContext.ContainerConfiguration, registrationConfiguration.Context, isDecorator);
        }

        private void PreProcessExistingInstanceIfNeeded(IContainerContext containerContext, RegistrationContext registrationContext, Type implementationType)
        {
            if (registrationContext.ExistingInstance == null) return;

            if (!registrationContext.IsLifetimeExternallyOwned && registrationContext.ExistingInstance is IDisposable disposable)
                containerContext.RootScope.AddDisposableTracking(disposable);

            if (registrationContext.Finalizer == null) return;

            var method = Constants.AddWithFinalizerMethod.MakeGenericMethod(implementationType);
            method.Invoke(containerContext.RootScope, new[] { registrationContext.ExistingInstance, registrationContext.Finalizer });
        }

        private LifetimeDescriptor ChooseLifeTime(IContainerContext containerContext, RegistrationContext registrationContext) => registrationContext.IsWireUp
                ? Lifetimes.Singleton
                : registrationContext.Lifetime ?? containerContext.ContainerConfiguration.DefaultLifetime;

        private RegistrationType DetermineRegistrationType(RegistrationConfiguration registrationConfiguration)
        {
            if (registrationConfiguration.ImplementationType.IsOpenGenericType())
                return RegistrationType.OpenGeneric;

            if (registrationConfiguration.Context.ExistingInstance != null)
                return registrationConfiguration.Context.IsWireUp
                    ? RegistrationType.WireUp
                    : RegistrationType.Instance;

            return registrationConfiguration.Context.ContainerFactory != null ||
                   registrationConfiguration.Context.SingleFactory != null
                ? RegistrationType.Factory
                : RegistrationType.Default;
        }
    }
}
