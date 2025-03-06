// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.ValueGeneration.Internal
{
    public class XuGuDbValueGeneratorSelector : RelationalValueGeneratorSelector
    {
        private readonly IXuGuDbSequenceValueGeneratorFactory _sequenceFactory;

        private readonly IXuGuDbConnection _connection;

        public XuGuDbValueGeneratorSelector(
            [NotNull] IXuGuDbValueGeneratorCache cache,
            [NotNull] IXuGuDbSequenceValueGeneratorFactory sequenceFactory,
            [NotNull] IXuGuDbConnection connection,
            [NotNull] IRelationalAnnotationProvider relationalExtensions)
            : base(cache, relationalExtensions)
        {
            Check.NotNull(sequenceFactory, nameof(sequenceFactory));
            Check.NotNull(connection, nameof(connection));

            _sequenceFactory = sequenceFactory;
            _connection = connection;
        }

        public new virtual IXuGuDbValueGeneratorCache Cache => (IXuGuDbValueGeneratorCache)base.Cache;

        public override ValueGenerator Select(IProperty property, IEntityType entityType)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(entityType, nameof(entityType));

            return property.XuGuDb().ValueGenerationStrategy == XuGuDbValueGenerationStrategy.SequenceHiLo
                ? _sequenceFactory.Create(property, Cache.GetOrAddSequenceState(property), _connection)
                : Cache.GetOrAdd(property, entityType, Create);
        }

        public override ValueGenerator Create(IProperty property, IEntityType entityType)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(entityType, nameof(entityType));

            return property.ClrType.UnwrapNullableType() == typeof(Guid)
                ? property.ValueGenerated == ValueGenerated.Never
                  || property.XuGuDb().DefaultValueSql != null
                    ? (ValueGenerator)new TemporaryGuidValueGenerator()
                    : new SequentialGuidValueGenerator()
                : base.Create(property, entityType);
        }
    }
}
