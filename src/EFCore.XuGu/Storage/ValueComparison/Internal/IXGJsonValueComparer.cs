// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Microsoft.EntityFrameworkCore.ChangeTracking;
using EntityFrameworkCore.XuGu.Storage.Internal;

namespace EntityFrameworkCore.XuGu.Storage.ValueComparison.Internal
{
    public interface IXGJsonValueComparer
    {
        ValueComparer Clone(XGJsonChangeTrackingOptions options);
    }
}
