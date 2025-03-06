// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.ValueGeneration.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class XuGuDbSequenceValueGeneratorFactory : IXuGuDbSequenceValueGeneratorFactory
    {
        private readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;
        private readonly IXuGuDbUpdateSqlGenerator _sqlGenerator;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public XuGuDbSequenceValueGeneratorFactory(
            [NotNull] IRawSqlCommandBuilder rawSqlCommandBuilder,
            [NotNull] IXuGuDbUpdateSqlGenerator sqlGenerator)
        {
            Check.NotNull(rawSqlCommandBuilder, nameof(rawSqlCommandBuilder));
            Check.NotNull(sqlGenerator, nameof(sqlGenerator));

            _rawSqlCommandBuilder = rawSqlCommandBuilder;
            _sqlGenerator = sqlGenerator;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ValueGenerator Create(IProperty property, XuGuDbSequenceValueGeneratorState generatorState, IXuGuDbConnection connection)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(generatorState, nameof(generatorState));
            Check.NotNull(connection, nameof(connection));

            var type = property.ClrType.UnwrapNullableType().UnwrapEnumType();

            if (type == typeof(long))
            {
                return new XuGuDbSequenceHiLoValueGenerator<long>(_rawSqlCommandBuilder, _sqlGenerator, generatorState, connection);
            }

            if (type == typeof(int))
            {
                return new XuGuDbSequenceHiLoValueGenerator<int>(_rawSqlCommandBuilder, _sqlGenerator, generatorState, connection);
            }

            if (type == typeof(short))
            {
                return new XuGuDbSequenceHiLoValueGenerator<short>(_rawSqlCommandBuilder, _sqlGenerator, generatorState, connection);
            }

            if (type == typeof(byte))
            {
                return new XuGuDbSequenceHiLoValueGenerator<byte>(_rawSqlCommandBuilder, _sqlGenerator, generatorState, connection);
            }

            if (type == typeof(char))
            {
                return new XuGuDbSequenceHiLoValueGenerator<char>(_rawSqlCommandBuilder, _sqlGenerator, generatorState, connection);
            }

            if (type == typeof(ulong))
            {
                return new XuGuDbSequenceHiLoValueGenerator<ulong>(_rawSqlCommandBuilder, _sqlGenerator, generatorState, connection);
            }

            if (type == typeof(uint))
            {
                return new XuGuDbSequenceHiLoValueGenerator<uint>(_rawSqlCommandBuilder, _sqlGenerator, generatorState, connection);
            }

            if (type == typeof(ushort))
            {
                return new XuGuDbSequenceHiLoValueGenerator<ushort>(_rawSqlCommandBuilder, _sqlGenerator, generatorState, connection);
            }

            if (type == typeof(sbyte))
            {
                return new XuGuDbSequenceHiLoValueGenerator<sbyte>(_rawSqlCommandBuilder, _sqlGenerator, generatorState, connection);
            }

            throw new ArgumentException(CoreStrings.InvalidValueGeneratorFactoryProperty(
                nameof(XuGuDbSequenceValueGeneratorFactory), property.Name, property.DeclaringEntityType.DisplayName()));
        }
    }
}
