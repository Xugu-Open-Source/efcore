// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using EntityFrameworkCore.XuGu.Storage.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    public static class XGCommonJsonChangeTrackingOptionsExtensions
    {
        public static XGJsonChangeTrackingOptions ToJsonChangeTrackingOptions(this XGCommonJsonChangeTrackingOptions options)
            => options switch
            {
                XGCommonJsonChangeTrackingOptions.RootPropertyOnly => XGJsonChangeTrackingOptions.CompareRootPropertyOnly,
                XGCommonJsonChangeTrackingOptions.FullHierarchyOptimizedFast => XGJsonChangeTrackingOptions.CompareStringRootPropertyByEquals |
                                                                                   XGJsonChangeTrackingOptions.CompareDomRootPropertyByEquals |
                                                                                   XGJsonChangeTrackingOptions.SnapshotCallsDeepClone |
                                                                                   XGJsonChangeTrackingOptions.SnapshotCallsClone,
                XGCommonJsonChangeTrackingOptions.FullHierarchyOptimizedSemantically => XGJsonChangeTrackingOptions.CompareStringRootPropertyByEquals |
                                                                                           XGJsonChangeTrackingOptions.CompareDomSemantically |
                                                                                           XGJsonChangeTrackingOptions.HashDomSemantiallyOptimized |
                                                                                           XGJsonChangeTrackingOptions.SnapshotCallsDeepClone |
                                                                                           XGJsonChangeTrackingOptions.SnapshotCallsClone,
                XGCommonJsonChangeTrackingOptions.FullHierarchySemantically => XGJsonChangeTrackingOptions.None,
                _ => throw new ArgumentOutOfRangeException(nameof(options)),
            };
    }
}
