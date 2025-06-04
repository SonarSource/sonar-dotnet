using System;
using System.Collections.Generic;
using System.Linq;

public class RedundantJumpStatement
{
    void Foo1()
    {
        var a = new Action(() =>
        {
            return; // Noncompliant
//          ^^^^^^^
        });

        goto A; // Noncompliant
        A:
        return; // Noncompliant {{Remove this redundant jump.}}
    }

    int Prop
    {
        set
        {
            goto A; // Noncompliant
            A:
            return; // Noncompliant
        }
    }

    void Foo2(bool a)
    {
        if (a)
        {
            return; // Noncompliant
        }
        else
        {
            return;
            Foo2(a);
        }
    }

    void Loop_Continue(bool a)
    {
        for (int i = 0; i < 10; i++)
        {
            if (a)
            {
                continue; // Noncompliant
            }
            else
            {
                continue;
                Foo2(a);
            }
        }
    }

    void Switch_Goto(int j)
    {
        switch (j)
        {
            case 1:
                goto default; // Not reported
            default:
                break;
        }

        throw new Exception();
    }

    void Switch_Return(int j)
    {
        switch (j)
        {
            case 1:
                return; // Compliant
            case 2:
                return; // Non-compliant, not reported
                break;
        }
    }

    IEnumerable<int> YieldBreak1(int j)
    {
        yield break; // Compliant
    }

    IEnumerable<int> YieldReturn(int j)
    {
        yield return 1;
    }

    IEnumerable<int> YieldBreak2(int j)
    {
        yield return 1;
        yield break; // Noncompliant
    }

    void LongChain()
    {
        if (true)
        {
        }
        else if (true)
        {
            return; // Noncompliant
        }
        else if (false)
        { }
        else if (false)
        { }
        else
        { }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/1265
    void RegressionTest_1265()
    {
        try
        {
            Console.WriteLine();
        }
        catch (Exception)
        {
            Console.WriteLine();
            return;
        }
        finally
        {
            Console.WriteLine();
        }
        Console.WriteLine();
    }

    void RedundantJumpInTryCatch1()
    {
        try
        {
            Console.WriteLine();
        }
        catch (Exception)
        {
            Console.WriteLine();
            return; // Noncompliant
        }
        finally
        {
            Console.WriteLine();
        }
    }

    void RedundantJumpInTryCatch2()
    {
        try
        {
            Console.WriteLine();
            return; // Noncompliant
        }
        catch (Exception)
        {
            Console.WriteLine();
        }
        finally
        {
            Console.WriteLine();
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/3105
public class Repro_3105
{
    public void DoSomething()
    {
        Action a = () => throw new Exception();

        Invoke(() => throw new Exception());
    }

    void Invoke(Action a)
    {
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/3193
public class Repro_3193
{
    void Foo(Log Logger)
    {
        {
            var retryAttempts = 0;
            while (retryAttempts < 3)
            {
                retryAttempts++;
                try
                {
                    // Do something real here.
                    return; // Noncompliant FP, this should end the loop if previous statement succeeded
                }
                catch (Exception ex)
                {
                    //Log
                }
                finally
                {
                    Logger?.Finally(); // Removing ? generates simple block and different CFG shape (Finally is on different location)
                }
            }
        }
    }
}

public class Log
{
    public void Finally() { }
}

// https://sonarsource.atlassian.net/browse/NET-1149
public class Repro_1149
{
    public void LocalFunctionAfterReturn(List<string> items)
    {
        items.ForEach(LocalFunction);

        return; // Compliant: exception for readability

        void LocalFunction(string item)
        {
            Console.WriteLine(item);
        }
    }

    public void LocalFunctionBeforeReturn(List<string> items)
    {
        void LocalFunction(string item)
        {
            Console.WriteLine(item);
        }

        items.ForEach(LocalFunction);

        return; // Noncompliant
    }

    public void LocalFunctionAfterGoto(List<string> items)
    {
        items.ForEach(LocalFunction);

        goto A; // Noncompliant
        A:

        void LocalFunction(string item)
        {
            Console.WriteLine(item);
        }
    }

    public void LabeledLocalFunctionAfterReturn(List<string> items)
    {
        items.ForEach(LocalFunction);

        return; // Compliant: exception for readability
        A:

        void LocalFunction(string item)
        {
            Console.WriteLine(item);
        }
    }

    public IEnumerable<int> LocalFunctionAfterYieldReturnYieldBreak(List<string> items)
    {
        items.ForEach(LocalFunction);

        yield return 1;

        Console.WriteLine("Some code");

        yield break; // Compliant: exception for readability

        void LocalFunction(string item)
        {
            Console.WriteLine(item);
        }
    }
}

