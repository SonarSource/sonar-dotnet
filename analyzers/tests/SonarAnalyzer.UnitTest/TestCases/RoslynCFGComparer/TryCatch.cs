using System;
using System.IO;

public class Sample
{
    private void A() { }
    private void B() { }
    private void C() { }
    private void D() { }
    private void E() { }
    private void F() { }
    private void Handle(Exception ex) { }

    public void TryCatch()
    {
        try
        {
            A();
            B();
            C();
        }
        catch (Exception ex)
        {
            Handle(ex);
        }
    }

    public void TryCatchCommonFinally()
    {
        try
        {
            A();
            B();
            C();
        }
        catch (Exception ex)
        {
            Handle(ex);
        }
        finally
        {
            F();
        }
    }

    public void TryCatchSpecificFinally()
    {
        try
        {
            A();
            B();
            C();
        }
        catch (ArgumentException ex)
        {
            Handle(ex);
        }
        finally
        {
            F();
        }
    }

    public void TryCatchWhenFinally()
    {
        try
        {
            A();
            B();
            C();
        }
        catch (Exception ex) when (ex.Message.Contains("memory"))
        {
            Handle(ex);
        }
        finally
        {
            F();
        }
    }

    public void TryFinally()
    {
        try
        {
            A();
            B();
            C();
        }
        finally
        {
            F();
        }
    }

    public void TryReturnFinally()
    {
        try
        {
            A();
            B();
            return;
            C();
        }
        finally
        {
            F();
        }
    }

    public void TryMultiCatchFinally()
    {
        try
        {
            A();
            B();
            C();
        }
        catch (FormatException ex)
        {
            Handle(ex);
        }
        catch (ArgumentNullException ex)
        {
            Handle(ex);
        }
        catch (Exception ex) when (ex.Message.Contains("memory"))
        {
            Handle(ex);
        }
        catch (Exception ex)
        {
            Handle(ex);
        }
        finally
        {
            F();
        }
    }

    public void TryCatchRethrow()
    {
        try
        {
            A();
            B();
            C();
        }
        catch (Exception ex)
        {
            Handle(ex);
            throw;
        }
    }

    public void TryCatchRethrowNested()
    {
        try
        {
            A();
            try
            {
                B();
                C();
                D();
            }
            catch (NullReferenceException nre)
            {
                throw;
            }
            E();
        }
        catch (Exception ex)
        {
            Handle(ex);
        }
        F();
    }

    public void TryThrowCatch()
    {
        try
        {
            A();
            B();
            throw new Exception("Message");
            C();
        }
        catch (Exception ex)
        {
            Handle(ex);
        }
    }

    public void DoTryBreakCatchContinueFinally()
    {
        do
        {
            A();
            try
            {
                B();
                break;
                C();
            }
            catch (Exception ex)
            {
                Handle(ex);
                continue;
            }
            finally
            {
                F();
            }
            D();
        } while (true);
    }

    public string TryReturnCatchReturn()
    {
        try
        {
            A();
            B();
            C();
            return "OK";
        }
        catch (Exception ex)
        {
            return "Error: " + ex.Message;
        }
    }

    public void Using()
    {
        using (var ms = new MemoryStream())
        {
            ms.Write(null, 0, 0);
        }
    }

    public void UsingThrow()
    {
        using (var ms = new MemoryStream())
        {
            ms.Write(null, 0, 0);
            throw new Exception("Message");
            ms.Write(null, 1, 1);
        }
    }

    public void UsingVar()
    {
        using var ms = new MemoryStream();
        ms.Write(null, 0, 0);
    }

    public void UsingVarThrow()
    {
        using var ms = new MemoryStream();
        ms.Write(null, 0, 0);
        throw new Exception("Message");
        ms.Write(null, 1, 1);
    }

    public void TryFinallyTryFinally()
    {
        try
        {
            A();
        }
        finally
        {
            F();
        }
        B();
        try
        {
            C();
        }
        finally
        {
            F();
        }
    }
}
