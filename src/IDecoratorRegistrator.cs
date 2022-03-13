﻿using Stashbox.Registration.Fluent;
using System;

namespace Stashbox
{
    /// <summary>
    /// Represents a decorator registrator.
    /// </summary>
    public interface IDecoratorRegistrator
    {
        /// <summary>
        /// Registers a decorator service into the container.
        /// </summary>
        /// <param name="typeFrom">The service type.</param>
        /// <param name="typeTo">The implementation type.</param>
        /// <returns>The <see cref="IStashboxContainer"/> instance.</returns>
        IStashboxContainer RegisterDecorator(Type typeFrom, Type typeTo);

        /// <summary>
        /// Registers a decorator service into the container with custom configuration.
        /// </summary>
        /// <param name="typeFrom">The service type.</param>
        /// <param name="typeTo">The implementation type.</param>
        /// <param name="configurator">The configurator for the registered types.</param>
        /// <returns>The <see cref="IStashboxContainer"/> instance.</returns>
        IStashboxContainer RegisterDecorator(Type typeFrom, Type typeTo, Action<DecoratorConfigurator> configurator);

        /// <summary>
        /// Registers a decorator service into the container.
        /// </summary>
        /// <typeparam name="TFrom">The service type.</typeparam>
        /// <typeparam name="TTo">The implementation type.</typeparam>
        /// <returns>The <see cref="IStashboxContainer"/> instance.</returns>
        IStashboxContainer RegisterDecorator<TFrom, TTo>()
            where TFrom : class
            where TTo : class, TFrom;

        /// <summary>
        /// Registers a decorator service into the container with custom configuration.
        /// </summary>
        /// <typeparam name="TFrom">The service type.</typeparam>
        /// <typeparam name="TTo">The implementation type.</typeparam>
        /// <param name="configurator">The configurator for the registered types.</param>
        /// <returns>The <see cref="IStashboxContainer"/> instance.</returns>
        IStashboxContainer RegisterDecorator<TFrom, TTo>(Action<DecoratorConfigurator<TFrom, TTo>> configurator)
            where TFrom : class
            where TTo : class, TFrom;

        /// <summary>
        /// Registers a decorator service into the container. 
        /// This function configures the registration with the <see cref="BaseFluentConfigurator{T}.AsImplementedTypes()"/> option.
        /// </summary>
        /// <param name="typeTo">The implementation type.</param>
        /// <returns>The <see cref="IStashboxContainer"/> instance.</returns>
        IStashboxContainer RegisterDecorator(Type typeTo);

        /// <summary>
        /// Registers a decorator service into the container with custom configuration. 
        /// This function configures the registration with the <see cref="BaseFluentConfigurator{T}.AsImplementedTypes()"/> option.
        /// </summary>
        /// <param name="typeTo">The implementation type.</param>
        /// <param name="configurator">The configurator for the registered types.</param>
        /// <returns>The <see cref="IStashboxContainer"/> instance.</returns>
        IStashboxContainer RegisterDecorator(Type typeTo, Action<DecoratorConfigurator> configurator);

        /// <summary>
        /// Registers a decorator service into the container. 
        /// This function configures the registration with the <see cref="BaseFluentConfigurator{T}.AsImplementedTypes()"/> option.
        /// </summary>
        /// <typeparam name="TTo">The implementation type.</typeparam>
        /// <returns>The <see cref="IStashboxContainer"/> instance.</returns>
        IStashboxContainer RegisterDecorator<TTo>()
            where TTo : class;

        /// <summary>
        /// Registers a decorator service into the container with custom configuration. 
        /// This function configures the registration with the <see cref="BaseFluentConfigurator{T}.AsImplementedTypes()"/> option.
        /// </summary>
        /// <typeparam name="TTo">The implementation type.</typeparam>
        /// <param name="configurator">The configurator for the registered types.</param>
        /// <returns>The <see cref="IStashboxContainer"/> instance.</returns>
        IStashboxContainer RegisterDecorator<TTo>(Action<DecoratorConfigurator<TTo, TTo>> configurator)
            where TTo : class;

        /// <summary>
        /// Registers a decorator service into the container.
        /// </summary>
        /// <typeparam name="TFrom">The service type.</typeparam>
        /// <param name="typeTo">The implementation type.</param>
        /// <returns>The <see cref="IStashboxContainer"/> instance.</returns>
        IStashboxContainer RegisterDecorator<TFrom>(Type typeTo)
            where TFrom : class;

        /// <summary>
        /// Registers a decorator service into the container with custom configuration.
        /// </summary>
        /// <typeparam name="TFrom">The service type.</typeparam>
        /// <param name="typeTo">The implementation type.</param>
        /// <param name="configurator">The configurator for the registered types.</param>
        /// <returns>The <see cref="IStashboxContainer"/> instance.</returns>
        IStashboxContainer RegisterDecorator<TFrom>(Type typeTo,
            Action<DecoratorConfigurator<TFrom, TFrom>> configurator)
            where TFrom : class;
    }
}
