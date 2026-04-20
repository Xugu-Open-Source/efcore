// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class XGAnnotationProvider : IRelationalAnnotationProvider
    {
        public virtual IRelationalEntityTypeAnnotations For(IEntityType entityType) => entityType.XG();
        public virtual IRelationalForeignKeyAnnotations For(IForeignKey foreignKey) => foreignKey.XG();
        public virtual IRelationalIndexAnnotations For(IIndex index) => index.XG();
        public virtual IRelationalKeyAnnotations For(IKey key) => key.XG();
        public virtual IRelationalModelAnnotations For(IModel model) => model.XG();
        public virtual IRelationalPropertyAnnotations For(IProperty property) => property.XG();
    }
}
