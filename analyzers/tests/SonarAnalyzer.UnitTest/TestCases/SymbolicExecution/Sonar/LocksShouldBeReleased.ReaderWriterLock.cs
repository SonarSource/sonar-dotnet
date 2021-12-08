using System;
using System.Threading;

class Program
{
    private ReaderWriterLock readerWriterLock = new ReaderWriterLock();

    public void DoSomethingWithReaderWriterLock1(bool b)
    {
        readerWriterLock.AcquireReaderLock(42); // FN
        if (b)
        {
            readerWriterLock.ReleaseReaderLock();
        }
    }

    public void DoSomethingWithReaderWriterLock2(bool b)
    {
        readerWriterLock.AcquireReaderLock(new TimeSpan(42)); // FN
        if (b)
        {
            readerWriterLock.ReleaseReaderLock();
        }
    }

    public void DoSomethingWithReaderWriterLock3(bool b)
    {
        readerWriterLock.AcquireWriterLock(42); // FN
        if (b)
        {
            readerWriterLock.ReleaseWriterLock();
        }
    }

    public void DoSomethingWithReaderWriterLock4(bool b)
    {
        readerWriterLock.AcquireWriterLock(new TimeSpan(42)); // FN
        if (b)
        {
            readerWriterLock.ReleaseWriterLock();
        }
    }

    public void DoSomethingWithReaderWriterLock5(bool b)
    {
        try
        {
            readerWriterLock.AcquireReaderLock(42);
            var cookie = readerWriterLock.UpgradeToWriterLock(42);
            if (b)
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

    public void DoSomethingWithReaderWriterLock6(bool b)
    {
        try
        {
            readerWriterLock.AcquireReaderLock(new TimeSpan(42));
            var cookie = readerWriterLock.UpgradeToWriterLock(new TimeSpan(42));
            if (b)
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

    public void DoSomethingWithReaderWriterLock7(string b)
    {
        try
        {
            readerWriterLock.AcquireReaderLock(42);
            var cookie = readerWriterLock.UpgradeToWriterLock(42);
            try
            {
                Console.WriteLine(b.Length);
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

    public void DoSomethingWithReaderWriterLock8(string b)
    {
        try
        {
            readerWriterLock.AcquireReaderLock(new TimeSpan(42));
            var cookie = readerWriterLock.UpgradeToWriterLock(new TimeSpan(42));
            try
            {
                Console.WriteLine(b.Length);
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

    public void DoSomethingWithReaderWriterLock9()
    {
        readerWriterLock.AcquireReaderLock(new TimeSpan(42)); // Compliant because the cookie tracking is too complicated
        var cookie = readerWriterLock.ReleaseLock();
        readerWriterLock.RestoreLock(ref cookie);
    }
}
