﻿using Stashbox.Configuration;
using Stashbox.Exceptions;
using Stashbox.Resolution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Stashbox.Registration
{
    /// <summary>
    /// Represents a service registration.
    /// </summary>
    public class ServiceRegistration : IServiceRegistration
    {
        private static int globalRegistrationOrder;
        private readonly ContainerConfiguration containerConfiguration;

        /// <inheritdoc />
        public Type ImplementationType { get; }

        /// <inheritdoc />
        public TypeInfo ImplementationTypeInfo { get; }

        /// <inheritdoc />
        public RegistrationContext RegistrationContext { get; }

        /// <inheritdoc />
        public bool IsDecorator { get; }

        /// <inheritdoc />
        public int RegistrationId { get; private set; }

        /// <inheritdoc />
        public object RegistrationName { get; }

        /// <inheritdoc />
        public bool IsResolvableByUnnamedRequest { get; }

        /// <inheritdoc />
        public bool HasScopeName { get; }

        /// <inheritdoc />
        public bool HasCondition { get; }

        /// <inheritdoc />
        public RegistrationType RegistrationType { get; }

        internal ServiceRegistration(Type implementationType, RegistrationType registrationType, ContainerConfiguration containerConfiguration,
            RegistrationContext registrationContext, bool isDecorator)
        {
            this.containerConfiguration = containerConfiguration;
            this.ImplementationType = implementationType;
            this.ImplementationTypeInfo = implementationType.GetTypeInfo();
            this.RegistrationContext = registrationContext;
            this.IsDecorator = isDecorator;
            this.RegistrationType = registrationType;

            this.IsResolvableByUnnamedRequest = this.RegistrationContext.Name == null || containerConfiguration.NamedDependencyResolutionForUnNamedRequestsEnabled;

            this.HasScopeName = this.RegistrationContext.NamedScopeRestrictionIdentifier != null;

            this.HasCondition = this.RegistrationContext.TargetTypeCondition != null || this.RegistrationContext.ResolutionCondition != null ||
                this.RegistrationContext.AttributeConditions != null && this.RegistrationContext.AttributeConditions.Any();

            this.RegistrationId = ReserveRegistrationOrder();
            this.RegistrationName = this.RegistrationContext.Name ??
                (containerConfiguration.RegistrationBehavior == Rules.RegistrationBehavior.PreserveDuplications
                ? (object)this.RegistrationId
                : implementationType);
        }

        /// <inheritdoc />
        public bool IsUsableForCurrentContext(TypeInformation typeInfo) =>
            this.HasParentTypeConditionAndMatch(typeInfo) ||
            this.HasAttributeConditionAndMatch(typeInfo) ||
            this.HasResolutionConditionAndMatch(typeInfo);

        /// <inheritdoc />
        public bool CanInjectIntoNamedScope(IEnumerable<object> scopeNames) =>
            scopeNames.Last() == this.RegistrationContext.NamedScopeRestrictionIdentifier;

        /// <inheritdoc />
        public IServiceRegistration Clone(Type implementationType, RegistrationType registrationType) =>
            new ServiceRegistration(implementationType, registrationType, this.containerConfiguration,
                this.RegistrationContext, this.IsDecorator);

        /// <inheritdoc />
        public void Replaces(IServiceRegistration serviceRegistration)
        {
            if (this.containerConfiguration.RegistrationBehavior == Rules.RegistrationBehavior.ThrowException)
                throw new ServiceAlreadyRegisteredException(this.ImplementationType);

            this.RegistrationId = serviceRegistration.RegistrationId;
        }

        private bool HasParentTypeConditionAndMatch(TypeInformation typeInfo) =>
            this.RegistrationContext.TargetTypeCondition != null && typeInfo.ParentType != null && this.RegistrationContext.TargetTypeCondition == typeInfo.ParentType;

        private bool HasAttributeConditionAndMatch(TypeInformation typeInfo) =>
            this.RegistrationContext.AttributeConditions != null && typeInfo.CustomAttributes != null &&
            this.RegistrationContext.AttributeConditions.Intersect(typeInfo.CustomAttributes.Select(attribute => attribute.GetType())).Any();

        private bool HasResolutionConditionAndMatch(TypeInformation typeInfo) =>
            this.RegistrationContext.ResolutionCondition != null && this.RegistrationContext.ResolutionCondition(typeInfo);

        private static int ReserveRegistrationOrder() =>
            Interlocked.Increment(ref globalRegistrationOrder);
    }
}
