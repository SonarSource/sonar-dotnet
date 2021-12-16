using System;
using System.Threading;

class Program
{
    private bool condition;
    private ReaderWriterLock readerWriterLock = new ReaderWriterLock();

    public void Method1()
    {
        readerWriterLock.AcquireReaderLock(42); // FN
        if (condition)
        {
            readerWriterLock.ReleaseReaderLock();
        }
    }

    public void Method2()
    {
        readerWriterLock.AcquireReaderLock(new TimeSpan(42)); // FN
        if (condition)
        {
            readerWriterLock.ReleaseReaderLock();
        }
    }

    public void Method3()
    {
        readerWriterLock.AcquireWriterLock(42); // FN
        if (condition)
        {
            readerWriterLock.ReleaseWriterLock();
        }
    }

    public void Method4()
    {
        readerWriterLock.AcquireWriterLock(new TimeSpan(42)); // FN
        if (condition)
        {
            readerWriterLock.ReleaseWriterLock();
        }
    }

    public void Method5()
    {
        readerWriterLock.AcquireReaderLock(42);
        try
        {
            var cookie = readerWriterLock.UpgradeToWriterLock(42);
            if (condition)
            {
                readerWriterLock.DowngradeFromWriterLock(ref cookie);
            }
        }
        catch (Exception)
        {

            throw;
        }
        finally
        {
            readerWriterLock.ReleaseReaderLock();
        }
    }

    public void Method6()
    {
        try
        {
            readerWriterLock.AcquireReaderLock(new TimeSpan(42));
            var cookie = readerWriterLock.UpgradeToWriterLock(new TimeSpan(42));
            if (condition)
            {
                readerWriterLock.DowngradeFromWriterLock(ref cookie);
            }
        }
        catch (Exception)
        {

            throw;
        }
        finally
        {
            readerWriterLock.ReleaseReaderLock();
        }
    }

    public void Method7(string arg)
    {
        try
        {
            readerWriterLock.AcquireReaderLock(42);
            var cookie = readerWriterLock.UpgradeToWriterLock(42);
            try
            {
                Console.WriteLine(arg.Length);
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                readerWriterLock.DowngradeFromWriterLock(ref cookie);
            }
        }
        catch (Exception)
        {

            throw;
        }
        finally
        {
            readerWriterLock.ReleaseReaderLock();
        }
    }

    public void Method8(string arg)
    {
        try
        {
            readerWriterLock.AcquireReaderLock(new TimeSpan(42));
            var cookie = readerWriterLock.UpgradeToWriterLock(new TimeSpan(42));
            try
            {
                Console.WriteLine(arg.Length);
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                readerWriterLock.DowngradeFromWriterLock(ref cookie);
            }
        }
        catch (Exception)
        {

            throw;
        }
        finally
        {
            readerWriterLock.ReleaseReaderLock();
        }
    }

    public void Method9()
    {
        readerWriterLock.AcquireReaderLock(new TimeSpan(42)); // Compliant because the cookie tracking is too complicated
        LockCookie cookie = new LockCookie();
        if (condition)
        {
            cookie = readerWriterLock.ReleaseLock();
        }

        readerWriterLock.RestoreLock(ref cookie);
    }

    public void Method10()
    {
        readerWriterLock.AcquireReaderLock(42); // Compliant
        readerWriterLock.ReleaseReaderLock();
    }
}
