// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EntityFrameworkCore.XuGu.Infrastructure.Internal;
using EntityFrameworkCore.XuGu.Storage.Internal;
using XuguClient;

namespace EntityFrameworkCore.XuGu.Json.Newtonsoft.Storage.Internal
{
    public class XGJsonNewtonsoftTypeMapping<T> : XGJsonTypeMapping<T>
    {
        // Called via reflection.
        // ReSharper disable once UnusedMember.Global
        public XGJsonNewtonsoftTypeMapping(
            [NotNull] string storeType,
            [CanBeNull] ValueConverter valueConverter,
            [CanBeNull] ValueComparer valueComparer,
            [NotNull] IXGOptions options)
            : base(
                storeType,
                valueConverter,
                valueComparer,
                options)
        {
        }

        protected XGJsonNewtonsoftTypeMapping(
            RelationalTypeMappingParameters parameters,
            XGDbType XGDbType,
            IXGOptions options)
            : base(parameters, XGDbType, options)
        {
        }

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new XGJsonNewtonsoftTypeMapping<T>(parameters, XGDbType, Options);

        public override Expression GenerateCodeLiteral(object value)
        {
            var defaultJsonLoadSettings = new Lazy<Expression>(() => Expression.New(typeof(JsonLoadSettings)));
            var parseMethod = new Lazy<MethodInfo>(() => typeof(JToken).GetMethod(nameof(JToken.Parse), new[] {typeof(string), typeof(JsonLoadSettings)}));

            return value switch
            {
                JToken jToken => Expression.Call(parseMethod.Value, Expression.Constant(jToken.ToString(Formatting.None)), defaultJsonLoadSettings.Value),
                string s => Expression.Constant(s),
                _ => throw new NotSupportedException("Cannot generate code literals for JSON POCOs.")
            };
        }
    }
}
