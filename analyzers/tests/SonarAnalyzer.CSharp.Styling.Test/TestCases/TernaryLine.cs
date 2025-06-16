
public class Sample
{
    private bool condition;
    private object trueBranch, falseBranch;

    public void Method()
    {
        _ = condition
            && condition ? trueBranch : falseBranch;
        //               ^^^^^^^^^^^^                   Noncompliant    {{Place branches of the multiline ternary on a separate line.}}
        //                            ^^^^^^^^^^^^^     Noncompliant@-1 {{Place branches of the multiline ternary on a separate line.}}

        _ = condition
            ? trueBranch : falseBranch;     // Noncompliant
        //               ^^^^^^^^^^^^^

        _ = condition ? trueBranch          // Noncompliant
        //            ^^^^^^^^^^^^
            : falseBranch;

        _ = condition ? trueBranch          // Noncompliant
                .ToString()
            : falseBranch;

        _ = condition ? trueBranch : falseBranch;
        _ = condition
                ? trueBranch
                : falseBranch;
        _ = condition
            && condition
                ? trueBranch
                : falseBranch;
    }
}
