﻿using Stashbox.Entity;
using Stashbox.Infrastructure;
using Stashbox.Infrastructure.ContainerExtension;
using System;
using System.Linq.Expressions;
using Stashbox.BuildUp.Expressions;

namespace Stashbox.BuildUp
{
    internal class FactoryObjectBuilder : ObjectBuilderBase
    {
        private readonly Func<IDependencyResolver, object> containerFactory;
        private readonly Func<object> singleFactory;
        private readonly IContainerExtensionManager containerExtensionManager;
        private readonly IMetaInfoProvider metaInfoProvider;
        private readonly InjectionParameter[] injectionParameters;
        private readonly IContainerContext containerContext;
        private readonly IExpressionBuilder expressionBuilder;

        private FactoryObjectBuilder(IContainerContext containerContext, IContainerExtensionManager containerExtensionManager,
            IMetaInfoProvider metaInfoProvider, IExpressionBuilder expressionBuilder,
            InjectionParameter[] injectionParameters, bool isDecorator, bool shouldHandleDisposal)
            : base(containerContext, isDecorator, shouldHandleDisposal)
        {
            this.containerContext = containerContext;
            this.containerExtensionManager = containerExtensionManager;
            this.metaInfoProvider = metaInfoProvider;
            this.injectionParameters = injectionParameters;
            this.expressionBuilder = expressionBuilder;
        }

        public FactoryObjectBuilder(Func<IDependencyResolver, object> containerFactory, IContainerContext containerContext,
            IContainerExtensionManager containerExtensionManager, IMetaInfoProvider metaInfoProvider, IExpressionBuilder expressionBuilder, 
            InjectionParameter[] injectionParameters, bool isDecorator, bool shouldHandleDisposal)
            : this(containerContext, containerExtensionManager, metaInfoProvider, expressionBuilder, injectionParameters, isDecorator, shouldHandleDisposal)
        {
            this.containerFactory = containerFactory;
        }

        public FactoryObjectBuilder(Func<object> factory, IContainerContext containerContext, IContainerExtensionManager containerExtensionManager,
            IMetaInfoProvider metaInfoProvider, IExpressionBuilder expressionBuilder, InjectionParameter[] injectionParameters, bool isDecorator, bool shouldHandleDisposal)
            : this(containerContext, containerExtensionManager, metaInfoProvider, expressionBuilder, injectionParameters, isDecorator, shouldHandleDisposal)
        {
            this.singleFactory = factory;
        }

        protected override Expression GetExpressionInternal(ResolutionInfo resolutionInfo, Type resolveType)
        {
            Expression<Func<IDependencyResolver, object>> lambda;
            if (this.containerFactory != null)
                lambda = scope => this.containerFactory(scope);
            else
                lambda = scope => this.singleFactory();
            
            var expr = Expression.Invoke(lambda, Expression.Convert(Constants.ScopeExpression, Constants.ResolverType));

            return this.expressionBuilder.CreateFillExpression(this.containerExtensionManager, this.containerContext,
                   expr, resolutionInfo, resolveType, this.injectionParameters,
                   this.metaInfoProvider.GetResolutionMembers(resolutionInfo),
                   this.metaInfoProvider.GetResolutionMethods(resolutionInfo));
        }
    }
}