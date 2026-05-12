using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Win32;

namespace Tests.Diagnostics
{
    public class DisposableNotDisposed
    {
        private FileStream field_fs1; // Compliant - not instantiated
        public FileStream field_fs2 = new FileStream(@"c:\foo.txt", FileMode.Open); // Compliant - public
        private FileStream field_fs3 = new FileStream(@"c:\foo.txt", FileMode.Open); // Noncompliant {{Dispose 'field_fs3' when it is no longer needed.}}
//                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        private FileStream field_fs4 = new FileStream(@"c:\foo.txt", FileMode.Open); // Compliant - disposed
        private FileStream field_fs5;
        private FileStream field_fs6;
        private object field_fs7;
        private FileStream field_fs8 = new FileStream(@"c:\foo.txt", FileMode.Open); // Compliant - passed to method using this
        FileStream field_fs9 = new FileStream(@"c:\foo.txt", FileMode.Open); // Noncompliant - effectively private
        private FileStream field_fs10 = new FileStream(@"c:\foo.txt", FileMode.Open); // Compliant - aliased in constructor initializer

        private TcpListener listener = new TcpListener(9000); // Compliant, not IDisposable

        private class InnerClass
        {
            private FileStream inner_field_fs1 = new FileStream(@"c:\foo.txt", FileMode.Open); // Noncompliant - should be reported on once
        }

        private FileStream Return()
        {
            var fs = new FileStream(@"c:\foo.txt", FileMode.Open); // Compliant - returned
            return fs;
        }

        public DisposableNotDisposed() : this(field_fs10) // Error [CS0120]
        {
        }

        public DisposableNotDisposed(FileStream fs)
        {
            var fs1 = new FileStream(@"c:\foo.txt", FileMode.Open); // Noncompliant - directly instantiated with new
            var fs2 = File.Open(@"c:\foo.txt", FileMode.Open); // Noncompliant - instantiated with factory method
            Stream fs3 = new FileStream(@"c:\foo.txt", FileMode.Open); // Noncompliant - declaration type should not matter
            var s = new WebClient(); // Noncompliant - another tracked type

            var fs4 = new FileStream(@"c:\foo.txt", FileMode.Open); // Compliant - passed to a method
            NoOperation(fs4);

            FileStream fs5; // Compliant - used properly
            using (fs5 = new FileStream(@"c:\foo.txt", FileMode.Open))
            {
                // do nothing but dispose
            }

            using (var fs6 = new FileStream(@"c:\foo.txt", FileMode.Open)) // Compliant - used properly
            {
                // do nothing but dispose
            }

            FileStream fs6_1 = new FileStream(@"c:\foo.txt", FileMode.Open);
            using (fs6_1)
            {
                // do nothing but dispose
            }

            var fs7 = new FileStream(@"c:\foo.txt", FileMode.Open); // Compliant - Dispose()
            fs7.Dispose();

            var fs8 = new FileStream(@"c:\foo.txt", FileMode.Open); // Compliant - Close()
            fs8.Close();

            var fs9 = new FileStream(@"c:\foo.txt", FileMode.Open); // Compliant - disposed using elvis operator
            fs9?.Dispose();

            FileStream fs10 = fs; // Compliant - not instantiated directly

            var fs11 = new FileStream(@"c:\foo.txt", FileMode.Open); // Compliant - aliased
            var newFs = fs11;

            var fs12 = new BufferedStream(fs); // Compliant - constructed from another stream
            var fs13 = new StreamReader(fs); // Compliant - constructed from another stream

            var fs14 = new FileStream(@"c:\foo.txt", FileMode.Open); // Compliant - passed to another method (or constructor)
            var fs15 = new BufferedStream(fs14); // Compliant - not tracked

            var fs16 = new FileStream(@"c:\foo.txt", FileMode.Open); // Compliant - aliased
            var myAnonymousType = new { SomeField = fs16 };

            FileStream fs17; // Compliant - no initializer, should not fail

            FileStream
                fs18 = new FileStream(@"c:\foo.txt", FileMode.Open), // Noncompliant - test issue location
                fs19; // Compliant - not instantiated

            FileStream fs20 = new FileStream(@"c:\foo.txt", FileMode.Open); // Compliant - aliased
            Stream fs21;
            fs21 = fs20;

            FileStream fs22;
            fs22 = new FileStream(@"c:\foo.txt", FileMode.Open); // Noncompliant

            field_fs4.Dispose();

            field_fs5 = new FileStream(@"c:\foo.txt", FileMode.Open); // Noncompliant

            NoOperation(field_fs6);
            field_fs6 = new FileStream(@"c:\foo.txt", FileMode.Open); // FN - field_fs6 is re-assigned a new FileStream (and not disposed) after passing it to a method

            field_fs7 = new FileStream(@"c:\foo.txt", FileMode.Open); // Noncompliant - even if field_fs7's type is object

            NoOperation(this.field_fs8);

            var tokenSource1 = new CancellationTokenSource(); // Noncompliant
            var tokenSource2 = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken.None, CancellationToken.None); // Noncompliant
            var tokenSource3 = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken.None, CancellationToken.None); // Compliant, disposed
            tokenSource3.Dispose();
        }

        private void Conditions(bool cond, string x)
        {
            var fs1 = new FileStream(@"c:\foo.txt", FileMode.Open); // FN - not disposed on all paths
            if (cond)
            {
                fs1.Dispose();
            }
        }

        private void NoOperation(FileStream fs)
        {
            // do nothing
        }
    }

    public class Empty
    {
    }

    public sealed class ImplementsDisposable : IDisposable
    {
        private readonly FileStream stream;

        public ImplementsDisposable()
        {
            stream = new FileStream(@"c:\foo.txt", FileMode.Open);              // Compliant
        }

        public void Dispose()
        {
            stream.Dispose();
        }
    }

    public class DisposableTest
    {
        private ImplementsDisposable stream = new ImplementsDisposable();     // Compliant - the rule only tracks specific IDisposable / IAsyncDisposable types
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/8365
    class Repro_8365
    {
        void NotDisposed(string filePath)
        {
            FileStream fileStream = File.OpenRead(filePath); // FN
        }

        void Disposed(string filePath)
        {
            FileStream fileStream = File.OpenRead(filePath); // Compliant
            fileStream.Dispose();
        }
    }

    public class Repro_8336_1 : IDisposable
    {
        private readonly CancellationTokenSource source;

        public Repro_8336_1() =>
            source = new CancellationTokenSource(); // Compliant

        public void Dispose() =>
             source.Dispose();
    }

    public class Repro_8336_2 : IDisposable
    {
        private readonly CancellationTokenSource source;

        public Repro_8336_2() =>
            source = new CancellationTokenSource(); // Noncompliant

        public void Dispose()
        {
            // Did we forget to dispose something here?
        }
    }

    // NET-3282: https://sonarsource.atlassian.net/browse/NET-3282
    public class Repro_ExplicitIDisposableCast : IDisposable
    {
        private readonly CancellationTokenSource source;

        public Repro_ExplicitIDisposableCast() =>
            source = new CancellationTokenSource(); // Noncompliant - FP: disposed via explicit cast to IDisposable

        public void Dispose()
        {
            ((IDisposable)source).Dispose();
        }
    }

    // NET-1265: https://sonarsource.atlassian.net/browse/NET-1265
    public class RegistryKeyTests
    {
        public void OpenSubKeyNotDisposed()
        {
            var key = Registry.CurrentUser.OpenSubKey("Software"); // Noncompliant {{Dispose 'key' when it is no longer needed.}}
            var key2 = Registry.LocalMachine.CreateSubKey("Software\\MyApp"); // Noncompliant
            var key3 = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32); // Noncompliant
            var key4 = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, "remoteMachine"); // Noncompliant
        }

        public void OpenSubKeyDisposed()
        {
            var key = Registry.CurrentUser.OpenSubKey("Software"); // Compliant - Dispose is called
            key?.Dispose();

            var key2 = Registry.CurrentUser.OpenSubKey("Software"); // Compliant - Close is equivalent to Dispose
            key2?.Close();

            using (var key3 = Registry.CurrentUser.OpenSubKey("Software")) // Compliant - using statement
            {
            }
        }

        public void DeleteSubKeyNoncompliant()
        {
            var key = Registry.CurrentUser.OpenSubKey("Software", true); // Noncompliant - DeleteSubKey deletes a child key, not the receiver
            key.DeleteSubKey("MyApp");

            var key2 = Registry.LocalMachine.OpenSubKey("Software", true); // Noncompliant - DeleteSubKeyTree deletes a child key, not the receiver
            key2.DeleteSubKeyTree("MyApp");
        }
    }
}
