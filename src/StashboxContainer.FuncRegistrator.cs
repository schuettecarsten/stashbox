﻿using Stashbox.Registration;
using System;

namespace Stashbox
{
    public partial class StashboxContainer
    {
        /// <inheritdoc />
        public IStashboxContainer RegisterFunc<TService>(Func<IDependencyResolver, TService> factory, string name = null) =>
            this.RegisterFuncInternal(factory, typeof(Func<TService>), name);

        /// <inheritdoc />
        public IStashboxContainer RegisterFunc<T1, TService>(Func<T1, IDependencyResolver, TService> factory, string name = null) =>
            this.RegisterFuncInternal(factory, typeof(Func<T1, TService>), name);

        /// <inheritdoc />
        public IStashboxContainer RegisterFunc<T1, T2, TService>(Func<T1, T2, IDependencyResolver, TService> factory, string name = null) =>
            this.RegisterFuncInternal(factory, typeof(Func<T1, T2, TService>), name);

        /// <inheritdoc />
        public IStashboxContainer RegisterFunc<T1, T2, T3, TService>(Func<T1, T2, T3, IDependencyResolver, TService> factory, string name = null) =>
            this.RegisterFuncInternal(factory, typeof(Func<T1, T2, T3, TService>), name);

        /// <inheritdoc />
        public IStashboxContainer RegisterFunc<T1, T2, T3, T4, TService>(Func<T1, T2, T3, T4, IDependencyResolver, TService> factory, string name = null) =>
            this.RegisterFuncInternal(factory, typeof(Func<T1, T2, T3, T4, TService>), name);

        private IStashboxContainer RegisterFuncInternal(Delegate factory, Type factoryType, string name)
        {
            this.ThrowIfDisposed();

            var data = new RegistrationContext { Name = name, FuncDelegate = factory };
            var registration = new ServiceRegistration(factoryType, RegistrationType.Func, this.ContainerContext.ContainerConfiguration, data, false);
            this.ContainerContext.RegistrationRepository.AddOrUpdateRegistration(registration, factoryType);
            return this;
        }
    }
}
