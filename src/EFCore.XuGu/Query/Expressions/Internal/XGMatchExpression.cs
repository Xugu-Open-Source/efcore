// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using EntityFrameworkCore.XuGu.Query.ExpressionVisitors.Internal;

namespace EntityFrameworkCore.XuGu.Query.Expressions.Internal
{
    public class XGMatchExpression : SqlExpression
    {
        public XGMatchExpression(
            SqlExpression match,
            SqlExpression against,
            XGMatchSearchMode searchMode,
            RelationalTypeMapping typeMapping)
            : base(typeof(bool), typeMapping)
        {
            Check.NotNull(match, nameof(match));
            Check.NotNull(against, nameof(against));

            Match = match;
            Against = against;
            SearchMode = searchMode;
        }

        public virtual XGMatchSearchMode SearchMode { get; }

        public virtual SqlExpression Match { get; }
        public virtual SqlExpression Against { get; }

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return visitor is XGQuerySqlGenerator XGQuerySqlGenerator // TODO: Move to VisitExtensions
                ? XGQuerySqlGenerator.VisitXGMatch(this)
                : base.Accept(visitor);
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var match = (SqlExpression)visitor.Visit(Match);
            var against = (SqlExpression)visitor.Visit(Against);

            return Update(match, against);
        }

        public virtual XGMatchExpression Update(SqlExpression match, SqlExpression against)
            => match != Match || against != Against
                ? new XGMatchExpression(
                    match,
                    against,
                    SearchMode,
                    TypeMapping)
                : this;

        public override bool Equals(object obj)
            => obj != null && ReferenceEquals(this, obj)
            || obj is XGMatchExpression matchExpression && Equals(matchExpression);

        private bool Equals(XGMatchExpression matchExpression)
            => base.Equals(matchExpression)
            && SearchMode == matchExpression.SearchMode
            && Match.Equals(matchExpression.Match)
            && Against.Equals(matchExpression.Against);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), SearchMode, Match, Against);

        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Append("CONTAINS ");
            expressionPrinter.Append($"({expressionPrinter.Visit(Match)}");
            expressionPrinter.Append(" , ");
            expressionPrinter.Append($"{expressionPrinter.Visit(Against)})");
        }
    }
}
