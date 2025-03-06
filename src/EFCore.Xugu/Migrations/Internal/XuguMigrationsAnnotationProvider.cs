// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Xugu.Migrations.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public class XuguMigrationsAnnotationProvider : MigrationsAnnotationProvider
    {
        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
#pragma warning disable EF1001 // Internal EF Core API usage.
        public XuguMigrationsAnnotationProvider(MigrationsAnnotationProviderDependencies dependencies)
#pragma warning restore EF1001 // Internal EF Core API usage.
            : base(dependencies)
        {
        }

        /// <inheritdoc />
        public override IEnumerable<IAnnotation> ForRemove(IRelationalModel model)
            => model.GetAnnotations().Where(a => a.Name != XuguAnnotationNames.EditionOptions);

        /// <inheritdoc />
        public override IEnumerable<IAnnotation> ForRemove(ITable table)
            => table.GetAnnotations();

        /// <inheritdoc />
        public override IEnumerable<IAnnotation> ForRemove(IUniqueConstraint constraint)
        {
            if (constraint.Table[XuguAnnotationNames.IsTemporal] as bool? == true)
            {
                yield return new Annotation(XuguAnnotationNames.IsTemporal, true);

                yield return new Annotation(
                    XuguAnnotationNames.TemporalPeriodStartColumnName,
                    constraint.Table[XuguAnnotationNames.TemporalPeriodStartColumnName]);

                yield return new Annotation(
                    XuguAnnotationNames.TemporalPeriodEndColumnName,
                    constraint.Table[XuguAnnotationNames.TemporalPeriodEndColumnName]);

                yield return new Annotation(
                    XuguAnnotationNames.TemporalHistoryTableName,
                    constraint.Table[XuguAnnotationNames.TemporalHistoryTableName]);

                yield return new Annotation(
                    XuguAnnotationNames.TemporalHistoryTableSchema,
                    constraint.Table[XuguAnnotationNames.TemporalHistoryTableSchema]);
            }
        }

        /// <inheritdoc />
        public override IEnumerable<IAnnotation> ForRemove(IColumn column)
        {
            if (column.Table[XuguAnnotationNames.IsTemporal] as bool? == true)
            {
                yield return new Annotation(XuguAnnotationNames.IsTemporal, true);

                yield return new Annotation(
                    XuguAnnotationNames.TemporalHistoryTableName,
                    column.Table[XuguAnnotationNames.TemporalHistoryTableName]);

                yield return new Annotation(
                    XuguAnnotationNames.TemporalHistoryTableSchema,
                    column.Table[XuguAnnotationNames.TemporalHistoryTableSchema]);

                if (column[XuguAnnotationNames.TemporalPeriodStartColumnName] is string periodStartColumnName)
                {
                    yield return new Annotation(
                        XuguAnnotationNames.TemporalPeriodStartColumnName,
                        periodStartColumnName);
                }

                if (column[XuguAnnotationNames.TemporalPeriodEndColumnName] is string periodEndColumnName)
                {
                    yield return new Annotation(
                        XuguAnnotationNames.TemporalPeriodEndColumnName,
                        periodEndColumnName);
                }
            }
        }

        /// <inheritdoc />
        public override IEnumerable<IAnnotation> ForRename(ITable table)
        {
            if (table[XuguAnnotationNames.IsTemporal] as bool? == true)
            {
                yield return new Annotation(XuguAnnotationNames.IsTemporal, true);

                yield return new Annotation(
                    XuguAnnotationNames.TemporalHistoryTableName,
                    table[XuguAnnotationNames.TemporalHistoryTableName]);

                yield return new Annotation(
                    XuguAnnotationNames.TemporalHistoryTableSchema,
                    table[XuguAnnotationNames.TemporalHistoryTableSchema]);
            }
        }
    }
}
