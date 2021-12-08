using System;
using System.Threading;

class Program
{
    private ReaderWriterLockSlim readerWriterLockSlim = new ReaderWriterLockSlim();

    public void Method1(bool b)
    {
        readerWriterLockSlim.EnterReadLock(); // FN
        if (b)
        {
            readerWriterLockSlim.ExitReadLock();
        }
    }

    public void Method2(bool b)
    {
        readerWriterLockSlim.EnterWriteLock(); // FN
        if (b)
        {
            readerWriterLockSlim.ExitWriteLock();
        }
    }

    public void Method3(bool b)
    {
        readerWriterLockSlim.EnterUpgradeableReadLock(); // FN
        if (b)
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


    public void Method10(bool b)
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

    public void Method11(string b)
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
