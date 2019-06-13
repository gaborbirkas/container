﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Unity.Policy;
using Unity.Resolution;

namespace Unity
{
    public partial class MethodPipeline : MethodBasePipeline<MethodInfo>
    {
        #region Constructors

        public MethodPipeline(UnityContainer container, ParametersProcessor? processor = null)
            : base(typeof(InjectionMethodAttribute), container, processor)
        {
        }

        #endregion


        #region Overrides

        protected override IEnumerable<MethodInfo> DeclaredMembers(Type type)
        {
            return type.GetDeclaredMethods()
                       .Where(member => !member.IsFamily && 
                                        !member.IsPrivate && 
                                        !member.IsStatic);
        }

        public override MemberSelector<MethodInfo> GetOrDefault(IPolicySet? registration) => 
            registration?.Get<MemberSelector<MethodInfo>>() ?? Defaults.SelectMethod;

        #endregion


        #region Resolution

        protected override ResolveDelegate<PipelineContext> GetResolverDelegate(MethodInfo info, object? resolvers)
        {
            var parameterResolvers = ParameterResolvers(info.GetParameters(), resolvers).ToArray();
            return (ref PipelineContext c) =>
            {
                if (null == c.Existing) return c.Existing;

                var parameters = new object[parameterResolvers.Length];
                for (var i = 0; i < parameters.Length; i++)
                    parameters[i] = parameterResolvers[i](ref c);

                info.Invoke(c.Existing, parameters);

                return c.Existing;
            };
        }

        #endregion


        #region Expression 

        protected override Expression GetResolverExpression(MethodInfo info, object? resolvers)
        {
            var parameters = info.GetParameters();
            var variables = parameters.Select(ToVariable)
                                      .ToArray();

            return Expression.Block(variables, ParameterExpressions(variables, parameters, resolvers)
                                              .Concat(new[] {
                                                  Expression.Call(
                                                      Expression.Convert(PipelineContextExpression.Existing, info.DeclaringType), 
                                                      info, variables) }));
        }

        #endregion
    }
}
