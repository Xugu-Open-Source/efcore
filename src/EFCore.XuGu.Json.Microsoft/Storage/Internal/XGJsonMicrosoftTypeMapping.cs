// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using EntityFrameworkCore.XuGu.Infrastructure.Internal;
using EntityFrameworkCore.XuGu.Storage.Internal;
using XuguClient;

namespace EntityFrameworkCore.XuGu.Json.Microsoft.Storage.Internal
{
    public class XGJsonMicrosoftTypeMapping<T> : XGJsonTypeMapping<T>
    {
        // Called via reflection.
        // ReSharper disable once UnusedMember.Global
        public XGJsonMicrosoftTypeMapping(
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

        protected XGJsonMicrosoftTypeMapping(
            RelationalTypeMappingParameters parameters,
            XGDbType XGDbType,
            IXGOptions options)
            : base(parameters, XGDbType, options)
        {
        }

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new XGJsonMicrosoftTypeMapping<T>(parameters, XGDbType, Options);

        public override Expression GenerateCodeLiteral(object value)
        {
            var defaultJsonDocumentOptions = new Lazy<Expression>(() => Expression.New(typeof(JsonDocumentOptions)));
            var parseMethod = new Lazy<MethodInfo>(() => typeof(JsonDocument).GetMethod(nameof(JsonDocument.Parse), new[] {typeof(string), typeof(JsonDocumentOptions)}));

            return value switch
            {
                JsonDocument document => Expression.Call(parseMethod.Value, Expression.Constant(document.RootElement.ToString()), defaultJsonDocumentOptions.Value),
                JsonElement element => Expression.Property(
                    Expression.Call(parseMethod.Value, Expression.Constant(element.ToString()), defaultJsonDocumentOptions.Value),
                    nameof(JsonDocument.RootElement)),
                string s => Expression.Constant(s),
                _ => throw new NotSupportedException("Cannot generate code literals for JSON POCOs.")
            };
        }
    }
}
