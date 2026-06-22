using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;

class EntityFrameworkTestCases
{
    public void ExpressionTree() // https://sonarsource.atlassian.net/browse/NET-3927
    {
        // A lambda body inside an Expression <T> that EF will translate to SQL.
        // Swapping All for TrueForAll inside the expression tree breaks translation causing a runtime Exception
        // as TrueForAll is not a LINQ-to-SQL primitive.
        Expression<Func<List<int>, bool>> listPredicate = list => list.All(x => true); // Compliant in EntityFramework
        Expression<Func<int[], bool>> arrayPredicate = list => list.All(x => true);
        Expression<Func<ImmutableList<int>, bool>> predicate = list => list.All(x => true);
    }
}
