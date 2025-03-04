// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Microsoft.EntityFrameworkCore;
using EntityFrameworkCore.XuGu.Storage;

namespace EntityFrameworkCore.XuGu.Scaffolding.Internal
{
    internal class XGCodeGenerationServerVersionCreation
    {
        public ServerVersion ServerVersion { get; }

        public XGCodeGenerationServerVersionCreation(ServerVersion serverVersion)
        {
            ServerVersion = serverVersion;
        }
    }
}
