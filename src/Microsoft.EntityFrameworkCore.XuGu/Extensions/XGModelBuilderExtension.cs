// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace XuGu.EntityFrameworkCore.Extensions
{
    public static class XGModelBuilderExtension
    {
        public static ModelBuilder ForXGUseIdentityColumns(
            [NotNull] this ModelBuilder modelBuilder)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));

            var property = modelBuilder.Model;

            return modelBuilder;
        }
        public static RelationalSequenceBuilder ForXGHasSequence(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] string name,
            [CanBeNull] string schema = null)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            return new RelationalSequenceBuilder(modelBuilder.Model.XG().GetOrAddSequence(name, schema));
        }
        public static ModelBuilder ForXGUseSequenceHiLo(
            [NotNull] this ModelBuilder modelBuilder,
            [CanBeNull] string name = null,
            [CanBeNull] string schema = null)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            var model = modelBuilder.Model;

            name = name ?? XGModelAnnotations.DefaultHiLoSequenceName;

            if (model.XG().FindSequence(name, schema) == null)
            {
                modelBuilder.ForXGHasSequence(name, schema).IncrementsBy(10);
            }

            model.XG().ValueGenerationStrategy = XGValueGenerationStrategy.SequenceHiLo;
            model.XG().HiLoSequenceName = name;
            model.XG().HiLoSequenceSchema = schema;

            return modelBuilder;
        }
    }
}
