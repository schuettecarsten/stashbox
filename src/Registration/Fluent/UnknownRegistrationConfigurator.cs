﻿using System;

namespace Stashbox.Registration.Fluent
{
    /// <summary>
    /// Represents the fluent service registration api.
    /// </summary>
    public class UnknownRegistrationConfigurator : RegistrationConfigurator
    {
        internal bool RegistrationShouldBeSkipped { get; set; }

        internal UnknownRegistrationConfigurator(Type serviceType, Type implementationType) : base(serviceType, implementationType)
        {
        }

        /// <summary>
        /// Sets the current registration's implementation type.
        /// </summary>
        /// <param name="implementationType">The implementation type.</param>
        /// <returns>The configurator itself.</returns>
        public UnknownRegistrationConfigurator SetImplementationType(Type implementationType)
        {
            if (!implementationType.Implements(base.ServiceType))
                throw new ArgumentException($"The type {implementationType} does not implement the actual service type {base.ServiceType}.");

            base.ImplementationType = implementationType;
            return this;

        }

        /// <summary>
        /// Marks the current unknown type registration as skipped.
        /// </summary>
        /// <returns>The configurator itself.</returns>
        public UnknownRegistrationConfigurator Skip()
        {
            this.RegistrationShouldBeSkipped = true;
            return this;
        }
    }
}
