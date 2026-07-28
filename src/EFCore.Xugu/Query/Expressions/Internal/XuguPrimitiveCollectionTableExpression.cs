using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.Expressions.Internal;

/// <summary>
///     Represents an Xugu hierarchical query row source used to expand a JSON primitive collection.
///     XuguDB has no JSON_TABLE/unnest; <c>DUAL CONNECT BY LEVEL</c> supplies the ordinal rows.
/// </summary>
public sealed class XuguPrimitiveCollectionTableExpression : TableExpressionBase
{
    private static ConstructorInfo? _quotingConstructor;

    public XuguPrimitiveCollectionTableExpression(
        string alias,
        SqlExpression collectionExpression,
        Type elementType,
        RelationalTypeMapping? elementTypeMapping,
        int maxElementCount = 128)
        : base(alias)
    {
        CollectionExpression = collectionExpression;
        ElementType = elementType;
        ElementTypeMapping = elementTypeMapping;
        MaxElementCount = maxElementCount;
    }

    public SqlExpression CollectionExpression { get; }

    public Type ElementType { get; }

    public RelationalTypeMapping? ElementTypeMapping { get; }

    public int MaxElementCount { get; }

    public override string Alias
        => base.Alias!;

    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => Update((SqlExpression)visitor.Visit(CollectionExpression));

    public XuguPrimitiveCollectionTableExpression Update(SqlExpression collectionExpression)
        => collectionExpression == CollectionExpression
            ? this
            : new XuguPrimitiveCollectionTableExpression(
                Alias, collectionExpression, ElementType, ElementTypeMapping, MaxElementCount);

    public override TableExpressionBase Clone(string? alias, ExpressionVisitor cloningExpressionVisitor)
        => new XuguPrimitiveCollectionTableExpression(
            alias!,
            (SqlExpression)cloningExpressionVisitor.Visit(CollectionExpression),
            ElementType,
            ElementTypeMapping,
            MaxElementCount);

    public override XuguPrimitiveCollectionTableExpression WithAlias(string newAlias)
        => new(newAlias, CollectionExpression, ElementType, ElementTypeMapping, MaxElementCount);

    protected override TableExpressionBase WithAnnotations(IReadOnlyDictionary<string, IAnnotation> annotations)
        => new XuguPrimitiveCollectionTableExpression(
            Alias, CollectionExpression, ElementType, ElementTypeMapping, MaxElementCount);

    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(XuguPrimitiveCollectionTableExpression).GetConstructor(
                [typeof(string), typeof(SqlExpression), typeof(Type), typeof(RelationalTypeMapping), typeof(int)])!,
            Constant(Alias, typeof(string)),
            CollectionExpression.Quote(),
            Constant(ElementType),
            RelationalExpressionQuotingUtilities.QuoteTypeMapping(ElementTypeMapping),
            Constant(MaxElementCount));

    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append("XuguPrimitiveCollection(");
        expressionPrinter.Visit(CollectionExpression);
        expressionPrinter.Append(") AS ");
        expressionPrinter.Append(Alias);
    }

    public override bool Equals(object? obj)
        => ReferenceEquals(this, obj)
            || obj is XuguPrimitiveCollectionTableExpression other
            && base.Equals(other)
            && CollectionExpression.Equals(other.CollectionExpression)
            && ElementType == other.ElementType
            && Equals(ElementTypeMapping, other.ElementTypeMapping)
            && MaxElementCount == other.MaxElementCount;

    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), CollectionExpression, ElementType, ElementTypeMapping, MaxElementCount);
}
