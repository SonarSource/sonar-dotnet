using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace ClassLibrary1
{
    public class Class1
    {
        public void ExtractArchive(ZipArchive archive)
        {
            foreach (var entry in archive.Entries)
            {
                entry.ExtractToFile(""); // Noncompliant
//              ^^^^^^^^^^^^^^^^^^^^^^^
            }

            for (int i = 0; i < archive.Entries.Count; i++)
            {
                archive.Entries[i].ExtractToFile(""); // Noncompliant
            }

            archive.Entries.ToList()
                .ForEach(e => e.ExtractToFile("")); // Noncompliant
//                            ^^^^^^^^^^^^^^^^^^^
        }

        public void ExtractEntry(ZipArchiveEntry entry)
        {
            string fullName;
            Stream stream;

            entry.ExtractToFile(""); // Noncompliant
            entry.ExtractToFile("", true); // Noncompliant

            ZipFileExtensions.ExtractToFile(entry, ""); // Noncompliant
            ZipFileExtensions.ExtractToFile(entry, "", true); // Noncompliant

            stream = entry.Open(); // Noncompliant

            entry.Delete(); // Compliant, method is not tracked

            fullName = entry.FullName; // Compliant, properties are not tracked

            ExtractToFile(entry); // Compliant, method is not tracked

            Invoke(ZipFileExtensions.ExtractToFile); // Compliant, not an invocation, but could be considered as FN
        }

        public void ExtractToFile(ZipArchiveEntry entry) { }

        public void Invoke(Action<ZipArchiveEntry, string> action) { }
    }
}
