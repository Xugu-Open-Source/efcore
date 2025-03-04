// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Linq;
using EntityFrameworkCore.XuGu.Infrastructure.Internal;
using EntityFrameworkCore.XuGu.Storage.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using EntityFrameworkCore.XuGu.Infrastructure;
using Microsoft.EntityFrameworkCore.Diagnostics;
using XuguClient;

namespace EntityFrameworkCore.XuGu.Internal
{
    public class XGOptions : IXGOptions
    {
        private static readonly XGSchemaNameTranslator _ignoreSchemaNameTranslator = (_, objectName) => objectName;

        public XGOptions()
        {
            ConnectionSettings = new XGConnectionSettings();
            ServerVersion = null;

            // We explicitly use `utf8mb4` in all instances, where charset based calculations need to be done, but accessing annotations
            // isn't possible (e.g. in `XGTypeMappingSource`).
            // This is also being used as the universal fallback character set, if no character set was explicitly defined for the model,
            // which will result in similar behavior as in previous versions and ensure that databases use a decent/the recommended charset
            // by default, if none was explicitly set.
            DefaultCharSet = CharSet.Utf8Mb4;

            // NCHAR and NVARCHAR are prefdefined by XG.
            NationalCharSet = CharSet.Utf8Mb3;

            // Optimize space and performance for GUID columns.
            DefaultGuidCollation = "ascii_general_ci";

            ReplaceLineBreaksWithCharFunction = true;
            DefaultDataTypeMappings = new XGDefaultDataTypeMappings();

            // Throw by default if a schema is being used with any type.
            SchemaNameTranslator = null;

            // TODO: Change to `true` for EF Core 5.
            IndexOptimizedBooleanColumns = false;

            LimitKeyedOrIndexedStringColumnLength = true;
            StringComparisonTranslations = false;
        }

        public virtual void Initialize(IDbContextOptions options)
        {
            var XGOptions = options.FindExtension<XGOptionsExtension>() ?? new XGOptionsExtension();
            var XGJsonOptions = (XGJsonOptionsExtension)options.Extensions.LastOrDefault(e => e is XGJsonOptionsExtension);

            ConnectionSettings = GetConnectionSettings(XGOptions);
            ServerVersion = XGOptions.ServerVersion ?? new XGServerVersion("12.0.0.0");
            NoBackslashEscapes = XGOptions.NoBackslashEscapes;
            ReplaceLineBreaksWithCharFunction = XGOptions.ReplaceLineBreaksWithCharFunction;
            DefaultDataTypeMappings = ApplyDefaultDataTypeMappings(XGOptions.DefaultDataTypeMappings, ConnectionSettings);
            SchemaNameTranslator = XGOptions.SchemaNameTranslator ?? (XGOptions.SchemaBehavior == XGSchemaBehavior.Ignore
                ? _ignoreSchemaNameTranslator
                : null);
            IndexOptimizedBooleanColumns = XGOptions.IndexOptimizedBooleanColumns;
            JsonChangeTrackingOptions = XGJsonOptions?.JsonChangeTrackingOptions ?? default;
            LimitKeyedOrIndexedStringColumnLength = XGOptions.LimitKeyedOrIndexedStringColumnLength;
            StringComparisonTranslations = XGOptions.StringComparisonTranslations;
        }

        public virtual void Validate(IDbContextOptions options)
        {
            var XGOptions = options.FindExtension<XGOptionsExtension>() ?? new XGOptionsExtension();
            var XGJsonOptions = (XGJsonOptionsExtension)options.Extensions.LastOrDefault(e => e is XGJsonOptionsExtension);
            var connectionSettings = GetConnectionSettings(XGOptions);

            //if (!Equals(ServerVersion, XGOptions.ServerVersion))
            //{
            //    throw new InvalidOperationException(
            //        CoreStrings.SingletonOptionChanged(
            //            nameof(XGOptionsExtension.ServerVersion),
            //            nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
            //}


            if (!Equals(NoBackslashEscapes, XGOptions.NoBackslashEscapes))
            {
                throw new InvalidOperationException(
                    CoreStrings.SingletonOptionChanged(
                        nameof(XGDbContextOptionsBuilder.DisableBackslashEscaping),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
            }

            if (!Equals(ReplaceLineBreaksWithCharFunction, XGOptions.ReplaceLineBreaksWithCharFunction))
            {
                throw new InvalidOperationException(
                    CoreStrings.SingletonOptionChanged(
                        nameof(XGDbContextOptionsBuilder.DisableLineBreakToCharSubstition),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
            }

            if (!Equals(DefaultDataTypeMappings, ApplyDefaultDataTypeMappings(XGOptions.DefaultDataTypeMappings ?? new XGDefaultDataTypeMappings(), connectionSettings)))
            {
                throw new InvalidOperationException(
                    CoreStrings.SingletonOptionChanged(
                        nameof(XGDbContextOptionsBuilder.DefaultDataTypeMappings),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
            }

            if (!Equals(
                SchemaNameTranslator,
                XGOptions.SchemaBehavior == XGSchemaBehavior.Ignore
                    ? _ignoreSchemaNameTranslator
                    : SchemaNameTranslator))
            {
                throw new InvalidOperationException(
                    CoreStrings.SingletonOptionChanged(
                        nameof(XGDbContextOptionsBuilder.SchemaBehavior),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
            }

            if (!Equals(IndexOptimizedBooleanColumns, XGOptions.IndexOptimizedBooleanColumns))
            {
                throw new InvalidOperationException(
                    CoreStrings.SingletonOptionChanged(
                        nameof(XGDbContextOptionsBuilder.EnableIndexOptimizedBooleanColumns),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
            }

            if (!Equals(JsonChangeTrackingOptions, XGJsonOptions?.JsonChangeTrackingOptions ?? default))
            {
                throw new InvalidOperationException(
                    CoreStrings.SingletonOptionChanged(
                        nameof(XGJsonOptionsExtension.JsonChangeTrackingOptions),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
            }

            if (!Equals(LimitKeyedOrIndexedStringColumnLength, XGOptions.LimitKeyedOrIndexedStringColumnLength))
            {
                throw new InvalidOperationException(
                    CoreStrings.SingletonOptionChanged(
                        nameof(XGDbContextOptionsBuilder.LimitKeyedOrIndexedStringColumnLength),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
            }

            if (!Equals(StringComparisonTranslations, XGOptions.StringComparisonTranslations))
            {
                throw new InvalidOperationException(
                    CoreStrings.SingletonOptionChanged(
                        nameof(XGDbContextOptionsBuilder.EnableStringComparisonTranslations),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
            }
        }

        protected virtual XGDefaultDataTypeMappings ApplyDefaultDataTypeMappings(XGDefaultDataTypeMappings defaultDataTypeMappings, XGConnectionSettings connectionSettings)
        {
            defaultDataTypeMappings ??= DefaultDataTypeMappings;

            

            if (defaultDataTypeMappings.ClrDateTime == XGDateTimeType.Default)
            {
                defaultDataTypeMappings = defaultDataTypeMappings.WithClrDateTime(
                    ServerVersion.Supports.DateTime6
                        ? XGDateTimeType.DateTime6
                        : XGDateTimeType.DateTime);
            }

            if (defaultDataTypeMappings.ClrDateTimeOffset == XGDateTimeType.Default)
            {
                defaultDataTypeMappings = defaultDataTypeMappings.WithClrDateTimeOffset(
                    ServerVersion.Supports.DateTime6
                        ? XGDateTimeType.DateTime6
                        : XGDateTimeType.DateTime);
            }

            if (defaultDataTypeMappings.ClrTimeSpan == XGTimeSpanType.Default)
            {
                defaultDataTypeMappings = defaultDataTypeMappings.WithClrTimeSpan(
                    ServerVersion.Supports.DateTime6
                        ? XGTimeSpanType.Time6
                        : XGTimeSpanType.Time);
            }

            return defaultDataTypeMappings;
        }

        private static XGConnectionSettings GetConnectionSettings(XGOptionsExtension relationalOptions)
            => relationalOptions.Connection != null
                ? new XGConnectionSettings(relationalOptions.Connection)
                : new XGConnectionSettings(relationalOptions.ConnectionString);

        protected bool Equals(XGOptions other)
        {
            return Equals(ConnectionSettings, other.ConnectionSettings) &&
                   Equals(ServerVersion, other.ServerVersion) &&
                   Equals(DefaultCharSet, other.DefaultCharSet) &&
                   Equals(NationalCharSet, other.NationalCharSet) &&
                   Equals(DefaultGuidCollation, other.DefaultGuidCollation) &&
                   NoBackslashEscapes == other.NoBackslashEscapes &&
                   ReplaceLineBreaksWithCharFunction == other.ReplaceLineBreaksWithCharFunction &&
                   Equals(DefaultDataTypeMappings, other.DefaultDataTypeMappings) &&
                   Equals(SchemaNameTranslator, other.SchemaNameTranslator) &&
                   IndexOptimizedBooleanColumns == other.IndexOptimizedBooleanColumns &&
                   JsonChangeTrackingOptions == other.JsonChangeTrackingOptions &&
                   LimitKeyedOrIndexedStringColumnLength == other.LimitKeyedOrIndexedStringColumnLength &&
                   StringComparisonTranslations == other.StringComparisonTranslations;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((XGOptions)obj);
            
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();

            hashCode.Add(ConnectionSettings);
            hashCode.Add(ServerVersion);
            hashCode.Add(DefaultCharSet);
            hashCode.Add(NationalCharSet);
            hashCode.Add(DefaultGuidCollation);
            hashCode.Add(NoBackslashEscapes);
            hashCode.Add(ReplaceLineBreaksWithCharFunction);
            hashCode.Add(DefaultDataTypeMappings);
            hashCode.Add(SchemaNameTranslator);
            hashCode.Add(IndexOptimizedBooleanColumns);
            hashCode.Add(JsonChangeTrackingOptions);
            hashCode.Add(LimitKeyedOrIndexedStringColumnLength);
            hashCode.Add(StringComparisonTranslations);

            return hashCode.ToHashCode();
        }

        public virtual XGConnectionSettings ConnectionSettings { get; private set; }
        public virtual ServerVersion ServerVersion { get; private set; }
        public virtual CharSet DefaultCharSet { get; private set; }
        public virtual CharSet NationalCharSet { get; }
        public virtual string DefaultGuidCollation { get; private set; }
        public virtual bool NoBackslashEscapes { get; private set; }
        public virtual bool ReplaceLineBreaksWithCharFunction { get; private set; }
        public virtual XGDefaultDataTypeMappings DefaultDataTypeMappings { get; private set; }
        public virtual XGSchemaNameTranslator SchemaNameTranslator { get; private set; }
        public virtual bool IndexOptimizedBooleanColumns { get; private set; }
        public virtual XGJsonChangeTrackingOptions JsonChangeTrackingOptions { get; private set; }
        public virtual bool LimitKeyedOrIndexedStringColumnLength { get; private set; }
        public virtual bool StringComparisonTranslations { get; private set; }
    }
}
