// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     XuGuDb specific extension methods for <see cref="PropertyBuilder"/>.
    /// </summary>
    public static class XuGuDbPropertyBuilderExtensions
    {
        /// <summary>
        ///     Configures the column that the property maps to when targeting XuGuDb.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="name"> The name of the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder ForXuGuDbHasColumnName(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string name)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            propertyBuilder.Metadata.XuGuDb().ColumnName = name;

            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the column that the property maps to when targeting XuGuDb.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured. </typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="name"> The name of the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder<TProperty> ForXuGuDbHasColumnName<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string name)
            => (PropertyBuilder<TProperty>)ForXuGuDbHasColumnName((PropertyBuilder)propertyBuilder, name);

        /// <summary>
        ///     Configures the data type of the column that the property maps to when targeting XuGuDb.
        ///     This should be the complete type name, including precision, scale, length, etc. 
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="typeName"> The name of the data type of the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder ForXuGuDbHasColumnType(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string typeName)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(typeName, nameof(typeName));

            propertyBuilder.Metadata.XuGuDb().ColumnType = typeName;

            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the data type of the column that the property maps to when targeting XuGuDb.
        ///     This should be the complete type name, including precision, scale, length, etc. 
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured. </typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="typeName"> The name of the data type of the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder<TProperty> ForXuGuDbHasColumnType<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string typeName)
            => (PropertyBuilder<TProperty>)ForXuGuDbHasColumnType((PropertyBuilder)propertyBuilder, typeName);

        /// <summary>
        ///     Configures the default value for the column that the property maps to when targeting XuGuDb.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="value"> The default value of the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder ForXuGuDbHasDefaultValue(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] object value)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            var internalPropertyBuilder = propertyBuilder.GetInfrastructure<InternalPropertyBuilder>();
            internalPropertyBuilder.XuGuDb(ConfigurationSource.Explicit).DefaultValue(value);

            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the default value for the column that the property maps to when targeting XuGuDb.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured. </typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="value"> The default value of the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder<TProperty> ForXuGuDbHasDefaultValue<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] object value)
            => (PropertyBuilder<TProperty>)ForXuGuDbHasDefaultValue((PropertyBuilder)propertyBuilder, value);

        /// <summary>
        ///     Configures the property to use a sequence-based hi-lo pattern to generate values for new entities, 
        ///     when targeting XuGuDb. This method sets the property to be <see cref="ValueGenerated.OnAdd"/>.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="schema"> The schema of the sequence. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder ForXuGuDbUseSequenceHiLo(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string name = null,
            [CanBeNull] string schema = null)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            var property = propertyBuilder.Metadata;

            name = name ?? XuGuDbModelAnnotations.DefaultHiLoSequenceName;

            var model = property.DeclaringEntityType.Model;

            if (model.XuGuDb().FindSequence(name, schema) == null)
            {
                model.XuGuDb().GetOrAddSequence(name, schema).IncrementBy = 10;
            }

            property.XuGuDb().ValueGenerationStrategy = XuGuDbValueGenerationStrategy.SequenceHiLo;
            property.ValueGenerated = ValueGenerated.OnAdd;
            property.RequiresValueGenerator = true;
            property.XuGuDb().HiLoSequenceName = name;
            property.XuGuDb().HiLoSequenceSchema = schema;

            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the property to use a sequence-based hi-lo pattern to generate values for new entities, 
        ///     when targeting XuGuDb. This method sets the property to be <see cref="ValueGenerated.OnAdd"/>.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured. </typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="schema"> The schema of the sequence. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder<TProperty> ForXuGuDbUseSequenceHiLo<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string name = null,
            [CanBeNull] string schema = null)
            => (PropertyBuilder<TProperty>)ForXuGuDbUseSequenceHiLo((PropertyBuilder)propertyBuilder, name, schema);

        /// <summary>
        ///     Configures the property to use the SQL Server IDENTITY feature to generate values for new entities, 
        ///     when targeting XuGuDb. This method sets the property to be <see cref="ValueGenerated.OnAdd"/>.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder UseXuGuDbIdentityColumn(
            [NotNull] this PropertyBuilder propertyBuilder)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            var property = propertyBuilder.Metadata;

            property.XuGuDb().ValueGenerationStrategy = XuGuDbValueGenerationStrategy.IdentityColumn;
            property.ValueGenerated = ValueGenerated.OnAdd;
            property.RequiresValueGenerator = true;
            property.XuGuDb().HiLoSequenceName = null;
            property.XuGuDb().HiLoSequenceSchema = null;

            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the property to use the SQL Server IDENTITY feature to generate values for new entities, 
        ///     when targeting XuGuDb. This method sets the property to be <see cref="ValueGenerated.OnAdd"/>.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured. </typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder<TProperty> UseXuGuDbIdentityColumn<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder)
            => (PropertyBuilder<TProperty>)UseXuGuDbIdentityColumn((PropertyBuilder)propertyBuilder);

        /// <summary>
        ///     Configures the property to map to a computed column when targeting SQL Server.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="sql"> The SQL expression that computes values for the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder ForXuGuDbHasComputedColumnSql(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string sql)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(sql, nameof(sql));

            var internalPropertyBuilder = propertyBuilder.GetInfrastructure<InternalPropertyBuilder>();
            internalPropertyBuilder.XuGuDb(ConfigurationSource.Explicit).ComputedColumnSql(sql);

            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the property to map to a computed column when targeting SQL Server.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured. </typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="sql"> The SQL expression that computes values for the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder<TProperty> ForXuGuDbHasComputedColumnSql<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string sql)
            => (PropertyBuilder<TProperty>)ForXuGuDbHasComputedColumnSql((PropertyBuilder)propertyBuilder, sql);
    }
}
