using System;
using System.Threading;

class Program
{
    private ReaderWriterLockSlim readerWriterLockSlim = new ReaderWriterLockSlim();

    public void Method1(bool condition)
    {
        readerWriterLockSlim.EnterReadLock(); // FN
        if (condition)
        {
            readerWriterLockSlim.ExitReadLock();
        }
    }

    public void Method2(bool condition)
    {
        readerWriterLockSlim.EnterWriteLock(); // FN
        if (condition)
        {
            readerWriterLockSlim.ExitWriteLock();
        }
    }

    public void Method3(bool condition)
    {
        readerWriterLockSlim.EnterUpgradeableReadLock(); // FN
        if (condition)
        {
            readerWriterLockSlim.ExitUpgradeableReadLock();
        }
    }

    public void Method4()
    {
        if (readerWriterLockSlim.TryEnterReadLock(42)) // FN
        {
        }
        else
        {
            readerWriterLockSlim.ExitReadLock();
        }
    }

    public void Method5()
    {
        if (readerWriterLockSlim.TryEnterReadLock(new TimeSpan(42))) // FN
        {
        }
        else
        {
            readerWriterLockSlim.ExitReadLock();
        }
    }

    public void Method6()
    {
        if (readerWriterLockSlim.TryEnterWriteLock(42)) // FN
        {
        }
        else
        {
            readerWriterLockSlim.ExitWriteLock();
        }
    }

    public void Method7()
    {
        if (readerWriterLockSlim.TryEnterWriteLock(new TimeSpan(42))) // FN
        {
        }
        else
        {
            readerWriterLockSlim.ExitWriteLock();
        }
    }

    public void Method8()
    {
        if (readerWriterLockSlim.TryEnterUpgradeableReadLock(42)) // FN
        {
        }
        else
        {
            readerWriterLockSlim.ExitReadLock();
        }
    }

    public void Method9()
    {
        if (readerWriterLockSlim.TryEnterUpgradeableReadLock(new TimeSpan(42))) // FN
        {
        }
        else
        {
            readerWriterLockSlim.ExitReadLock();
        }
    }


    public void Method10(bool condition)
    {
        try
        {
            readerWriterLockSlim.EnterUpgradeableReadLock();
            readerWriterLockSlim.EnterWriteLock();
            if (condition)
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

    public void Method11(string arg)
    {
        try
        {
            readerWriterLockSlim.EnterUpgradeableReadLock();
            readerWriterLockSlim.EnterWriteLock();
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
