// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using XuguClient;
using EntityFrameworkCore.XuGu.Infrastructure.Internal;

namespace EntityFrameworkCore.XuGu.Storage.Internal
{
    public class XGJsonTypeMapping<T> : XGJsonTypeMapping
    {
        public XGJsonTypeMapping(
            [NotNull] string storeType,
            [CanBeNull] ValueConverter valueConverter,
            [CanBeNull] ValueComparer valueComparer,
            [NotNull] IXGOptions options)
            : base(
                storeType,
                typeof(T),
                valueConverter,
                valueComparer,
                options)
        {
        }

        protected XGJsonTypeMapping(
            RelationalTypeMappingParameters parameters,
            XGDbType XGDbType,
            IXGOptions options)
            : base(parameters, XGDbType, options)
        {
        }

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new XGJsonTypeMapping<T>(parameters, XGDbType, Options);
    }

    public abstract class XGJsonTypeMapping : XGStringTypeMapping
    {
        [NotNull]
        protected IXGOptions Options { get; }

        public XGJsonTypeMapping(
            [NotNull] string storeType,
            [NotNull] Type clrType,
            [CanBeNull] ValueConverter valueConverter,
            [CanBeNull] ValueComparer valueComparer,
            [NotNull] IXGOptions options)
            : base(
                new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(
                        clrType,
                        valueConverter,
                        valueComparer),
                    storeType,
                    unicode: true),
                XGDbType.Json,
                options,
                false,
                false)
        {
            if (storeType != "json")
            {
                throw new ArgumentException($"The store type '{nameof(storeType)}' must be 'json'.", nameof(storeType));
            }

            Options = options;
        }

        protected XGJsonTypeMapping(
            RelationalTypeMappingParameters parameters,
            XGDbType XGDbType,
            IXGOptions options)
            : base(parameters, XGDbType, options, false, false)
        {
            Options = options;
        }

        protected override void ConfigureParameter(DbParameter parameter)
        {
            base.ConfigureParameter(parameter);

            // XuguClient does not know how to handle our custom XGJsonString type, that could be used when a
            // string parameter is explicitly cast to it.
            if (parameter.Value is XGJsonString XGJsonString)
            {
                parameter.Value = (string)XGJsonString;
            }
        }
    }
}
