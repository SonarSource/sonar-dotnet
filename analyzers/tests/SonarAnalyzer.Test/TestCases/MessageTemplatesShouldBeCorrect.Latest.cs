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
