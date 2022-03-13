﻿using Stashbox.Resolution;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Stashbox.Utils
{
    internal static class Constants
    {
        public static readonly Type StringType = typeof(string);

        public static readonly Type ResolutionScopeType = typeof(IResolutionScope);

        public static readonly Type ServiceProviderType = typeof(IServiceProvider);

        public static readonly Type ResolverType = typeof(IDependencyResolver);

        public static readonly Type RequestContextType = typeof(IRequestContext);

        public static readonly Type InternalRequestContextType = typeof(IInternalRequestContext);

        public static readonly Type ObjectType = typeof(object);

        public static readonly ParameterExpression ResolutionScopeParameter = ResolutionScopeType.AsParameter("scope");

        public static readonly ParameterExpression RequestContextParameter = RequestContextType.AsParameter("request");

        public static readonly MethodInfo AddDisposalMethod = ResolutionScopeType.GetMethod(nameof(IResolutionScope.AddDisposableTracking))!;

        public static readonly MethodInfo AddRequestContextAwareDisposalMethod = ResolutionScopeType.GetMethod(nameof(IResolutionScope.AddRequestContextAwareDisposableTracking))!;

        public static readonly MethodInfo GetOrAddScopedObjectMethod = ResolutionScopeType.GetMethod(nameof(IResolutionScope.GetOrAddScopedObject))!;

        public static readonly MethodInfo AddWithFinalizerMethod = ResolutionScopeType.GetMethod(nameof(IResolutionScope.AddWithFinalizer))!;

        public static readonly MethodInfo AddWithAsyncInitializerMethod = ResolutionScopeType.GetMethod(nameof(IResolutionScope.AddWithAsyncInitializer))!;

        public static readonly MethodInfo GetOrAddInstanceMethod = InternalRequestContextType.GetMethod(nameof(IInternalRequestContext.GetOrAddInstance))!;

        public static readonly MethodInfo CheckRuntimeCircularDependencyBarrierMethod =
#pragma warning disable 618
            ResolutionScopeType.GetMethod(nameof(IResolutionScope.CheckRuntimeCircularDependencyBarrier))!;
#pragma warning restore 618

        public static readonly MethodInfo ResetRuntimeCircularDependencyBarrierMethod =
#pragma warning disable 618
            ResolutionScopeType.GetMethod(nameof(IResolutionScope.ResetRuntimeCircularDependencyBarrier))!;
#pragma warning restore 618

        public static readonly MethodInfo BeginScopeMethod = ResolverType.GetMethod(nameof(IDependencyResolver.BeginScope))!;

        public static readonly Type DisposableType = typeof(IDisposable);

#if HAS_ASYNC_DISPOSABLE
        public static readonly Type AsyncDisposableType = typeof(IAsyncDisposable);
#endif

        public static readonly Type FuncType = typeof(Func<>);

        public static readonly Type[] EmptyTypes = EmptyArray<Type>();

        public static readonly Type CompositionRootType = typeof(ICompositionRoot);

        public static readonly Type DependencyAttributeType = typeof(Attributes.DependencyAttribute);

        public static readonly Type InjectionAttributeType = typeof(Attributes.InjectionMethodAttribute);

        public const MethodImplOptions Inline = (MethodImplOptions)256;

        public const byte DelegatePlaceholder = 0;

        public static T[] EmptyArray<T>() => InternalArrayHelper<T>.Empty;
    }
}
