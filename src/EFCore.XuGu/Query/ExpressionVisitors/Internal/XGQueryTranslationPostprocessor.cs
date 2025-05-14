// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionVisitors.Internal
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
