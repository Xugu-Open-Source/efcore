// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

namespace EntityFrameworkCore.XuGu.Infrastructure
{
    public enum ServerType
    {
        /// <summary>
        /// Custom server implementation
        /// </summary>
        Custom = -1,

        /// <summary>
        /// XuGu server
        /// </summary>
        XuGu,

        /// <summary>
        /// MariaDB server
        /// </summary>
        MariaDb
    }
}
