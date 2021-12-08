using System;
using System.Threading;

class Program
{
    private ReaderWriterLockSlim readerWriterLockSlim = new ReaderWriterLockSlim();

    public void DoSomethingWithReaderWriterSlimLock1(bool b)
    {
        readerWriterLockSlim.EnterReadLock(); // FN
        if (b)
        {
            readerWriterLockSlim.ExitReadLock();
        }
    }

    public void DoSomethingWithReaderWriterSlimLock2(bool b)
    {
        readerWriterLockSlim.EnterWriteLock(); // FN
        if (b)
        {
            readerWriterLockSlim.ExitWriteLock();
        }
    }

    public void DoSomethingWithReaderWriterSlimLock3(bool b)
    {
        readerWriterLockSlim.EnterUpgradeableReadLock(); // FN
        if (b)
        {
            readerWriterLockSlim.ExitUpgradeableReadLock();
        }
    }

    public void DoSomethingWithReaderWriterSlimLock4()
    {
        if (readerWriterLockSlim.TryEnterReadLock(42)) // FN
        {
        }
        else
        {
            readerWriterLockSlim.ExitReadLock();
        }
    }

    public void DoSomethingWithReaderWriterSlimLock5()
    {
        if (readerWriterLockSlim.TryEnterReadLock(new TimeSpan(42))) // FN
        {
        }
        else
        {
            readerWriterLockSlim.ExitReadLock();
        }
    }

    public void DoSomethingWithReaderWriterSlimLock6()
    {
        if (readerWriterLockSlim.TryEnterWriteLock(42)) // FN
        {
        }
        else
        {
            readerWriterLockSlim.ExitWriteLock();
        }
    }

    public void DoSomethingWithReaderWriterSlimLock7()
    {
        if (readerWriterLockSlim.TryEnterWriteLock(new TimeSpan(42))) // FN
        {
        }
        else
        {
            readerWriterLockSlim.ExitWriteLock();
        }
    }

    public void DoSomethingWithReaderWriterSlimLock8()
    {
        if (readerWriterLockSlim.TryEnterUpgradeableReadLock(42)) // FN
        {
        }
        else
        {
            readerWriterLockSlim.ExitReadLock();
        }
    }

    public void DoSomethingWithReaderWriterSlimLock9()
    {
        if (readerWriterLockSlim.TryEnterUpgradeableReadLock(new TimeSpan(42))) // FN
        {
        }
        else
        {
            readerWriterLockSlim.ExitReadLock();
        }
    }


    public void DoSomethingWithReaderWriterSlimLock10(bool b)
    {
        try
        {
            readerWriterLockSlim.EnterUpgradeableReadLock();
            readerWriterLockSlim.EnterWriteLock();
            if (b)
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }
        catch (Exception)
        {

            throw;
        }
        finally
        {
            readerWriterLockSlim.ExitUpgradeableReadLock();
        }
    }

    public void DoSomethingWithReaderWriterSlimLock11(string b)
    {
        try
        {
            readerWriterLockSlim.EnterUpgradeableReadLock();
            readerWriterLockSlim.EnterWriteLock();
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
                readerWriterLockSlim.ExitWriteLock();
            }
        }
        catch (Exception)
        {

            throw;
        }
        finally
        {
            readerWriterLockSlim.ExitUpgradeableReadLock();
        }
    }
}
