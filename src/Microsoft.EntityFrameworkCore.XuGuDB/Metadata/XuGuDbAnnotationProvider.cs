// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class XuGuDbAnnotationProvider : IRelationalAnnotationProvider
    {
        public virtual IRelationalEntityTypeAnnotations For(IEntityType entityType) => entityType.XuGuDb();
        public virtual IRelationalForeignKeyAnnotations For(IForeignKey foreignKey) => foreignKey.XuGuDb();
        public virtual IRelationalIndexAnnotations For(IIndex index) => index.XuGuDb();
        public virtual IRelationalKeyAnnotations For(IKey key) => key.XuGuDb();
        public virtual IRelationalPropertyAnnotations For(IProperty property) => property.XuGuDb();
        public virtual IRelationalModelAnnotations For(IModel model) => model.XuGuDb();
    }
}
