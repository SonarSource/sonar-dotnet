using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    // https://github.com/SonarSource/sonar-dotnet/issues/8266
    public class Repro_8266
    {
        void Method(Exception[] array)
        {
            if (array.Any())
            {
                Exception exception = array.FirstOrDefault();
                Console.WriteLine(exception.Message); // Noncompliant - FP
            }
        }
    }
}

