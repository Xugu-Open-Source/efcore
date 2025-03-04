// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using EntityFrameworkCore.XuGu.Infrastructure.Internal;
using EntityFrameworkCore.XuGu.Query.Internal;

namespace EntityFrameworkCore.XuGu.Query.ExpressionVisitors.Internal
{
    public class XGQueryTranslationPostprocessor : RelationalQueryTranslationPostprocessor
    {
        private readonly IXGOptions _options;
        private readonly XGSqlExpressionFactory _sqlExpressionFactory;

        public XGQueryTranslationPostprocessor(
            QueryTranslationPostprocessorDependencies dependencies,
            RelationalQueryTranslationPostprocessorDependencies relationalDependencies,
            QueryCompilationContext queryCompilationContext,
            IXGOptions options,
            XGSqlExpressionFactory sqlExpressionFactory)
            : base(dependencies, relationalDependencies, queryCompilationContext)
        {
            _options = options;
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public override Expression Process(Expression query)
        {
            query = base.Process(query);

            query = new XGJsonParameterExpressionVisitor(_sqlExpressionFactory, _options).Visit(query);

            if (_options.ServerVersion.Supports.XGBug96947Workaround)
            {
                query = new XGBug96947WorkaroundExpressionVisitor(_sqlExpressionFactory).Visit(query);
            }

            return query;
        }
    }
}
