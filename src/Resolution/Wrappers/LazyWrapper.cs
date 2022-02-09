﻿using Stashbox.Utils;
using System;
using System.Linq.Expressions;

namespace Stashbox.Resolution.Wrappers
{
    internal class LazyWrapper : IServiceWrapper
    {
        private static bool IsLazy(Type type) => type.IsClosedGenericType() && type.GetGenericTypeDefinition() == typeof(Lazy<>);

        public Expression WrapExpression(TypeInformation originalTypeInformation, TypeInformation wrappedTypeInformation, 
            ServiceContext serviceContext)
        {
            var lazyArgumentInfo = originalTypeInformation.Clone(originalTypeInformation.Type.GetGenericArguments()[0]);

            var ctorParamType = Constants.FuncType.MakeGenericType(lazyArgumentInfo.Type);
            var lazyConstructor = originalTypeInformation.Type.GetConstructor(ctorParamType);
            return lazyConstructor.MakeNew(serviceContext.ServiceExpression.AsLambda());
        }

        public bool TryUnWrap(TypeInformation typeInformation, out TypeInformation unWrappedType)
        {
            if (!IsLazy(typeInformation.Type))
            {
                unWrappedType = null;
                return false;
            }

            unWrappedType = typeInformation.Clone(typeInformation.Type.GetGenericArguments()[0]);
            return true;
        }
    }
}
