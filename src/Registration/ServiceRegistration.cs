﻿using Stashbox.Configuration;
using Stashbox.Lifetime;
using Stashbox.Resolution;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Stashbox.Registration
{
    /// <summary>
    /// Represents a service registration.
    /// </summary>
    public class ServiceRegistration
    {
        private static int globalRegistrationId;

        private static int globalRegistrationOrder;

        /// <summary>
        /// The implementation type.
        /// </summary>
        public Type ImplementationType { get; }

        /// <summary>
        /// The registration context.
        /// </summary>
        public RegistrationContext RegistrationContext { get; }

        /// <summary>
        /// The registration id.
        /// </summary>
        public int RegistrationId { get; }

        /// <summary>
        /// The registration order indicator.
        /// </summary>
        public int RegistrationOrder { get; private set; }

        /// <summary>
        /// True if the registration is a decorator.
        /// </summary>
        public bool IsDecorator { get; }

        /// <summary>
        /// Represents the nature of the registration.
        /// </summary>
        public RegistrationType RegistrationType { get; }

        internal readonly bool IsResolvableByUnnamedRequest;

        internal readonly bool HasScopeName;

        internal readonly object NamedScopeRestrictionIdentifier;

        internal readonly bool HasCondition;

        internal readonly object RegistrationDiscriminator;

        private protected readonly ContainerConfiguration Configuration;

        internal ServiceRegistration(Type implementationType, RegistrationType registrationType, ContainerConfiguration containerConfiguration,
            RegistrationContext registrationContext, bool isDecorator)
        {
            this.Configuration = containerConfiguration;
            this.ImplementationType = implementationType;
            this.RegistrationContext = registrationContext;
            this.IsDecorator = isDecorator;
            this.RegistrationType = registrationType;

            this.IsResolvableByUnnamedRequest = this.RegistrationContext.Name == null || containerConfiguration.NamedDependencyResolutionForUnNamedRequestsEnabled;

            if (this.RegistrationContext.Lifetime is NamedScopeLifetime lifetime)
            {
                this.HasScopeName = true;
                this.NamedScopeRestrictionIdentifier = lifetime.ScopeName;
            }

            this.HasCondition = this.RegistrationContext.TargetTypeConditions.Length > 0 || 
                this.RegistrationContext.ResolutionConditions.Length > 0 ||
                this.RegistrationContext.AttributeConditions.Length > 0;

            this.RegistrationId = ReserveRegistrationId();
            this.RegistrationOrder = ReserveRegistrationOrder();
            this.RegistrationDiscriminator = containerConfiguration.RegistrationBehavior == Rules.RegistrationBehavior.PreserveDuplications
                    ? this.RegistrationId
                    : this.RegistrationContext.Name ?? implementationType;
        }

        internal bool IsUsableForCurrentContext(TypeInformation typeInfo) =>
            this.HasParentTypeConditionAndMatch(typeInfo) ||
            this.HasAttributeConditionAndMatch(typeInfo) ||
            this.HasResolutionConditionAndMatch(typeInfo);

        internal void Replaces(ServiceRegistration serviceRegistration) =>
            this.RegistrationOrder = serviceRegistration.RegistrationOrder;

        private bool HasParentTypeConditionAndMatch(TypeInformation typeInfo) =>
            this.RegistrationContext.TargetTypeConditions.Length > 0 && typeInfo.ParentType != null && this.RegistrationContext.TargetTypeConditions.Contains(typeInfo.ParentType);

        private bool HasAttributeConditionAndMatch(TypeInformation typeInfo) =>
            this.RegistrationContext.AttributeConditions.Length > 0 && typeInfo.CustomAttributes != null &&
            this.RegistrationContext.AttributeConditions.Intersect(typeInfo.CustomAttributes.Select(attribute => attribute.GetType())).Any();

        private bool HasResolutionConditionAndMatch(TypeInformation typeInfo)
        {
            if (this.RegistrationContext.ResolutionConditions.Length == 0)
                return false;

            var length = this.RegistrationContext.ResolutionConditions.Length;
            for (int i = 0; i < length; i++)
            {
                if(this.RegistrationContext.ResolutionConditions[i](typeInfo))
                    return true;
            }

            return false;
        }

        private static int ReserveRegistrationId() =>
            Interlocked.Increment(ref globalRegistrationId);

        private static int ReserveRegistrationOrder() =>
            Interlocked.Increment(ref globalRegistrationOrder);
    }
}
