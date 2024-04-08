﻿using Stashbox.Configuration;
using Stashbox.Exceptions;
using Stashbox.Lifetime;
using Stashbox.Resolution;
using Stashbox.Utils;
using Stashbox.Utils.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Stashbox.Registration.Fluent;

/// <summary>
/// Represents the base of the fluent registration api.
/// </summary>
/// <typeparam name="TConfigurator"></typeparam>
public class BaseFluentConfigurator<TConfigurator> : ServiceRegistration
    where TConfigurator : BaseFluentConfigurator<TConfigurator>
{
    /// <summary>
    /// The service type.
    /// </summary>
    public Type ServiceType { get; }

    internal BaseFluentConfigurator(Type serviceType, Type implementationType, object? name,
        LifetimeDescriptor lifetimeDescriptor, bool isDecorator)
        : base(implementationType, name, lifetimeDescriptor, isDecorator)
    {
        this.ServiceType = serviceType;
    }

    /// <summary>
    /// Determines whether the registration is mapped to the given service type.
    /// </summary>
    /// <typeparam name="TService">The target service type.</typeparam>
    /// <returns>True when the registration is mapped to the given service type, otherwise false.</returns>
    public bool HasServiceType<TService>() => this.HasServiceType(TypeCache<TService>.Type);
    
    /// <summary>
    /// Determines whether the registration is mapped to the given service type.
    /// </summary>
    /// <param name="serviceType">The target service type.</param>
    /// <returns>True when the registration is mapped to the given service type, otherwise false.</returns>
    public bool HasServiceType(Type serviceType)
    {
        Shield.EnsureNotNull(serviceType, nameof(serviceType));
        if (this.ServiceType == serviceType)
            return true;

        if (this.Options == null)
            return false;
        
        if (this.Options.TryGetValue(RegistrationOption.AdditionalServiceTypes, out var option) && option is ExpandableArray<Type> serviceTypes)
            return serviceTypes.Contains(serviceType);

        return false;
    }
    
    /// <summary>
    /// Sets the lifetime of the registration.
    /// </summary>
    /// <param name="lifetime">An <see cref="LifetimeDescriptor"/> implementation.</param>
    /// <returns>The configurator itself.</returns>
    public TConfigurator WithLifetime(LifetimeDescriptor lifetime)
    {
        this.Lifetime = lifetime;
        return (TConfigurator)this;
    }

    /// <summary>
    /// Sets scoped lifetime for the registration.
    /// </summary>
    /// <returns>The configurator itself.</returns>
    public TConfigurator WithScopedLifetime() => this.WithLifetime(Lifetimes.Scoped);

    /// <summary>
    /// Sets singleton lifetime for the registration.
    /// </summary>
    /// <returns>The configurator itself.</returns>
    public TConfigurator WithSingletonLifetime() => this.WithLifetime(Lifetimes.Singleton);

    /// <summary>
    /// Sets transient lifetime for the registration.
    /// </summary>
    /// <returns>The configurator itself.</returns>
    public TConfigurator WithTransientLifetime() => this.WithLifetime(Lifetimes.Transient);

    /// <summary>
    /// Sets the lifetime to <see cref="PerScopedRequestLifetime"/>. This lifetime will create a new instance between scoped services. This means
    /// that every scoped service will get a different instance but within their dependency tree it will behave as a singleton.
    /// </summary>
    /// <returns>The configurator itself.</returns>
    public TConfigurator WithPerScopedRequestLifetime() => this.WithLifetime(Lifetimes.PerScopedRequest);

    /// <summary>
    /// Sets the lifetime to <see cref="PerRequestLifetime"/>. This lifetime will create a new instance between resolution requests. 
    /// Within the request the same instance will be re-used.
    /// </summary>
    /// <returns>The configurator itself.</returns>
    public TConfigurator WithPerRequestLifetime() => this.WithLifetime(Lifetimes.PerRequest);
    
    /// <summary>
    /// Sets the lifetime to auto lifetime. This lifetime aligns to the lifetime of the resolved service's dependencies.
    /// When the underlying service has a dependency with a higher lifespan, this lifetime will inherit that lifespan up to a given boundary.
    /// </summary>
    /// <param name="boundaryLifetime">The lifetime that represents a boundary which the derived lifetime must not exceed.</param>
    /// <returns>The configurator itself.</returns>
    public TConfigurator WithAutoLifetime(LifetimeDescriptor boundaryLifetime) => this.WithLifetime(Lifetimes.Auto(boundaryLifetime));

    /// <summary>
    /// Sets a scope name condition for the registration, it will be used only when a scope with the given name requests it.
    /// </summary>
    /// <param name="scopeName">The name of the scope.</param>
    /// <returns>The configurator itself.</returns>
    public TConfigurator InNamedScope(object scopeName) => this.WithLifetime(Lifetimes.NamedScope(scopeName));

    /// <summary>
    /// Sets a condition for the registration that it will be used only within the scope defined by the given type.
    /// </summary>
    /// <param name="type">The type which defines the scope.</param>
    /// <returns>The configurator itself.</returns>
    public TConfigurator InScopeDefinedBy(Type type) => this.WithLifetime(Lifetimes.NamedScope(type));

    /// <summary>
    /// Sets a condition for the registration that it will be used only within the scope defined by the given type.
    /// </summary>
    /// <returns>The configurator itself.</returns>
    public TConfigurator InScopeDefinedBy<TScopeDefiner>() => this.WithLifetime(Lifetimes.NamedScope(TypeCache<TScopeDefiner>.Type));

    /// <summary>
    /// Binds a constructor/method parameter or a property/field to a named registration, so the container will perform a named resolution on the bound dependency.
    /// </summary>
    /// <param name="dependencyName">The name of the bound named registration.</param>
    /// <returns>The configurator itself.</returns>
    public TConfigurator WithDependencyBinding<TDependency>(object? dependencyName = null) =>
        this.WithDependencyBinding(TypeCache<TDependency>.Type, dependencyName);

    /// <summary>
    /// Binds a constructor/method parameter or a property/field to a named registration, so the container will perform a named resolution on the bound dependency.
    /// </summary>
    /// <param name="dependencyType">The type of the dependency to search for.</param>
    /// <param name="dependencyName">The name of the bound named registration.</param>
    /// <returns>The fluent configurator.</returns>
    public TConfigurator WithDependencyBinding(Type dependencyType, object? dependencyName = null)
    {
        Shield.EnsureNotNull(dependencyType, nameof(dependencyType));

        this.Options ??= new Dictionary<RegistrationOption, object?>();
        if (this.Options.TryGetValue(RegistrationOption.DependencyBindings, out var value) && value is Dictionary<object, object?> bindings)
            bindings.Add(dependencyType, dependencyName);
        else
            this.Options[RegistrationOption.DependencyBindings] = new Dictionary<object, object?> { { dependencyType, dependencyName } };

        return (TConfigurator)this;
    }

    /// <summary>
    /// Binds a constructor/method parameter or a property/field to a named registration, so the container will perform a named resolution on the bound dependency.
    /// </summary>
    /// <param name="parameterName">The parameter name of the dependency to search for.</param>
    /// <param name="dependencyName">The name of the bound named registration.</param>
    /// <returns>The fluent configurator.</returns>
    public TConfigurator WithDependencyBinding(string parameterName, object? dependencyName = null)
    {
        Shield.EnsureNotNull(parameterName, nameof(parameterName));

        this.Options ??= new Dictionary<RegistrationOption, object?>();
        if (this.Options.TryGetValue(RegistrationOption.DependencyBindings, out var value) && value is Dictionary<object, object?> bindings)
            bindings.Add(parameterName, dependencyName);
        else
            this.Options[RegistrationOption.DependencyBindings] = new Dictionary<object, object?> { { parameterName, dependencyName } };

        return (TConfigurator)this;
    }

    /// <summary>
    /// Sets a parent target condition for the registration.
    /// </summary>
    /// <typeparam name="TTarget">The type of the parent.</typeparam>
    /// <param name="name">The optional name of the parent.</param>
    /// <returns>The configurator itself.</returns>
    public TConfigurator WhenDependantIs<TTarget>(object? name = null) where TTarget : class => this.WhenDependantIs(TypeCache<TTarget>.Type, name);

    /// <summary>
    /// Sets a parent target condition for the registration.
    /// </summary>
    /// <param name="targetType">The type of the parent.</param>
    /// <param name="name">The optional name of the parent.</param>
    /// <returns>The configurator itself.</returns>
    public TConfigurator WhenDependantIs(Type targetType, object? name = null)
    {
        Shield.EnsureNotNull(targetType, nameof(targetType));

        this.Options ??= new Dictionary<RegistrationOption, object?>();
        if (this.Options.TryGetValue(RegistrationOption.ConditionOptions, out var value) && value is ConditionOptions conditions)
        {
            conditions.TargetTypeConditions ??= [];
            conditions.TargetTypeConditions.Add(new ReadOnlyKeyValue<object?, Type>(name, targetType));
        }
        else
            this.Options[RegistrationOption.ConditionOptions] = 
                new ConditionOptions { TargetTypeConditions = new ExpandableArray<object?, Type> { new ReadOnlyKeyValue<object?, Type>(name, targetType) } };

        return (TConfigurator)this;
    }

    /// <summary>
    /// Sets a resolution path condition for the registration. The service will be selected only in the resolution path of the given target.
    /// This means that only the direct and sub-dependencies of the target type will get the configured service.
    /// </summary>
    /// <typeparam name="TTarget">The type of the parent.</typeparam>
    /// <param name="name">The optional name of the parent.</param>
    /// <returns>The configurator itself.</returns>
    public TConfigurator WhenInResolutionPathOf<TTarget>(object? name = null) where TTarget : class => this.WhenInResolutionPathOf(TypeCache<TTarget>.Type, name);

    /// <summary>
    /// Sets a resolution path condition for the registration. The service will be selected only in the resolution path of the given target.
    /// This means that only the direct and sub-dependencies of the target type will get the configured service.
    /// </summary>
    /// <param name="targetType">The type of the parent.</param>
    /// <param name="name">The optional name of the parent.</param>
    /// <returns>The configurator itself.</returns>
    public TConfigurator WhenInResolutionPathOf(Type targetType, object? name = null)
    {
        Shield.EnsureNotNull(targetType, nameof(targetType));

        this.Options ??= new Dictionary<RegistrationOption, object?>();
        if (this.Options.TryGetValue(RegistrationOption.ConditionOptions, out var value) && value is ConditionOptions conditions)
        {
            conditions.TargetTypeInResolutionPathConditions ??= [];
            conditions.TargetTypeInResolutionPathConditions.Add(new ReadOnlyKeyValue<object?, Type>(name, targetType));
        }
        else
            this.Options[RegistrationOption.ConditionOptions] =
                new ConditionOptions { TargetTypeInResolutionPathConditions = new ExpandableArray<object?, Type> { new ReadOnlyKeyValue<object?, Type>(name, targetType) } };

        return (TConfigurator)this;
    }

    /// <summary>
    /// Sets an attribute condition for the registration.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
    /// <returns>The configurator itself.</returns>
    public TConfigurator WhenHas<TAttribute>(object? name = null) where TAttribute : Attribute => this.WhenHas(TypeCache<TAttribute>.Type);

    /// <summary>
    /// Sets an attribute condition for the registration.
    /// </summary>
    /// <param name="attributeType">The type of the attribute.</param>
    /// <returns>The configurator itself.</returns>
    public TConfigurator WhenHas(Type attributeType)
    {
        Shield.EnsureNotNull(attributeType, nameof(attributeType));

        this.Options ??= new Dictionary<RegistrationOption, object?>();
        if (this.Options.TryGetValue(RegistrationOption.ConditionOptions, out var value) && value is ConditionOptions conditions)
        {
            conditions.AttributeConditions ??= [];
            conditions.AttributeConditions.Add(attributeType);
        }
        else
            this.Options[RegistrationOption.ConditionOptions] = 
                new ConditionOptions { AttributeConditions = new ExpandableArray<Type> { attributeType } };

        return (TConfigurator)this;
    }

    /// <summary>
    /// Sets a resolution path condition for the registration. The service will be selected only in the resolution path of the target that has the given attribute.
    /// This means that only the direct and sub-dependencies of the target type that has the given attribute will get the configured service.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
    /// <returns>The configurator itself.</returns>
    public TConfigurator WhenResolutionPathHas<TAttribute>(object? name = null) where TAttribute : Attribute => this.WhenResolutionPathHas(TypeCache<TAttribute>.Type, name);

    /// <summary>
    /// Sets a resolution path condition for the registration. The service will be selected only in the resolution path of the target that has the given attribute.
    /// This means that only the direct and sub-dependencies of the target type that has the given attribute will get the configured service.
    /// </summary>
    /// <param name="attributeType">The type of the attribute.</param>
    /// <param name="name">The optional name of the target that has the desired attribute.</param>
    /// <returns>The configurator itself.</returns>
    public TConfigurator WhenResolutionPathHas(Type attributeType, object? name = null)
    {
        Shield.EnsureNotNull(attributeType, nameof(attributeType));

        this.Options ??= new Dictionary<RegistrationOption, object?>();
        if (this.Options.TryGetValue(RegistrationOption.ConditionOptions, out var value) && value is ConditionOptions conditions)
        {
            conditions.AttributeInResolutionPathConditions ??= [];
            conditions.AttributeInResolutionPathConditions.Add(new ReadOnlyKeyValue<object?, Type>(name, attributeType));
        }
        else
            this.Options[RegistrationOption.ConditionOptions] =
                new ConditionOptions { AttributeInResolutionPathConditions = new ExpandableArray<object?, Type> { new ReadOnlyKeyValue<object?, Type>(name, attributeType) } };

        return (TConfigurator)this;
    }

    /// <summary>
    /// Sets a generic condition for the registration.
    /// </summary>
    /// <param name="resolutionCondition">The predicate.</param>
    /// <returns>The configurator itself.</returns>
    public TConfigurator When(Func<TypeInformation, bool> resolutionCondition)
    {
        Shield.EnsureNotNull(resolutionCondition, nameof(resolutionCondition));

        this.Options ??= new Dictionary<RegistrationOption, object?>();
        if (this.Options.TryGetValue(RegistrationOption.ConditionOptions, out var value) && value is ConditionOptions conditions)
        {
            conditions.ResolutionConditions ??= [];
            conditions.ResolutionConditions.Add(resolutionCondition);
        }
        else
            this.Options[RegistrationOption.ConditionOptions] = new ConditionOptions { ResolutionConditions = new ExpandableArray<Func<TypeInformation, bool>> { resolutionCondition } };

        return (TConfigurator)this;
    }

    /// <summary>
    /// Sets injection parameters for the registration.
    /// </summary>
    /// <param name="injectionParameters">The injection parameters.</param>
    /// <returns>The fluent configurator.</returns>
    public TConfigurator WithInjectionParameters(params KeyValuePair<string, object?>[] injectionParameters)
    {
        this.Options ??= new Dictionary<RegistrationOption, object?>();
        if (this.Options.TryGetValue(RegistrationOption.InjectionParameters, out var value) && value is ExpandableArray<KeyValuePair<string, object?>> parameters)
            parameters.AddRange(injectionParameters);
        else
            this.Options[RegistrationOption.InjectionParameters] = new ExpandableArray<KeyValuePair<string, object?>>(injectionParameters);

        return (TConfigurator)this;
    }

    /// <summary>
    /// Sets injection parameters for the registration.
    /// </summary>
    /// <param name="name">The name of the injection parameter.</param>
    /// <param name="value">The value of the injection parameter.</param>
    /// <returns>The fluent configurator.</returns>
    public TConfigurator WithInjectionParameter(string name, object? value)
    {
        Shield.EnsureNotNull(name, nameof(name));

        this.Options ??= new Dictionary<RegistrationOption, object?>();
        if (this.Options.TryGetValue(RegistrationOption.InjectionParameters, out var injectionParams) && injectionParams is ExpandableArray<KeyValuePair<string, object?>> parameters)
            parameters.Add(new KeyValuePair<string, object?>(name, value));
        else
            this.Options[RegistrationOption.InjectionParameters] = new ExpandableArray<KeyValuePair<string, object?>> { new KeyValuePair<string, object?>(name, value) };

        return (TConfigurator)this;
    }

    /// <summary>
    /// Enables auto member injection on the registration.
    /// </summary>
    /// <param name="rule">The auto member injection rule.</param>
    /// <param name="filter">A filter delegate used to determine which members should be auto injected and which are not.</param>
    /// <returns>The fluent configurator.</returns>
    public TConfigurator WithAutoMemberInjection(Rules.AutoMemberInjectionRules rule = Rules.AutoMemberInjectionRules.PropertiesWithPublicSetter, Func<MemberInfo, bool>? filter = null)
    {
        this.Options ??= new Dictionary<RegistrationOption, object?>();
        this.Options[RegistrationOption.AutoMemberOptions] = new AutoMemberOptions(rule, filter);

        return (TConfigurator)this;
    }
    
    /// <summary>
    /// Enables or disables required member injection.
    /// </summary>
    /// <param name="enabled">True when the feature should be enabled, otherwise false.</param>
    /// <returns>The container configurator.</returns>
    public TConfigurator WithRequiredMemberInjection(bool enabled = true)
    {
        this.Options ??= new Dictionary<RegistrationOption, object?>();
        this.Options[RegistrationOption.RequiredMemberInjectionEnabled] = enabled;

        return (TConfigurator)this;
    }

    /// <summary>
    /// The constructor selection rule.
    /// </summary>
    /// <param name="rule">The constructor selection rule.</param>
    /// <returns>The fluent configurator.</returns>
    public TConfigurator WithConstructorSelectionRule(Func<IEnumerable<ConstructorInfo>, IEnumerable<ConstructorInfo>> rule)
    {
        this.Options ??= new Dictionary<RegistrationOption, object?>();
        this.Options[RegistrationOption.ConstructorSelectionRule] = rule;

        return (TConfigurator)this;
    }

    /// <summary>
    /// Sets the selected constructor.
    /// </summary>
    /// <param name="argumentTypes">The constructor argument types.</param>
    /// <returns>The fluent configurator.</returns>
    /// <exception cref="ConstructorNotFoundException" />
    public TConfigurator WithConstructorByArgumentTypes(params Type[] argumentTypes)
    {
        var constructor = this.ImplementationType.GetConstructor(argumentTypes);
        if (constructor == null)
        {
            ThrowConstructorNotFoundException(this.ImplementationType, argumentTypes);
            return (TConfigurator)this;
        }

        this.Options ??= new Dictionary<RegistrationOption, object?>();
        this.Options[RegistrationOption.ConstructorOptions] = new ConstructorOptions(constructor, null);

        return (TConfigurator)this;
    }

    /// <summary>
    /// Sets the selected constructor.
    /// </summary>
    /// <param name="arguments">The constructor arguments.</param>
    /// <returns>The fluent configurator.</returns>
    /// <exception cref="ConstructorNotFoundException" />
    public TConfigurator WithConstructorByArguments(params object[] arguments)
    {
        var argTypes = arguments.Select(arg => arg.GetType()).ToArray();
        var constructor = this.ImplementationType.GetConstructor(argTypes);
        if (constructor == null)
        {
            ThrowConstructorNotFoundException(this.ImplementationType, argTypes);
            return (TConfigurator)this;
        }

        this.Options ??= new Dictionary<RegistrationOption, object?>();
        this.Options[RegistrationOption.ConstructorOptions] = new ConstructorOptions(constructor, arguments);

        return (TConfigurator)this;
    }

    /// <summary>
    /// Tells the container that it shouldn't track the resolved transient object for disposal.
    /// </summary>
    /// <returns>The fluent configurator.</returns>
    public TConfigurator WithoutDisposalTracking()
    {
        this.Options ??= new Dictionary<RegistrationOption, object?>();
        this.Options[RegistrationOption.IsLifetimeExternallyOwned] = true;

        return (TConfigurator)this;
    }

    /// <summary>
    /// Tells the container that it should replace an existing registration with the current one, or add it if there is no existing found.
    /// </summary>
    /// <returns>The fluent configurator.</returns>
    public TConfigurator ReplaceExisting()
    {
        this.Options ??= new Dictionary<RegistrationOption, object?>();
        this.Options[RegistrationOption.ReplaceExistingRegistration] = true;

        return (TConfigurator)this;
    }

    /// <summary>
    /// Tells the container that it should replace an existing registration with the current one, but only if there is an existing registration.
    /// </summary>
    /// <returns>The fluent configurator.</returns>
    public TConfigurator ReplaceOnlyIfExists()
    {
        this.Options ??= new Dictionary<RegistrationOption, object?>();
        this.Options[RegistrationOption.ReplaceExistingRegistrationOnlyIfExists] = true;

        return (TConfigurator)this;
    }

    /// <summary>
    /// Registers the given service by all of it's implemented types.
    /// </summary>
    /// <returns>The configurator itself.</returns>
    public TConfigurator AsImplementedTypes()
    {
        this.Options ??= new Dictionary<RegistrationOption, object?>();
        var additionalTypes = this.ImplementationType.GetRegisterableInterfaceTypes()
            .Concat(this.ImplementationType.GetRegisterableBaseTypes());
        if (this.Options.TryGetValue(RegistrationOption.AdditionalServiceTypes, out var option) && option is ExpandableArray<Type> serviceTypes)
            serviceTypes.AddRange(additionalTypes);
        else
            this.Options[RegistrationOption.AdditionalServiceTypes] = new ExpandableArray<Type>(additionalTypes);
        
        return (TConfigurator)this;
    }

    /// <summary>
    /// Binds the currently configured registration to an additional service type.
    /// </summary>
    /// <returns>The configurator itself.</returns>
    public TConfigurator AsServiceAlso<TAdditionalService>() =>
        this.AsServiceAlso(TypeCache<TAdditionalService>.Type);

    /// <summary>
    /// Binds the currently configured registration to an additional service type.
    /// </summary>
    /// <param name="serviceType">The additional service type.</param>
    /// <returns>The configurator itself.</returns>
    public TConfigurator AsServiceAlso(Type serviceType)
    {
        if (!IsFactory() && !this.ImplementationType.Implements(serviceType))
            throw new ArgumentException($"The implementation type {this.ImplementationType} does not implement the given service type {serviceType}.");

        this.Options ??= new Dictionary<RegistrationOption, object?>();
        if (this.Options.TryGetValue(RegistrationOption.AdditionalServiceTypes, out var option) && option is ExpandableArray<Type> serviceTypes)
            serviceTypes.Add(serviceType);
        else
            this.Options[RegistrationOption.AdditionalServiceTypes] = new ExpandableArray<Type> { serviceType };

        return (TConfigurator)this;
    }

    internal void ValidateTypeMap()
    {
        if (IsFactory())
            return;

        Shield.EnsureTypeMapIsValid(ServiceType, ImplementationType);
    }

    internal void ValidateImplementationIsResolvable()
    {
        if (IsFactory())
            return;

        Shield.EnsureIsResolvable(ImplementationType);
    }

    private protected TConfigurator SetFactory(Delegate factory, Type implementationType, bool isCompiledLambda, params Type[] parameterTypes)
    {
        Shield.EnsureNotNull(implementationType, nameof(implementationType));

        this.ImplementationType = implementationType;
        return this.SetFactory(factory, isCompiledLambda, parameterTypes);
    }
    
    private protected TConfigurator SetFactory(Delegate factory, bool isCompiledLambda, params Type[] parameterTypes)
    {
        Shield.EnsureNotNull(factory, nameof(factory));

        this.Options ??= new Dictionary<RegistrationOption, object?>();
        this.Options[RegistrationOption.RegistrationTypeOptions] = new FactoryOptions(factory, parameterTypes, isCompiledLambda);

        return (TConfigurator)this;
    }

    private static void ThrowConstructorNotFoundException(Type type, params Type[] argTypes)
    {
        throw argTypes.Length switch
        {
            0 => new ConstructorNotFoundException(type),
            1 => new ConstructorNotFoundException(type, argTypes[0]),
            _ => new ConstructorNotFoundException(type, argTypes)
        };
    }
}