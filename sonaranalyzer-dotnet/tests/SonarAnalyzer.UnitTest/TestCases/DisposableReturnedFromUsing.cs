using System;
using System.Collections.Generic;
using System.IO;

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
        public FileStream WriteToFile(string path, string text)
        {
            using (fs = File.Create(path)) // Noncompliant
            {
                var bytes = Encoding.UTF8.GetBytes(text);
                fs.Write(bytes, 0, bytes.Length);
                return fs;
            }
        }

        public FileStream WriteToFile2(string path, string text)
        {
            var fs = File.Create(path);
            var bytes = Encoding.UTF8.GetBytes(text);
            fs.Write(bytes, 0, bytes.Length);
            return fs;
        }

        public void WriteToFile3(string path, string text)
        {
            using (fs = File.Create(path))
            {
                var f = new Func(() =>
                {
                    return fs;
                });
                f();

                var bytes = Encoding.UTF8.GetBytes(text);
                fs.Write(bytes, 0, bytes.Length);
            }
        }
    }
}
