// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;

namespace EntityFrameworkCore.XuGu.Query.Internal
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
