﻿using Stashbox.Entity;
using Stashbox.Entity.Resolution;
using Stashbox.Infrastructure;
using Stashbox.Overrides;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Stashbox
{
    internal class ResolutionStrategy : IResolutionStrategy
    {
        private readonly IResolverSelector resolverSelector;
        private readonly MethodInfo containsMethodInfo;
        private readonly MethodInfo getOverriddenValueMethodInfo;
        private readonly MethodInfo propertyAccessor;

        internal ResolutionStrategy(IResolverSelector resolverSelector)
        {
            this.resolverSelector = resolverSelector;
            var typeInfo = typeof(OverrideManager).GetTypeInfo();
            this.containsMethodInfo = typeInfo.GetDeclaredMethod("ContainsValue");
            this.getOverriddenValueMethodInfo = typeInfo.GetDeclaredMethod("GetOverriddenValue");
            this.propertyAccessor = typeof(ResolutionInfo).GetRuntimeProperty("OverrideManager").SetMethod;
        }

        public bool CanResolve(ResolutionInfo resolutionInfo, IContainerContext containerContext, TypeInformation typeInformation,
            InjectionParameter[] injectionParameters)
        {
            return this.resolverSelector.CanResolve(containerContext, typeInformation) ||
                                          (resolutionInfo?.OverrideManager != null && resolutionInfo.OverrideManager.ContainsValue(typeInformation)) ||
                                          (injectionParameters != null &&
                                           injectionParameters.Any(injectionParameter =>
                                           injectionParameter.Name == typeInformation.ParameterName));
        }

        public ResolutionTarget BuildResolutionTarget(IContainerContext containerContext, TypeInformation typeInformation,
            InjectionParameter[] injectionParameters)
        {
            Resolver resolver;
            this.resolverSelector.TryChooseResolver(containerContext, typeInformation, out resolver);

            return new ResolutionTarget
            {
                Resolver = resolver,
                TypeInformation = typeInformation,
                ResolutionTargetValue = injectionParameters?.FirstOrDefault(param => param.Name == typeInformation.ParameterName)?.Value
            };
        }

        public object EvaluateResolutionTarget(ResolutionTarget resolutionTarget, ResolutionInfo resolutionInfo)
        {
            if (resolutionInfo.OverrideManager != null && resolutionInfo.OverrideManager.ContainsValue(resolutionTarget.TypeInformation))
                return resolutionInfo.OverrideManager.GetOverriddenValue(resolutionTarget.TypeInformation);
            return resolutionTarget.ResolutionTargetValue ?? resolutionTarget.Resolver.Resolve(resolutionInfo);
        }

        public Expression GetExpressionForResolutionTarget(ResolutionTarget resolutionTarget, ResolutionInfo resolutionInfo, Expression resolutionInfoExpression)
        {
            var typeInfoConstant = Expression.Constant(resolutionTarget.TypeInformation);
            var nullExpr = Expression.Default(resolutionTarget.TypeInformation.Type);

            var overrideManagerExpression = Expression.Property(resolutionInfoExpression, this.propertyAccessor);
            var nullCheck = Expression.NotEqual(overrideManagerExpression, Expression.Constant(null));
            var contains = Expression.Call(overrideManagerExpression, this.containsMethodInfo, typeInfoConstant);

            var condition = Expression.AndAlso(nullCheck, contains);

            return Expression.Condition(condition,
                 Expression.Convert(Expression.Call(overrideManagerExpression, this.getOverriddenValueMethodInfo,
                        Expression.Constant(resolutionTarget.TypeInformation)), resolutionTarget.TypeInformation.Type),

                 resolutionTarget.ResolutionTargetValue != null ? Expression.Constant(resolutionTarget.ResolutionTargetValue) :
                    resolutionTarget.Resolver == null ? nullExpr : resolutionTarget.Resolver.GetExpression(resolutionInfo, resolutionInfoExpression),

                 resolutionTarget.TypeInformation.Type
            );
        }
    }
}
