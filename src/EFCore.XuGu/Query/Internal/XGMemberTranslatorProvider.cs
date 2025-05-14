// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.Internal
{
    public class XGMemberTranslatorProvider : RelationalMemberTranslatorProvider
    {
        public XGMemberTranslatorProvider([NotNull] RelationalMemberTranslatorProviderDependencies dependencies)
            : base(dependencies)
        {
            var sqlExpressionFactory = (XGSqlExpressionFactory)dependencies.SqlExpressionFactory;

            AddTranslators(
                new IMemberTranslator[] {
                    new XGDateTimeMemberTranslator(sqlExpressionFactory),
                    new XGStringMemberTranslator(sqlExpressionFactory),
                    new XGTimeSpanMemberTranslator(sqlExpressionFactory),
                });
        }
    }
}
