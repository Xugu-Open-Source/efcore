// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using XuguClient;

namespace EntityFrameworkCore.XuGu.Storage.Internal
{
    // TODO: Use as base class for all type mappings.
    /// <summary>
    /// The base class for mapping XG-specific types. It configures parameters with the
    /// <see cref="XGDbType"/> provider-specific type enum.
    /// </summary>
    public abstract class XGTypeMapping : RelationalTypeMapping
    {
        /// <summary>
        /// The database type used by XG.
        /// </summary>
        public virtual XGDbType XGDbType { get; }

        // ReSharper disable once PublicConstructorInAbstractClass
        public XGTypeMapping(
            [NotNull] string storeType,
            [NotNull] Type clrType,
            XGDbType XGDbType,
            DbType? dbType = null,
            bool unicode = false,
            int? size = null,
            ValueConverter valueConverter = null,
            ValueComparer valueComparer = null)
            : base(
                new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(clrType, valueConverter, valueComparer), storeType, StoreTypePostfix.None, dbType, unicode, size))
            => XGDbType = XGDbType;

        /// <summary>
        /// Constructs an instance of the <see cref="XGTypeMapping"/> class.
        /// </summary>
        /// <param name="parameters">The parameters for this mapping.</param>
        /// <param name="XGDbType">The database type of the range subtype.</param>
        protected XGTypeMapping(RelationalTypeMappingParameters parameters, XGDbType XGDbType)
            : base(parameters)
            => XGDbType = XGDbType;

        protected override void ConfigureParameter(DbParameter parameter)
        {
            if (!(parameter is XGParameters XGParameters))
            {
                throw new ArgumentException($"XG-specific type mapping {GetType()} being used with non-XG parameter type {parameter.GetType().Name}");
            }

            base.ConfigureParameter(parameter);

            XGParameters.m_DbType = XGDbType;
        }
    }
}
