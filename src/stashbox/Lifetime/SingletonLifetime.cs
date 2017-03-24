﻿using Stashbox.Entity;
using Stashbox.Infrastructure;
using System;
using System.Linq.Expressions;

namespace Stashbox.Lifetime
{
    /// <summary>
    /// Represents a singleton lifetime manager.
    /// </summary>
    public class SingletonLifetime : LifetimeBase
    {
        private volatile Expression expression;
        private object instance;
        private readonly object syncObject = new object();

        /// <inheritdoc />
        public override Expression GetExpression(IContainerContext containerContext, IObjectBuilder objectBuilder, ResolutionInfo resolutionInfo, Type resolveType)
        {
            if (this.expression != null) return this.expression;
            lock (this.syncObject)
            {
                if (this.expression != null) return this.expression;
                var expr = base.GetExpression(containerContext, objectBuilder, resolutionInfo, resolveType);
                if (expr == null)
                    return null;

                this.instance = expr.CompileDelegate(Constants.ScopeExpression)(resolutionInfo.ResolutionScope);
                this.expression = Expression.Constant(this.instance);
            }

            return this.expression;
        }

        /// <inheritdoc />
        public override bool HandlesObjectDisposal => true;

        /// <inheritdoc />
        public override ILifetime Create() => new SingletonLifetime();

        /// <inheritdoc />
        public override void CleanUp()
        {
            if (this.instance == null) return;
            lock (this.syncObject)
            {
                if (this.instance == null) return;
                var disposable = this.instance as IDisposable;
                disposable?.Dispose();
                this.instance = null;
            }
        }
    }
}
