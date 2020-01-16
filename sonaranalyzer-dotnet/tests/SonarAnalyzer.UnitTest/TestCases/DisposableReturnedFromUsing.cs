using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tests.Diagnostics
{
    public class DisposableReturnedFromUsing
    {
        public FileStream WriteToFile(string path, string text)
        {
            using (var fs = File.Create(path)) // Noncompliant {{Remove the 'using' statement; it will cause automatic disposal of 'fs'.}}
//          ^^^^^
            {
                var bytes = Encoding.UTF8.GetBytes(text);
                fs.Write(bytes, 0, bytes.Length);
                return fs;
            }
        }

        private FileStream fs;
        public FileStream WriteToFile2(string path, string text)
        {
            using (fs = File.Create(path)) // Noncompliant
            {
                var bytes = Encoding.UTF8.GetBytes(text);
                fs.Write(bytes, 0, bytes.Length);
                return fs;
            }
        }

        public FileStream WriteToFile3(string path, string text)
        {
            var fs = File.Create(path);
            var bytes = Encoding.UTF8.GetBytes(text);
            fs.Write(bytes, 0, bytes.Length);
            return fs;
        }

        public void WriteToFile4(string path, string text)
        {
            using (fs = File.Create(path))
            {
                var f = new Func<FileStream>(() =>
                {
                    return fs;
                });
                f();

                var bytes = Encoding.UTF8.GetBytes(text);
                fs.Write(bytes, 0, bytes.Length);
            }
        }

        public FileStream Method(string path)
        {
            using var fs1 = File.Create(path); // Noncompliant {{Remove the 'using' statement; it will cause automatic disposal of 'fs1'.}}
//          ^^^^^

            using var fs2 = File.Create(path);

            return fs1;
        }

        public FileStream MethodSingleNoncompliantVariables(string path)
        {
            // Noncompliant@+1 {{Remove the 'using' statement; it will cause automatic disposal of 'fs1'.}}
            using FileStream fs1 = File.Create(path), fs2 = File.Create(path);

            if (path != null)
                return fs1;
            return null;
        }

        public FileStream MethodMultipleNoncompliantVariables(string path)
        {
            // Noncompliant@+1 {{Remove the 'using' statement; it will cause automatic disposal of 'fs1', 'fs2'.}}
            using FileStream fs1 = File.Create(path), fs2 = File.Create(path);

            if (path != null)
                return fs1;
            return fs2;
        }

        public ref struct Struct
        {
            public void Dispose()
            {
            }
        }

        public Struct Foo()
        {
            using (var disposableRefStruct = new Struct()) // Noncompliant {{Remove the 'using' statement; it will cause automatic disposal of 'disposableRefStruct'.}}
            {
                return disposableRefStruct;
            }
        }

        public Struct Bar()
        {
            using var disposableRefStruct = new Struct(); // Noncompliant

            return disposableRefStruct;
        }

        public Struct FooBar()
        {
            using var notReturnedDisposableRefStruct = new Struct();
            var notUsingRefStruct = new Struct();
            return notUsingRefStruct;
        }

        public Struct BarFoo()
        {
            using var foo = new Struct(); // FN - we do not track alias variables
            var bar = foo;
            return bar;
        }
    }
}
