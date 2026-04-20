// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Migrations.Internal
{
    public class XGMigrationsAnnotationProvider : MigrationsAnnotationProvider
    {
        public override IEnumerable<IAnnotation> For(IProperty property)
        {
            // The migrations SQL generator gets the property's DefaultValue and DefaultValueSql.
            // However, there's no way there to detect properties that have ValueGenerated.OnAdd
            // *without* defining a default value; these should translate to SERIAL columns.
            // So we add a custom annotation here to pass the information.
            if (property.ValueGenerated == ValueGenerated.OnAdd)
                yield return new Annotation(XGAnnotationNames.Prefix + XGAnnotationNames.ValueGeneratedOnAdd, true);
            else if (property.ValueGenerated == ValueGenerated.OnAddOrUpdate)
                yield return new Annotation(XGAnnotationNames.Prefix + XGAnnotationNames.ValueGeneratedOnAddOrUpdate, true);
        }

        public override IEnumerable<IAnnotation> For(IIndex index)
        {
            if (index.XG().Method != null)
            {
                yield return new Annotation(
                     XGAnnotationNames.Prefix + XGAnnotationNames.IndexMethod,
                     index.XG().Method);
            }
        }
    }
}