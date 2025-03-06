// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     XuGuDb specific extension methods for <see cref="ModelBuilder"/>.
    /// </summary>
    public static class XuGuDbModelBuilderExtensions
    {
        /// <summary>
        ///     Configures a database sequence when targeting XuGuDb.
        /// </summary>
        /// <param name="modelBuilder"> The model builder. </param>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="schema">The schema of the sequence. </param>
        /// <returns> A builder to further configure the sequence. </returns>
        public static RelationalSequenceBuilder ForXuGuDbHasSequence(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] string name,
            [CanBeNull] string schema = null)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            return new RelationalSequenceBuilder(modelBuilder.Model.XuGuDb().GetOrAddSequence(name, schema));
        }

        /// <summary>
        ///     Configures a database sequence when targeting XuGuDb.
        /// </summary>
        /// <param name="modelBuilder"> The model builder. </param>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="builderAction"> An action that performs configuration of the sequence. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ModelBuilder ForXuGuDbHasSequence(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] string name,
            [NotNull] Action<RelationalSequenceBuilder> builderAction)
            => modelBuilder.ForXuGuDbHasSequence(name, null, builderAction);

        /// <summary>
        ///     Configures a database sequence when targeting XuGuDb.
        /// </summary>
        /// <param name="modelBuilder"> The model builder. </param>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="schema">The schema of the sequence. </param>
        /// <param name="builderAction"> An action that performs configuration of the sequence. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ModelBuilder ForXuGuDbHasSequence(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] string name,
            [CanBeNull] string schema,
            [NotNull] Action<RelationalSequenceBuilder> builderAction)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));
            Check.NotNull(builderAction, nameof(builderAction));

            builderAction(ForXuGuDbHasSequence(modelBuilder, name, schema));

            return modelBuilder;
        }

        /// <summary>
        ///     Configures a database sequence when targeting XuGuDb.
        /// </summary>
        /// <typeparam name="T"> The type of values the sequence will generate. </typeparam>
        /// <param name="modelBuilder"> The model builder. </param>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="schema">The schema of the sequence. </param>
        /// <returns> A builder to further configure the sequence. </returns>
        public static RelationalSequenceBuilder ForXuGuDbHasSequence<T>(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] string name,
            [CanBeNull] string schema = null)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            var sequence = modelBuilder.Model.XuGuDb().GetOrAddSequence(name, schema);
            sequence.ClrType = typeof(T);

            return new RelationalSequenceBuilder(sequence);
        }

        /// <summary>
        ///     Configures a database sequence when targeting XuGuDb.
        /// </summary>
        /// <typeparam name="T"> The type of values the sequence will generate. </typeparam>
        /// <param name="modelBuilder"> The model builder. </param>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="builderAction"> An action that performs configuration of the sequence. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ModelBuilder ForXuGuDbHasSequence<T>(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] string name,
            [NotNull] Action<RelationalSequenceBuilder> builderAction)
            => modelBuilder.ForXuGuDbHasSequence<T>(name, null, builderAction);

        /// <summary>
        ///     Configures a database sequence when targeting XuGuDb.
        /// </summary>
        /// <typeparam name="T"> The type of values the sequence will generate. </typeparam>
        /// <param name="modelBuilder"> The model builder. </param>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="schema">The schema of the sequence. </param>
        /// <param name="builderAction"> An action that performs configuration of the sequence. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ModelBuilder ForXuGuDbHasSequence<T>(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] string name,
            [CanBeNull] string schema,
            [NotNull] Action<RelationalSequenceBuilder> builderAction)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));
            Check.NotNull(builderAction, nameof(builderAction));

            builderAction(ForXuGuDbHasSequence<T>(modelBuilder, name, schema));

            return modelBuilder;
        }

        /// <summary>
        ///     Configures a database sequence when targeting XuGuDb.
        /// </summary>
        /// <param name="clrType"> The type of values the sequence will generate. </param>
        /// <param name="modelBuilder"> The model builder. </param>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="schema">The schema of the sequence. </param>
        /// <returns> A builder to further configure the sequence. </returns>
        public static RelationalSequenceBuilder ForXuGuDbHasSequence(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] Type clrType,
            [NotNull] string name,
            [CanBeNull] string schema = null)
        {
            Check.NotNull(clrType, nameof(clrType));
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            var sequence = modelBuilder.Model.XuGuDb().GetOrAddSequence(name, schema);
            sequence.ClrType = clrType;

            return new RelationalSequenceBuilder(sequence);
        }

        /// <summary>
        ///     Configures a database sequence when targeting XuGuDb.
        /// </summary>
        /// <param name="clrType"> The type of values the sequence will generate. </param>
        /// <param name="modelBuilder"> The model builder. </param>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="builderAction"> An action that performs configuration of the sequence. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ModelBuilder ForXuGuDbHasSequence(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] Type clrType,
            [NotNull] string name,
            [NotNull] Action<RelationalSequenceBuilder> builderAction)
            => modelBuilder.ForXuGuDbHasSequence(clrType, name, null, builderAction);

        /// <summary>
        ///     Configures a database sequence when targeting XuGuDb.
        /// </summary>
        /// <param name="clrType"> The type of values the sequence will generate. </param>
        /// <param name="modelBuilder"> The model builder. </param>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="schema">The schema of the sequence. </param>
        /// <param name="builderAction"> An action that performs configuration of the sequence. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ModelBuilder ForXuGuDbHasSequence(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] Type clrType,
            [NotNull] string name,
            [CanBeNull] string schema,
            [NotNull] Action<RelationalSequenceBuilder> builderAction)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotNull(clrType, nameof(clrType));
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));
            Check.NotNull(builderAction, nameof(builderAction));

            builderAction(ForXuGuDbHasSequence(modelBuilder, clrType, name, schema));

            return modelBuilder;
        }

        /// <summary>
        ///     Configures the model to use a sequence-based hi-lo pattern to generate values for properties
        ///     marked as <see cref="ValueGenerated.OnAdd"/>, when targeting XuGuDb.
        /// </summary>
        /// <param name="modelBuilder"> The model builder. </param>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="schema">The schema of the sequence. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ModelBuilder ForXuGuDbUseSequenceHiLo(
            [NotNull] this ModelBuilder modelBuilder,
            [CanBeNull] string name = null,
            [CanBeNull] string schema = null)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            var model = modelBuilder.Model;

            name = name ?? XuGuDbModelAnnotations.DefaultHiLoSequenceName;

            if (model.XuGuDb().FindSequence(name, schema) == null)
            {
                modelBuilder.ForXuGuDbHasSequence(name, schema).IncrementsBy(10);
            }

            model.XuGuDb().ValueGenerationStrategy = XuGuDbValueGenerationStrategy.SequenceHiLo;
            model.XuGuDb().HiLoSequenceName = name;
            model.XuGuDb().HiLoSequenceSchema = schema;

            return modelBuilder;
        }

        /// <summary>
        ///     Configures the model to use the XuGuDb IDENTITY feature to generate values for properties
        ///     marked as <see cref="ValueGenerated.OnAdd"/>, when targeting XuGuDb. This is the default
        ///     behavior when targeting XuGuDb.
        /// </summary>
        /// <param name="modelBuilder"> The model builder. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ModelBuilder ForXuGuDbUseIdentityColumns(
            [NotNull] this ModelBuilder modelBuilder)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));

            var property = modelBuilder.Model;

            property.XuGuDb().ValueGenerationStrategy = XuGuDbValueGenerationStrategy.IdentityColumn;
            property.XuGuDb().HiLoSequenceName = null;
            property.XuGuDb().HiLoSequenceSchema = null;

            return modelBuilder;
        }
    }
}
