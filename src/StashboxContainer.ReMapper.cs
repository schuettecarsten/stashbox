﻿using Stashbox.Registration.Fluent;
using System;
using Stashbox.Registration;

namespace Stashbox
{
    public partial class StashboxContainer
    {
        /// <inheritdoc />
        public IStashboxContainer ReMap<TFrom, TTo>(Action<RegistrationConfigurator<TFrom, TTo>>? configurator = null)
            where TFrom : class
            where TTo : class, TFrom
        {
            this.ThrowIfDisposed();
            var registrationConfigurator = new RegistrationConfigurator<TFrom, TTo>(typeof(TFrom), typeof(TTo));
            configurator?.Invoke(registrationConfigurator);
            return this.ReMapInternal(registrationConfigurator);
        }

        /// <inheritdoc />
        public IStashboxContainer ReMap<TFrom>(Type typeTo, Action<RegistrationConfigurator<TFrom, TFrom>>? configurator = null)
            where TFrom : class
        {
            this.ThrowIfDisposed();

            var registrationConfigurator = new RegistrationConfigurator<TFrom, TFrom>(typeof(TFrom), typeTo);
            configurator?.Invoke(registrationConfigurator);

            registrationConfigurator.ValidateTypeMap();

            return this.ReMapInternal(registrationConfigurator);
        }

        /// <inheritdoc />
        public IStashboxContainer ReMap(Type typeFrom, Type typeTo, Action<RegistrationConfigurator>? configurator = null)
        {
            this.ThrowIfDisposed();

            var registrationConfigurator = new RegistrationConfigurator(typeFrom, typeTo);
            configurator?.Invoke(registrationConfigurator);

            registrationConfigurator.ValidateTypeMap();

            return this.ReMapInternal(registrationConfigurator);
        }

        /// <inheritdoc />
        public IStashboxContainer ReMap<TTo>(Action<RegistrationConfigurator<TTo, TTo>>? configurator = null)
             where TTo : class
        {
            this.ThrowIfDisposed();
            var type = typeof(TTo);
            var registrationConfigurator = new RegistrationConfigurator<TTo, TTo>(type, type);
            configurator?.Invoke(registrationConfigurator);

            registrationConfigurator.ValidateImplementationIsResolvable();

            return this.ReMapInternal(registrationConfigurator);
        }

        /// <inheritdoc />
        public IStashboxContainer ReMapDecorator(Type typeFrom, Type typeTo, Action<DecoratorConfigurator>? configurator = null)
        {
            this.ThrowIfDisposed();

            var decoratorConfigurator = new DecoratorConfigurator(typeFrom, typeTo);
            configurator?.Invoke(decoratorConfigurator);

            decoratorConfigurator.ValidateTypeMap();

            return this.ReMapInternal(decoratorConfigurator, true);
        }

        /// <inheritdoc />
        public IStashboxContainer ReMapDecorator<TFrom, TTo>(Action<DecoratorConfigurator<TFrom, TTo>>? configurator = null)
            where TFrom : class
            where TTo : class, TFrom
        {
            this.ThrowIfDisposed();
            var decoratorConfigurator = new DecoratorConfigurator<TFrom, TTo>(typeof(TFrom), typeof(TTo));
            configurator?.Invoke(decoratorConfigurator);
            return this.ReMapInternal(decoratorConfigurator, true);
        }

        /// <inheritdoc />
        public IStashboxContainer ReMapDecorator<TFrom>(Type typeTo, Action<DecoratorConfigurator<TFrom, TFrom>>? configurator = null)
            where TFrom : class
        {
            this.ThrowIfDisposed();

            var decoratorConfigurator = new DecoratorConfigurator<TFrom, TFrom>(typeof(TFrom), typeTo);
            configurator?.Invoke(decoratorConfigurator);

            decoratorConfigurator.ValidateTypeMap();

            return this.ReMapInternal(decoratorConfigurator, true);
        }

        private IStashboxContainer ReMapInternal(RegistrationConfiguration registrationConfiguration,
            bool isDecorator = false)
        {
            ServiceRegistrator.ReMap(
                this.ContainerContext,
                RegistrationBuilder.BuildServiceRegistration(this.ContainerContext,
                    registrationConfiguration, isDecorator),
                registrationConfiguration.ServiceType);

            return this;
        }
    }
}
