using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Extensions
{
    public static class XGPropertyBuilderExtensions
    {
        /// <summary>
        ///     Configures the column that the property maps to when targeting XG.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="name"> The name of the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder ForXGHasColumnName(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string name)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            propertyBuilder.Metadata.XG().ColumnName = name;

            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the column that the property maps to when targeting XG.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured. </typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="name"> The name of the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder<TProperty> ForXGHasColumnName<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string name)
            => (PropertyBuilder<TProperty>)ForXGHasColumnName((PropertyBuilder)propertyBuilder, name);

        /// <summary>
        ///     Configures the data type of the column that the property maps to when targeting XG.
        ///     This should be the complete type name, including precision, scale, length, etc. 
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="typeName"> The name of the data type of the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder ForXGHasColumnType(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string typeName)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(typeName, nameof(typeName));

            propertyBuilder.Metadata.XG().ColumnType = typeName;

            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the data type of the column that the property maps to when targeting XG.
        ///     This should be the complete type name, including precision, scale, length, etc. 
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured. </typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="typeName"> The name of the data type of the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder<TProperty> ForXGHasColumnType<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string typeName)
            => (PropertyBuilder<TProperty>)ForXGHasColumnType((PropertyBuilder)propertyBuilder, typeName);

        /// <summary>
        ///     Configures the default value for the column that the property maps to when targeting XG.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="value"> The default value of the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder ForXGHasDefaultValue(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] object value)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            var internalPropertyBuilder = propertyBuilder.GetInfrastructure<InternalPropertyBuilder>();
            internalPropertyBuilder.XG(ConfigurationSource.Explicit).DefaultValue(value);

            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the default value for the column that the property maps to when targeting XG.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured. </typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="value"> The default value of the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder<TProperty> ForXGHasDefaultValue<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] object value)
            => (PropertyBuilder<TProperty>)ForXGHasDefaultValue((PropertyBuilder)propertyBuilder, value);

        /// <summary>
        ///     Configures the property to use a sequence-based hi-lo pattern to generate values for new entities, 
        ///     when targeting XG. This method sets the property to be <see cref="ValueGenerated.OnAdd"/>.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="schema"> The schema of the sequence. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder ForXGUseSequenceHiLo(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string name = null,
            [CanBeNull] string schema = null)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            var property = propertyBuilder.Metadata;

            name = name ?? XGModelAnnotations.DefaultHiLoSequenceName;

            var model = property.DeclaringEntityType.Model;

            if (model.XG().FindSequence(name, schema) == null)
            {
                model.XG().GetOrAddSequence(name, schema).IncrementBy = 10;
            }

            property.XG().ValueGenerationStrategy = XGValueGenerationStrategy.SequenceHiLo;
            property.ValueGenerated = ValueGenerated.OnAdd;
            property.RequiresValueGenerator = true;
            property.XG().HiLoSequenceName = name;
            property.XG().HiLoSequenceSchema = schema;

            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the property to use a sequence-based hi-lo pattern to generate values for new entities, 
        ///     when targeting XG. This method sets the property to be <see cref="ValueGenerated.OnAdd"/>.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured. </typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="schema"> The schema of the sequence. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder<TProperty> ForXGUseSequenceHiLo<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string name = null,
            [CanBeNull] string schema = null)
            => (PropertyBuilder<TProperty>)ForXGUseSequenceHiLo((PropertyBuilder)propertyBuilder, name, schema);

        /// <summary>
        ///     Configures the property to use the SQL Server IDENTITY feature to generate values for new entities, 
        ///     when targeting XG. This method sets the property to be <see cref="ValueGenerated.OnAdd"/>.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder UseXGIdentityColumn(
            [NotNull] this PropertyBuilder propertyBuilder)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            var property = propertyBuilder.Metadata;

            property.XG().ValueGenerationStrategy = XGValueGenerationStrategy.IdentityColumn;
            property.ValueGenerated = ValueGenerated.OnAdd;
            property.RequiresValueGenerator = true;
            property.XG().HiLoSequenceName = null;
            property.XG().HiLoSequenceSchema = null;

            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the property to use the SQL Server IDENTITY feature to generate values for new entities, 
        ///     when targeting XG. This method sets the property to be <see cref="ValueGenerated.OnAdd"/>.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured. </typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder<TProperty> UseXGIdentityColumn<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder)
            => (PropertyBuilder<TProperty>)UseXGIdentityColumn((PropertyBuilder)propertyBuilder);

        /// <summary>
        ///     Configures the property to map to a computed column when targeting SQL Server.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="sql"> The SQL expression that computes values for the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder ForXGHasComputedColumnSql(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string sql)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(sql, nameof(sql));

            var internalPropertyBuilder = propertyBuilder.GetInfrastructure<InternalPropertyBuilder>();
            internalPropertyBuilder.XG(ConfigurationSource.Explicit).ComputedColumnSql(sql);

            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the property to map to a computed column when targeting SQL Server.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured. </typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="sql"> The SQL expression that computes values for the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder<TProperty> ForXGHasComputedColumnSql<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string sql)
            => (PropertyBuilder<TProperty>)ForXGHasComputedColumnSql((PropertyBuilder)propertyBuilder, sql);
    }
}
