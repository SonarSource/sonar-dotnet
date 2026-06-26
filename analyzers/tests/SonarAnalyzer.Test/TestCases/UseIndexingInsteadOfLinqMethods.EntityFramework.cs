using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

class MyEntity { }

class EntityFrameworkTestCases
{
    public void ExpressionTree()
    {
        // A lambda body inside an Expression<T> that EF will translate to SQL.
        // Swapping First/Last/ElementAt for indexers inside the expression tree breaks translation causing a runtime Exception
        // as indexers are not LINQ-to-SQL primitives.

        // List<T> (implements IList<T> and IReadOnlyList<T>)
        Expression<Func<List<int>, int>> firstPredicate = list => list.First(); // Compliant in EntityFramework
        Expression<Func<List<int>, int>> lastPredicate = list => list.Last(); // Compliant in EntityFramework
        Expression<Func<List<int>, int>> elementAtPredicate = list => list.ElementAt(1); // Compliant in EntityFramework

        // Reproducer: receiver is List<MyEntity> - EF Core expression visitor does not recognise
        // ElementAccessExpression on an arbitrary list parameter as a queryable primitive.
        Expression<Func<List<MyEntity>, MyEntity>> entityFirst = list => list.First(); // Compliant in EntityFramework
        Expression<Func<List<MyEntity>, MyEntity>> entityLast = list => list.Last(); // Compliant in EntityFramework
        Expression<Func<List<MyEntity>, MyEntity>> entityElementAt = list => list.ElementAt(1); // Compliant in EntityFramework

        // IList<T>
        Expression<Func<IList<int>, int>> ilistFirst = list => list.First(); // Compliant in EntityFramework
        Expression<Func<IList<int>, int>> ilistLast = list => list.Last(); // Compliant in EntityFramework
        Expression<Func<IList<int>, int>> ilistElementAt = list => list.ElementAt(1); // Compliant in EntityFramework

        // IReadOnlyList<T>
        Expression<Func<IReadOnlyList<int>, int>> readonlyFirst = list => list.First(); // Compliant in EntityFramework
        Expression<Func<IReadOnlyList<int>, int>> readonlyLast = list => list.Last(); // Compliant in EntityFramework
        Expression<Func<IReadOnlyList<int>, int>> readonlyElementAt = list => list.ElementAt(1); // Compliant in EntityFramework
    }
}
