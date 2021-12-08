using System;
using System.Threading;

class Program
{
    private ReaderWriterLock readerWriterLock = new ReaderWriterLock();

    public void Method1(bool condition)
    {
        readerWriterLock.AcquireReaderLock(42); // FN
        if (condition)
        {
            readerWriterLock.ReleaseReaderLock();
        }
    }

    public void Method2(bool condition)
    {
        readerWriterLock.AcquireReaderLock(new TimeSpan(42)); // FN
        if (condition)
        {
            readerWriterLock.ReleaseReaderLock();
        }
    }

    public void Method3(bool condition)
    {
        readerWriterLock.AcquireWriterLock(42); // FN
        if (condition)
        {
            readerWriterLock.ReleaseWriterLock();
        }
    }

    public void Method4(bool condition)
    {
        readerWriterLock.AcquireWriterLock(new TimeSpan(42)); // FN
        if (condition)
        {
            readerWriterLock.ReleaseWriterLock();
        }
    }

    public void Method5(bool condition)
    {
        try
        {
            readerWriterLock.AcquireReaderLock(42);
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

    public void Method6(bool condition)
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

    public void Method9(bool condition)
    {
        readerWriterLock.AcquireReaderLock(new TimeSpan(42)); // Compliant because the cookie tracking is too complicated
        LockCookie cookie = new LockCookie();
        if (condition)
        {
            cookie = readerWriterLock.ReleaseLock();
        }

        readerWriterLock.RestoreLock(ref cookie);
    }
}
