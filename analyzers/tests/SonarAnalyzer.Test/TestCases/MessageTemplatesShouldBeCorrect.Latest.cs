using Microsoft.Extensions.Logging;
using System;

public class Program
{
    public void EscapeSequence(ILogger logger, string user)
    {
        logger.LogError("Login failed for Us\er");
        logger.LogError("Login failed for \e{User}", user);

        logger.LogError("{\eUser}", user);                       // Noncompliant {{Log message template placeholder '\eUser' should only contain letters, numbers, and underscore.}}
        logger.LogError("\e{User}{Us\eer}{Us\u001ber}", user);   // Noncompliant {{Log message template placeholder 'Us\eer' should only contain letters, numbers, and underscore.}}
                                                                 // Noncompliant@-1 {{Log message template placeholder 'Us\u001ber' should only contain letters, numbers, and underscore.}}
        logger.LogError("User{Use\er}User", user);               // Noncompliant
        logger.LogError("Login failed for \e{\eUser", user);     // Noncompliant {{Log message template should be syntactically correct.}}
        logger.LogError("Login failed for {{\u001b{User", user); // Noncompliant 
        logger.LogError("{\e", user);                            // Noncompliant
        logger.LogError("{{\e{", user);                          // Noncompliant
    }
}

public union UserOrId(string, int);

public class Holder { }

public static class Extensions
{
    extension(Holder holder)
    {
        public string this[int index] => "user";
    }
}

public class CSharp15
{
    public void Method(ILogger logger, UserOrId union, Holder holder)
    {
        logger.LogError("Login failed for {User}", union);
        logger.LogError("Login failed for {User}", holder[0]);
        logger.LogError("Login failed for {User}", [union]);

        logger.LogError("Login failed for {User", union);       // Noncompliant {{Log message template should be syntactically correct.}}
        logger.LogError("Login failed for {User", holder[0]);   // Noncompliant
        logger.LogError("Login failed for {User", [union]);     // Noncompliant
    }
}
