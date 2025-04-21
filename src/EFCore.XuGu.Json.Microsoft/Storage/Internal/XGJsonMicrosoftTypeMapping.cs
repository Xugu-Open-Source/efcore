// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq.Expressions;
using System.Text.Json;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using XuguClient;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Json.Microsoft.Storage.Internal
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
            XGDbType xgDbType,
            IXGOptions options)
            : base(parameters, xgDbType, options)
        {
        }

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new XGJsonMicrosoftTypeMapping<T>(parameters, XGDbType, Options);

        public override Expression GenerateCodeLiteral(object value)
            => value switch
            {
                JsonDocument document => Expression.Call(
                    typeof(JsonDocument).GetMethod(nameof(JsonDocument.Parse), new[] {typeof(string), typeof(JsonDocumentOptions)}),
                    Expression.Constant(document.RootElement.ToString()),
                    Expression.New(typeof(JsonDocumentOptions))),
                JsonElement element => Expression.Property(
                    Expression.Call(
                        typeof(JsonDocument).GetMethod(nameof(JsonDocument.Parse), new[] {typeof(string), typeof(JsonDocumentOptions)}),
                        Expression.Constant(element.ToString()),
                        Expression.New(typeof(JsonDocumentOptions))),
                    nameof(JsonDocument.RootElement)),
                string s => Expression.Constant(s),
                _ => throw new NotSupportedException("Cannot generate code literals for JSON POCOs.")
            };
    }
}
