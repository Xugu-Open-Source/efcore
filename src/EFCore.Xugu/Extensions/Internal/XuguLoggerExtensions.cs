// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Xugu.Internal;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Xugu.Extensions.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class XuguLoggerExtensions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void DecimalTypeKeyWarning(
            this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
            IProperty property)
        {
            var definition = XuguResources.LogDecimalTypeKey(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics, property.Name, property.DeclaringEntityType.DisplayName());
            }

            if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = new PropertyEventData(
                    definition,
                    DecimalTypeKeyWarning,
                    property);

                diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
            }
        }

        private static string DecimalTypeKeyWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (PropertyEventData)payload;
            return d.GenerateMessage(
                p.Property.Name,
                p.Property.DeclaringEntityType.DisplayName());
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void DecimalTypeDefaultWarning(
            this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
            IProperty property)
        {
            var definition = XuguResources.LogDefaultDecimalTypeColumn(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics, property.Name, property.DeclaringEntityType.DisplayName());
            }

            if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = new PropertyEventData(
                    definition,
                    DecimalTypeDefaultWarning,
                    property);

                diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
            }
        }

        private static string DecimalTypeDefaultWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (PropertyEventData)payload;
            return d.GenerateMessage(
                p.Property.Name,
                p.Property.DeclaringEntityType.DisplayName());
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void ByteIdentityColumnWarning(
            this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
            IProperty property)
        {
            var definition = XuguResources.LogByteIdentityColumn(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics, property.Name, property.DeclaringEntityType.DisplayName());
            }

            if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = new PropertyEventData(
                    definition,
                    ByteIdentityColumnWarning,
                    property);

                diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
            }
        }

        private static string ByteIdentityColumnWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (PropertyEventData)payload;
            return d.GenerateMessage(
                p.Property.Name,
                p.Property.DeclaringEntityType.DisplayName());
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void ConflictingValueGenerationStrategiesWarning(
            this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
            XuguValueGenerationStrategy xuguValueGenerationStrategy,
            string otherValueGenerationStrategy,
            IReadOnlyProperty property)
        {
            var definition = XuguResources.LogConflictingValueGenerationStrategies(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(
                    diagnostics, xuguValueGenerationStrategy.ToString(), otherValueGenerationStrategy,
                    property.Name, property.DeclaringEntityType.DisplayName());
            }

            if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = new ConflictingValueGenerationStrategiesEventData(
                    definition,
                    ConflictingValueGenerationStrategiesWarning,
                    xuguValueGenerationStrategy,
                    otherValueGenerationStrategy,
                    property);

                diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
            }
        }

        private static string ConflictingValueGenerationStrategiesWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string, string, string>)definition;
            var p = (ConflictingValueGenerationStrategiesEventData)payload;
            return d.GenerateMessage(
                p.XuguValueGenerationStrategy.ToString(),
                p.OtherValueGenerationStrategy,
                p.Property.Name,
                p.Property.DeclaringEntityType.DisplayName());
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void ColumnFound(
            this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            string tableName,
            string columnName,
            int ordinal,
            string dataTypeName,
            int maxLength,
            int precision,
            int scale,
            bool nullable,
            bool identity,
            string? defaultValue,
            string? computedValue,
            bool? stored)
        {
            var definition = XuguResources.LogFoundColumn(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(
                    diagnostics,
                    l => l.LogDebug(
                        definition.EventId,
                        null,
                        definition.MessageFormat,
                        tableName,
                        columnName,
                        ordinal,
                        dataTypeName,
                        maxLength,
                        precision,
                        scale,
                        nullable,
                        identity,
                        defaultValue,
                        computedValue,
                        stored));
            }

            // No DiagnosticsSource events because these are purely design-time messages
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void ForeignKeyFound(
            this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            string foreignKeyName,
            string tableName,
            string principalTableName,
            string onDeleteAction)
        {
            var definition = XuguResources.LogFoundForeignKey(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics, foreignKeyName, tableName, principalTableName, onDeleteAction);
            }

            // No DiagnosticsSource events because these are purely design-time messages
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void DefaultSchemaFound(
            this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            string schemaName)
        {
            var definition = XuguResources.LogFoundDefaultSchema(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics, schemaName);
            }

            // No DiagnosticsSource events because these are purely design-time messages
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void TypeAliasFound(
            this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            string typeAliasName,
            string systemTypeName)
        {
            var definition = XuguResources.LogFoundTypeAlias(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics, typeAliasName, systemTypeName);
            }

            // No DiagnosticsSource events because these are purely design-time messages
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void PrimaryKeyFound(
            this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            string primaryKeyName,
            string tableName)
        {
            var definition = XuguResources.LogFoundPrimaryKey(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics, primaryKeyName, tableName);
            }

            // No DiagnosticsSource events because these are purely design-time messages
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void UniqueConstraintFound(
            this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            string uniqueConstraintName,
            string tableName)
        {
            var definition = XuguResources.LogFoundUniqueConstraint(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics, uniqueConstraintName, tableName);
            }

            // No DiagnosticsSource events because these are purely design-time messages
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void IndexFound(
            this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            string indexName,
            string tableName,
            bool unique)
        {
            var definition = XuguResources.LogFoundIndex(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics, indexName, tableName, unique);
            }

            // No DiagnosticsSource events because these are purely design-time messages
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void ForeignKeyReferencesUnknownPrincipalTableWarning(
            this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            string? foreignKeyName,
            string? tableName)
        {
            var definition = XuguResources.LogPrincipalTableInformationNotFound(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics, foreignKeyName, tableName);
            }

            // No DiagnosticsSource events because these are purely design-time messages
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void ForeignKeyReferencesMissingPrincipalTableWarning(
            this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            string? foreignKeyName,
            string? tableName,
            string? principalTableName)
        {
            var definition = XuguResources.LogPrincipalTableNotInSelectionSet(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics, foreignKeyName, tableName, principalTableName);
            }

            // No DiagnosticsSource events because these are purely design-time messages
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void ForeignKeyPrincipalColumnMissingWarning(
            this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            string foreignKeyName,
            string tableName,
            string principalColumnName,
            string principalTableName)
        {
            var definition = XuguResources.LogPrincipalColumnNotFound(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics, foreignKeyName, tableName, principalColumnName, principalTableName);
            }

            // No DiagnosticsSource events because these are purely design-time messages
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void MissingSchemaWarning(
            this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            string? schemaName)
        {
            var definition = XuguResources.LogMissingSchema(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics, schemaName);
            }

            // No DiagnosticsSource events because these are purely design-time messages
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void MissingTableWarning(
            this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            string? tableName)
        {
            var definition = XuguResources.LogMissingTable(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics, tableName);
            }

            // No DiagnosticsSource events because these are purely design-time messages
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void ColumnWithoutTypeWarning(
            this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            string tableName,
            string columnName)
        {
            var definition = XuguResources.LogColumnWithoutType(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics, tableName, columnName);
            }

            // No DiagnosticsSource events because these are purely design-time messages
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void SequenceFound(
            this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            string sequenceName,
            string sequenceTypeName,
            bool cyclic,
            int increment,
            long start,
            long min,
            long max)
        {
            // No DiagnosticsSource events because these are purely design-time messages
            var definition = XuguResources.LogFoundSequence(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(
                    diagnostics,
                    l => l.LogDebug(
                        definition.EventId,
                        null,
                        definition.MessageFormat,
                        sequenceName,
                        sequenceTypeName,
                        cyclic,
                        increment,
                        start,
                        min,
                        max));
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void TableFound(
            this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            string tableName)
        {
            var definition = XuguResources.LogFoundTable(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics, tableName);
            }

            // No DiagnosticsSource events because these are purely design-time messages
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void ReflexiveConstraintIgnored(
            this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            string foreignKeyName,
            string tableName)
        {
            var definition = XuguResources.LogReflexiveConstraintIgnored(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics, foreignKeyName, tableName);
            }

            // No DiagnosticsSource events because these are purely design-time messages
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void DuplicateForeignKeyConstraintIgnored(
            this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            string foreignKeyName,
            string tableName,
            string duplicateForeignKeyName)
        {
            var definition = XuguResources.LogDuplicateForeignKeyConstraintIgnored(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics, foreignKeyName, tableName, duplicateForeignKeyName);
            }

            // No DiagnosticsSource events because these are purely design-time messages
        }

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.MultipleCollectionIncludeWarning" /> event.
        /// </summary>
        /// <param name="diagnostics">The diagnostics logger to use.</param>
        public static void SavepointsDisabledBecauseOfMARS(
            this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics)
        {
            var definition = XuguResources.LogSavepointsDisabledBecauseOfMARS(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics);
            }

            if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = new EventData(
                    definition,
                    (d, p) => ((EventDefinition)d).GenerateMessage());

                diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
            }
        }
    }
}
