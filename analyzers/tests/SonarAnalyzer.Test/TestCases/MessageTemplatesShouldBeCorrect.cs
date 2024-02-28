using Microsoft.Extensions.Logging;
using System;

public class Program
{
    public void Compliant(ILogger logger, string user)
    {
        Console.WriteLine("Login failed for {User", user);

        logger.LogError("");
        logger.LogError("Login failed for User");
        logger.LogError("{User}", user);
        logger.LogError("{User}{User}{User}", user);
        logger.LogError("User{User}User", user);

        logger.LogError($"Login failed for {{User", user);
        logger.LogError("Login failed for {User}", user);
        logger.LogError("Login failed for {{User}}", user);
        logger.LogError("Login failed for {{{User}}}", user);

        logger.LogError("{{ }} {{ {{{{  {{🔥}}");     // On fire

        logger.LogError("Login failed for {$User,42}", user);
        logger.LogError("Login failed for {@User:format}", user);
        logger.LogError("Login failed for {User42,42:format}", user);
        logger.LogError("Login failed for {User_Name:format,42}", user);

        logger.LogError("Login failed for {User:                 }", user);
        logger.LogError("Login failed for {User: -f0rmat c@n contain _any!@#$thing_ {you{want{ }", user);
        logger.LogError("Login failed for {User,-42}", user);

        logger.LogError("Login failed for {{User}", user);          // Compliant FN. Syntactically valid, in reality it's probably a typo.
    }

    public void InvalidSyntax(ILogger logger, string user)
    {
        logger.LogError("Login failed for {User", user);                // Noncompliant {{Log message template should be syntactically correct.}}
        //              ^^^^^^^^^^^^^^^^^^^^^^^^
        logger.LogError("Login failed for {{{User", user);              // Noncompliant {{Log message template should be syntactically correct.}}
        //              ^^^^^^^^^^^^^^^^^^^^^^^^^^
        logger.LogError("{", user);                                     // Noncompliant {{Log message template should be syntactically correct.}}
        //              ^^^
        logger.LogError("{{{", user);                                   // Noncompliant {{Log message template should be syntactically correct.}}
        //              ^^^^^
    }

    public void EmptyPlaceholder(ILogger logger, string user)
    {
        logger.LogError("Login failed for {}", user);                   // Noncompliant {{Log message template should not contain empty placeholder.}}
        //                                ^^
        logger.LogError("{}", user);                                    // Noncompliant {{Log message template should not contain empty placeholder.}}
        //               ^^
        logger.LogError("{{{}}}", user);                                // Noncompliant {{Log message template should not contain empty placeholder.}}
        //                 ^^
        logger.LogError("{{Login for {} user }}failed", user);          // Noncompliant {{Log message template should not contain empty placeholder.}}
        //                           ^^
    }

    public void InvalidName(ILogger logger, string user)
    {
        logger.LogError("Login failed for {@}", user);                  // Noncompliant {{Log message template placeholder '' should only contain letters, numbers, and underscore.}}
        //                                 ^
        logger.LogError("Login failed for {%User}", user);              // Noncompliant {{Log message template placeholder '%User' should only contain letters, numbers, and underscore.}}
        //                                 ^^^^^
        logger.LogError("Login failed for { {User} }", user);              // Noncompliant {{Log message template placeholder ' {User' should only contain letters, numbers, and underscore.}}
        //                                 ^^^^^^
        logger.LogError("Login failed for {User-Name}", user);          // Noncompliant {{Log message template placeholder 'User-Name' should only contain letters, numbers, and underscore.}}
        //                                 ^^^^^^^^^
        logger.LogError("Login failed for {User Name}", user);          // Noncompliant {{Log message template placeholder 'User Name' should only contain letters, numbers, and underscore.}}
        //                                 ^^^^^^^^^
        logger.LogError("Login failed for {user}, server is on {🔥}", user, "fire");          // Noncompliant {{Log message template placeholder '🔥' should only contain letters, numbers, and underscore.}}
        //                                                      ^^
        logger.LogError("Retry attempt {@,:}", user);                    // Noncompliant {{Log message template placeholder '' should only contain letters, numbers, and underscore.}}
        //                              ^^^
    }

    public void InvalidAlignmentAndFormat(ILogger logger, int cnt)
    {
        logger.LogError("Retry attempt {Cnt,r}", cnt);                   // Noncompliant {{Log message template placeholder 'Cnt' should have numeric alignment instead of 'r'.}}
        //                              ^^^^^
        logger.LogError("Retry attempt {Cnt,}", cnt);                    // Noncompliant {{Log message template placeholder 'Cnt' should have numeric alignment instead of ''.}}
        //                              ^^^^
        logger.LogError("Retry attempt {Cnt,-foo}", cnt);                // Noncompliant {{Log message template placeholder 'Cnt' should have numeric alignment instead of '-foo'.}}
        //                              ^^^^^^^^
        logger.LogError("Retry attempt {Cnt:}", cnt);                    // Noncompliant {{Log message template placeholder 'Cnt' should not have empty format.}}
        //                              ^^^^
        logger.LogError("Retry attempt {Cnt,:}", cnt);                    // Noncompliant {{Log message template placeholder 'Cnt' should have numeric alignment instead of ''.}}
        //                              ^^^^^
    }

    public void Complex(ILogger logger, string user, int cnt, int total)
    {
        logger.LogError("[User {user name,42}] Retry attempt {Cnt,:}", user, cnt);
        //                      ^^^^^^^^^^^^ {{Log message template placeholder 'user name' should only contain letters, numbers, and underscore.}}
        //                                                    ^^^^^ @-1 {{Log message template placeholder 'Cnt' should have numeric alignment instead of ''.}}


        logger.LogError("[User {user%,42}] Retry {{attempt}} {@Cnt:foo,} {$Total,-5000:}", user, cnt, total);
        //                      ^^^^^^^^ {{Log message template placeholder 'user%' should only contain letters, numbers, and underscore.}}
        //                                                                ^^^^^^^^^^^^^ @-1 {{Log message template placeholder 'Total' should not have empty format.}}

        logger.LogError(@"[User {user,42}] Retry attempt {@Cnt
            :foo,}", user, cnt); // Noncompliant@-1

        logger.LogError(@"
            [User {user name}]
            Retry attempt {@Cnt:}
            Out of total {$Total,-abc:format}",
            user, cnt, total);
        //         ^^^^^^^^^ @-3 {{Log message template placeholder 'user name' should only contain letters, numbers, and underscore.}}
        //                 ^^^^^ @-3 {{Log message template placeholder 'Cnt' should not have empty format.}}
        //                ^^^^^^^^^^^^^^^^^^ @-3 {{Log message template placeholder 'Total' should have numeric alignment instead of '-abc'.}}
    }

}
