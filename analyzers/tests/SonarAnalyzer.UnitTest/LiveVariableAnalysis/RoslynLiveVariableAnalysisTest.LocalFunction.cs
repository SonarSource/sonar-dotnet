/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonarAnalyzer.UnitTest.LiveVariableAnalysis
{
    public partial class RoslynLiveVariableAnalysisTest
    {
        [TestMethod]
        public void StaticLocalFunction_Expression_LiveIn()
        {
            var code = @"
outParameter = LocalFunction(intParameter);
static int LocalFunction(int a) => a + 1;";
            var context = new Context(code, "LocalFunction");
            context.ValidateEntry(new LiveIn("a"), new LiveOut("a"));
            context.Validate("a + 1", new LiveIn("a"));
            context.ValidateExit();
        }

        [TestMethod]
        public void StaticLocalFunction_Expression_NotLiveIn_NotLiveOut()
        {
            var code = @"
outParameter = LocalFunction(0);
static int LocalFunction(int a) => 42;";
            var context = new Context(code, "LocalFunction");
            context.ValidateEntry();
            context.Validate("42");
            context.ValidateExit();
        }

        [TestMethod]
        public void StaticLocalFunction_LiveIn()
        {
            var code = @"
outParameter = LocalFunction(intParameter);
static int LocalFunction(int a)
{
    return a + 1;
}";
            var context = new Context(code, "LocalFunction");
            context.ValidateEntry(new LiveIn("a"), new LiveOut("a"));
            context.Validate("a + 1", new LiveIn("a"));
            context.ValidateExit();
        }

        [TestMethod]
        public void StaticLocalFunction_NotLiveIn_NotLivOut()
        {
            var code = @"
outParameter = LocalFunction(0);
static int LocalFunction(int a)
{
    return 42;
}";
            var context = new Context(code, "LocalFunction");
            context.ValidateEntry();
            context.Validate("42");
            context.ValidateExit();
        }

        [TestMethod]
        public void StaticLocalFunction_Recursive()
        {
            var code = @"
outParameter = LocalFunction(intParameter);
static int LocalFunction(int a)
{
    if(a <= 0)
        return 0;
    else
        return LocalFunction(a - 1);
};";
            var context = new Context(code, "LocalFunction");
            context.ValidateEntry(new LiveIn("a"), new LiveOut("a"));
            context.Validate("0");
            context.Validate("LocalFunction(a - 1)", new LiveIn("a"));
            context.ValidateExit();
        }

        [TestMethod]
        public void LocalFunctionInvocation_LiveIn()
        {
            var code = @"
var variable = 42;
if (boolParameter)
    return;
LocalFunction();

int LocalFunction() => variable;";
            var context = new Context(code);
            context.ValidateEntry(new LiveIn("boolParameter"), new LiveOut("boolParameter"));
            context.Validate("boolParameter", new LiveIn("boolParameter"), new LiveOut("variable"));
            context.Validate("LocalFunction();", new LiveIn("variable"));
        }

        [TestMethod]
        public void LocalFunctionInvocation_FunctionDeclaredBeforeCode_LiveIn()
        {
            var code = @"
int LocalFunction() => variable;

var variable = 42;
if (boolParameter)
    return;
LocalFunction();";
            var context = new Context(code);
            context.ValidateEntry(new LiveIn("boolParameter"), new LiveOut("boolParameter"));
            context.Validate("boolParameter", new LiveIn("boolParameter"), new LiveOut("variable"));
            context.Validate("LocalFunction();", new LiveIn("variable"));
        }

        [TestMethod]
        public void LocalFunctionInvocation_Generic_LiveIn()
        {
            var code = @"
var variable = 42;
if (boolParameter)
    return;
LocalFunction<int>();

int LocalFunction<T>() => variable;";
            var context = new Context(code);
            context.ValidateEntry(new LiveIn("boolParameter"), new LiveOut("boolParameter"));
            context.Validate("boolParameter", new LiveIn("boolParameter"), new LiveOut("variable"));
            context.Validate("LocalFunction<int>();", new LiveIn("variable"));
        }

        [TestMethod]
        public void LocalFunctionInvocation_NestedGeneric_LiveIn()
        {
            var code = @"
var variable = 42;
if (boolParameter)
    return;
LocalFunction<int>();

int LocalFunction<T>()
{
    return Nested<string>();

    int Nested<TT>() => variable;
}";
            var context = new Context(code);
            context.ValidateEntry(new LiveIn("boolParameter"), new LiveOut("boolParameter"));
            context.Validate("boolParameter", new LiveIn("boolParameter"), new LiveOut("variable"));
            context.Validate("LocalFunction<int>();", new LiveIn("variable"));
        }

        [TestMethod]
        public void LocalFunctionInvocation_NotLiveIn()
        {
            var code = @"
var variable = 42;
if (boolParameter)
    return;
LocalFunction();
Method(variable);

void LocalFunction()
{
    variable = 0;
}";
            var context = new Context(code);
            context.ValidateEntry(new LiveIn("boolParameter"), new LiveOut("boolParameter"));
            context.Validate("boolParameter", new LiveIn("boolParameter"));
            context.Validate("LocalFunction();");
        }

        [TestMethod]
        public void LocalFunctionInvocation_NestedArgument_NotLiveIn()
        {
            var code = @"
LocalFunction(40);

int LocalFunction(int cnt) => cnt + 2;";
            var context = new Context(code);
            context.ValidateEntry();
            context.Validate("LocalFunction(40);");
        }

        [TestMethod]
        public void LocalFunctionInvocation_NestedVariable_NotLiveIn_NotCaptured()
        {
            var code = @"
LocalFunction();

int LocalFunction()
{
    var nested = 42;
    Func<int> f = () => nested;
}";
            var context = new Context(code);
            context.ValidateEntry();
            context.Validate("LocalFunction();");
        }

        [TestMethod]
        public void LocalFunctionInvocation_Recursive_LiveIn()
        {
            var code = @"
var variable = 42;
if (boolParameter)
    return;
LocalFunction(10);

int LocalFunction(int cnt) => variable + (cnt == 0 ? 0 : LocalFunction(cnt - 1));";
            var context = new Context(code);
            context.ValidateEntry(new LiveIn("boolParameter"), new LiveOut("boolParameter"));
            context.Validate("boolParameter", new LiveIn("boolParameter"), new LiveOut("variable"));
            context.Validate("LocalFunction(10);", new LiveIn("variable"));
        }

        [TestMethod]
        public void LocalFunctionInvocation_Nested_LiveIn()
        {
            var code = @"
var variable = 42;
if (boolParameter)
    return;
LocalFunction();

int LocalFunction()
{
    return Nested();

    int Nested() => variable;
}";
            var context = new Context(code);
            context.ValidateEntry(new LiveIn("boolParameter"), new LiveOut("boolParameter"));
            context.Validate("boolParameter", new LiveIn("boolParameter"), new LiveOut("variable"));
            context.Validate("LocalFunction();", new LiveIn("variable"));
        }

        [TestMethod]
        public void LocalFunctionInvocation_TryCatchFinally_LiveIn()
        {
            var code = @"
var usedInTry = 42;
var usedInCatch = 42;
var usedInFinally = 42;
var usedInUnreachable = 42;
if (boolParameter)
    return;
LocalFunction();

int LocalFunction()
{
    try
    {
        Method(usedInTry);
    }
    catch
    {
        Method(usedInCatch);
    }
    finally
    {
        Method(usedInFinally);
    }
    return;
    Method(usedInUnreachable);
}";

            var context = new Context(code);
            context.ValidateEntry(new LiveIn("boolParameter"), new LiveOut("boolParameter"));
            // usedInUnreachable is here only because of simplified processing inside local functions
            context.Validate("boolParameter", new LiveIn("boolParameter"), new LiveOut("usedInTry", "usedInCatch", "usedInFinally", "usedInUnreachable"));
            context.Validate("LocalFunction();", new LiveIn("usedInTry", "usedInCatch", "usedInFinally", "usedInUnreachable"));
        }
    }
}
