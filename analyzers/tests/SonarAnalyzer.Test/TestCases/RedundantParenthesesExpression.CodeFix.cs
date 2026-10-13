using System;
using System.Linq;
using System.Threading.Tasks;

class Example
{
    int Calculate(int a, int b) => ((a + b));
//                                 ^ Noncompliant
//                                         ^ Secondary@-1

    int DeepNesting(int a, int b) => ((((a + b))));
//                                   ^^^ Noncompliant
//                                             ^^^ Secondary@-1

    int Precedence(int a, int b, int c) => ((a + b)) * c;
//                                         ^ Noncompliant
//                                                 ^ Secondary@-1

    int MultipleFindings(int a, int b) => ((a)) + (((b)));
//                                        ^ Noncompliant
//                                            ^ Secondary@-1
//                                                ^^ Noncompliant@-2
//                                                     ^^ Secondary@-3

    int NestedFindings(int a, int b) => (((((a)) + (((b))))));
//                                      ^^ Noncompliant
//                                                         ^^ Secondary@-1
//                                         ^ Noncompliant@-2
//                                             ^ Secondary@-3
//                                                 ^^ Noncompliant@-4
//                                                      ^^ Secondary@-5

    int Cast(double value) => (((int)value));
//                            ^ Noncompliant
//                                         ^ Secondary@-1

    int Conditional(bool condition) => ((condition ? 1 : 2)) + 3;
//                                     ^ Noncompliant
//                                                         ^ Secondary@-1

    int Coalesce(int? value) => ((value ?? 1)) + 2;
//                              ^ Noncompliant
//                                           ^ Secondary@-1

    int MemberAccess(int value) => ((value)).CompareTo(1);
//                                 ^ Noncompliant
//                                         ^ Secondary@-1

    void Argument(int value) => Console.WriteLine(((value)));
//                                                ^ Noncompliant
//                                                        ^ Secondary@-1

    int Indexer(int[] values, int index) => values[((index))];
//                                                 ^ Noncompliant
//                                                         ^ Secondary@-1

    [Obsolete(((nameof(Example))))] void AttributeArgument() { }
//            ^ Noncompliant
//                              ^ Secondary@-1

    int Interpolation(int value) => $"{((value))}".Length;
//                                     ^ Noncompliant
//                                             ^ Secondary@-1

    int Switch(int value) => ((value)) switch { _ => 1 };
//                           ^ Noncompliant
//                                   ^ Secondary@-1

    object Query(int[] values) => from x in ((values)) select x;
//                                          ^ Noncompliant
//                                                   ^ Secondary@-1

    Func<int, int> Lambda() => x => ((x));
//                                  ^ Noncompliant
//                                      ^ Secondary@-1

    Func<int, int> BlockLambda() => x => { return ((x)); };
//                                                ^ Noncompliant
//                                                    ^ Secondary@-1

    int InvokeComputed(Func<int, int> f, Func<int, int> g) => ((f ?? g))(1);
//                                                            ^ Noncompliant
//                                                                     ^ Secondary@-1

    async Task<int> AwaitOperand(Task<int> task) => await ((task));
//                                                        ^ Noncompliant
//                                                               ^ Secondary@-1

    async Task<int> AwaitInside(Task<int> task) => ((await task));
//                                                 ^ Noncompliant
//                                                              ^ Secondary@-1

    async Task<string> AwaitMemberAccess(Task<int> task) => ((await task)).ToString();
//                                                          ^ Noncompliant
//                                                                       ^ Secondary@-1

    async Task<int> AwaitConfigureAwait(Task<int> task) => await ((task)).ConfigureAwait(false);
//                                                               ^ Noncompliant
//                                                                      ^ Secondary@-1

    async Task<int> AwaitThenBinary(Task<int> task) => await ((task)) + 1;
//                                                           ^ Noncompliant
//                                                                  ^ Secondary@-1

    async Task AwaitStatement() { await ((Task.Delay(1))); }
//                                      ^ Noncompliant
//                                                      ^ Secondary@-1

    Func<Task<int>, Task<int>> AsyncLambda() => async x => await ((x));
//                                                               ^ Noncompliant
//                                                                   ^ Secondary@-1

    int AwaitAsIdentifier(int await) => ((await)) + 1;
//                                      ^ Noncompliant
//                                              ^ Secondary@-1

    (int, int) DeepNestingTuple() => (((1, 2)));
//                                   ^ Noncompliant
//                                            ^ Secondary@-1

    (int, int) NestedElement(int a, int b) => (((a + b)), b);
//                                             ^ Noncompliant
//                                                     ^ Secondary@-1

    int SinglePair(int a, int b) => (a + b);

    int Comments(int a, int b) => /* before */ (/* outer open */ (/* inner open */ a + b /* inner close */)/* outer close */)/* after */;
//                                             ^ Noncompliant
//                                                                                                                          ^ Secondary@-1

    int DeepComments(int a) => (/* first */ (/* second */ (a) /* third */) /* fourth */);
//                             ^^^^^^^^^^^^^^ Noncompliant
//                                                                       ^^^^^^^^^^^^^^^ Secondary@-1

    int Multiline(int a, int b) => // Noncompliant@+1
        ( // outer open
            ( // middle open
                (a + b) // inner close
            ) // middle close
        ); // Secondary@-1 outer close

    int MultilineComments(int a, int b) => // Noncompliant@+1
        /* before */ ( /* outer open */
            /* inner leading */ (a + b) /* inner trailing */
        /* outer leading */ ) /* after */; // Secondary

    int Whitespace(int a, int b) => (  ( a  +  b )  );
//                                  ^ Noncompliant
//                                                  ^ Secondary@-1

    int Directives(int value) => (
//                               ^ Noncompliant
#if true
        (value)
#else
        (value + 1)
#endif
        );
//      ^ Secondary

    int ParenthesesAcrossBranches(int value) =>
#if FIRST
        (value
#else
        ((value)
//      ^ Noncompliant
#endif
        );
//      ^ Secondary

    int DirectiveSpanningRemovedParenthesis(int value) => ((value
//                                                        ^ Noncompliant
#if UNDEFINED
        + 1));
#else
        ));
//       ^ Secondary
#endif

    int InvocationBecomesCast(Func<int, int> f) => ((f))(1);
//                                                 ^ Noncompliant
//                                                     ^ Secondary@-1

#nullable enable
    string SuppressionBecomesCast(string? value) => ((value))!.ToString();
//                                                  ^ Noncompliant
//                                                          ^ Secondary@-1

    Point WithBecomesCast(Point point) => ((point)) with { X = 1 };
//                                        ^ Noncompliant
//                                                ^ Secondary@-1

    Point CastThenWith(Point point) => (Point)((point)) with { X = 1 };
//                                            ^ Noncompliant
//                                                    ^ Secondary@-1

    async Task<Point> AwaitThenWith(Task<Point> task) => await ((task)) with { X = 1 };
//                                                             ^ Noncompliant
//                                                                    ^ Secondary@-1

    int MissingClosingParenthesis(int value) => (((value));
//                                              ^^ Noncompliant
//                                                       ^ Secondary@-1
//                                                        ^ Error@-2 [CS1026]

    int MissingOperand(int value) => ((value + ));
//                                   ^ Noncompliant
//                                             ^ Error@-1 [CS1525]
//                                              ^ Secondary@-2
}

record Point(int X);
