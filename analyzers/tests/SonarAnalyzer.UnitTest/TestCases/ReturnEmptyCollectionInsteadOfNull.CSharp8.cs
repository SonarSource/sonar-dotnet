using System;
using System.Collections.Generic;

// Reproducer for #6491 https://github.com/SonarSource/sonar-dotnet/issues/6491
namespace Issue_6491
{
    class SwitchExpressionClass
    {
        List<string> SwitchExpression(string str)
        {
            return str switch
            {
                "First" => null, // FN
                "Second" => new List<string> { },
                _ => null // FN
            };
        }

        List<string> SwitchExpression2(string str) =>
            str switch
            {
                "First" => null, // FN
                "Second" => new List<string> { },
                _ => null // FN
            };
    }
}
