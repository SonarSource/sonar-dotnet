using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;


//-----------------------------------------------------------------------
// <copyright file="ArgumentValidation.cs" company="SonarSource SA and Microsoft Corporation">
//   Copyright (c) SonarSource SA and Microsoft Corporation.  All rights reserved.
//   Licensed under the MIT License. See License.txt in the project root for license information.
// </copyright>
//-----------------------------------------------------------------------
namespace ConsoleApplication1
{
    /// <summary>
    /// Static methods that implement aspects of the NUnit framework that cut
    /// across individual test types, extensions, etc. Some of these use the
    /// methods of the Reflect class to implement operations specific to the
    /// NUnit Framework.
    /// </summary>
    public class Program
    {
        public static int Add(int op1, int op2)
        {
            if (op1 == 0)
            {
                return op2;
            }

            if (op2 == 0)
            {
                return op1;
            }

            return op1 + op2;
        }

        static void Main(string[] args)
        {
        }
    }

    class IFoo
    {
    }

    class IBar  // NOSONAR
    {
    }

    [SuppressMessage("Maintainability", "S2326:Unused type parameters should be removed")]
    class MoreMath<T> // Noncompliant; <T>is ignored
    {
        public int Add<T>(int a, int b) // Noncompliant; <T> is ignored
        {
            return a + b;
        }
    }
	
}
