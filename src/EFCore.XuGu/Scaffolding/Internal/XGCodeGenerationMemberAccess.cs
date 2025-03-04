// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.Reflection;

namespace EntityFrameworkCore.XuGu.Scaffolding.Internal
{
    internal class XGCodeGenerationMemberAccess
    {
        public MemberInfo MemberInfo { get; }

        public XGCodeGenerationMemberAccess(MemberInfo memberInfo)
        {
            MemberInfo = memberInfo;
        }
    }
}
