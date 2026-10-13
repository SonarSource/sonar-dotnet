using System;
using System.Linq;
using System.Threading.Tasks;

class Example
{
    int Calculate(int a, int b) => (a + b);

    int DeepNesting(int a, int b) => (a + b);

    int Precedence(int a, int b, int c) => (a + b) * c;

    int MultipleFindings(int a, int b) => (a) + (b);

    int NestedFindings(int a, int b) => ((a) + (b));

    int Cast(double value) => ((int)value);

    int Conditional(bool condition) => (condition ? 1 : 2) + 3;

    int Coalesce(int? value) => (value ?? 1) + 2;

    int MemberAccess(int value) => (value).CompareTo(1);

    void Argument(int value) => Console.WriteLine((value));

    int Indexer(int[] values, int index) => values[(index)];

    [Obsolete((nameof(Example)))] void AttributeArgument() { }

    int Interpolation(int value) => $"{(value)}".Length;

    int Switch(int value) => (value) switch { _ => 1 };

    object Query(int[] values) => from x in (values) select x;

    Func<int, int> Lambda() => x => (x);

    Func<int, int> BlockLambda() => x => { return (x); };

    int InvokeComputed(Func<int, int> f, Func<int, int> g) => (f ?? g)(1);

    async Task<int> AwaitOperand(Task<int> task) => await (task);

    async Task<int> AwaitInside(Task<int> task) => (await task);

    async Task<string> AwaitMemberAccess(Task<int> task) => (await task).ToString();

    async Task<int> AwaitConfigureAwait(Task<int> task) => await (task).ConfigureAwait(false);

    async Task<int> AwaitThenBinary(Task<int> task) => await (task) + 1;

    async Task AwaitStatement() { await (Task.Delay(1)); }

    Func<Task<int>, Task<int>> AsyncLambda() => async x => await (x);

    int AwaitAsIdentifier(int await) => (await) + 1;

    (int, int) DeepNestingTuple() => ((1, 2));

    (int, int) NestedElement(int a, int b) => ((a + b), b);

    int SinglePair(int a, int b) => (a + b);

    int Comments(int a, int b) => /* before */ /* outer open */ (/* inner open */ a + b /* inner close */)/* outer close *//* after */;

    int DeepComments(int a) => /* first */ /* second */ (a) /* third */ /* fourth */;

    int Multiline(int a, int b) => // Fixed
        // outer open
            // middle open
                (a + b) // inner close
            // middle close
        ;

    int MultilineComments(int a, int b) => // Fixed
        /* before */ /* outer open */
            /* inner leading */ (a + b) /* inner trailing */
        /* outer leading */ /* after */;

    int Whitespace(int a, int b) => ( a  +  b );

    int Directives(int value) => (
#if true
        (value)
#else
        (value + 1)
#endif
        );

    int ParenthesesAcrossBranches(int value) =>
#if FIRST
        (value
#else
        ((value)
#endif
        );

    int DirectiveSpanningRemovedParenthesis(int value) => ((value
#if UNDEFINED
        + 1));
#else
        ));
#endif

    int InvocationBecomesCast(Func<int, int> f) => ((f))(1);

#nullable enable
    string SuppressionBecomesCast(string? value) => ((value))!.ToString();

    Point WithBecomesCast(Point point) => ((point)) with { X = 1 };

    Point CastThenWith(Point point) => (Point)((point)) with { X = 1 };

    async Task<Point> AwaitThenWith(Task<Point> task) => await ((task)) with { X = 1 };

    int MissingClosingParenthesis(int value) => (((value));

    int MissingOperand(int value) => ((value + ));
}

record Point(int X);
