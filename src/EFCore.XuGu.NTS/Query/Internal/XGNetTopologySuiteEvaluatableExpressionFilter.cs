using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EntityFrameworkCore.XuGu.Query.Internal
{
    public class XGNetTopologySuiteEvaluatableExpressionFilter : IXGEvaluatableExpressionFilter
    {
        public bool? IsEvaluatableExpression(Expression expression, IModel model)
        {
            if (expression is MethodCallExpression methodCallExpression &&
                methodCallExpression.Method.DeclaringType == typeof(XGSpatialDbFunctionsExtensions))
            {
                return false;
            }

            return null;
        }
    }
}
