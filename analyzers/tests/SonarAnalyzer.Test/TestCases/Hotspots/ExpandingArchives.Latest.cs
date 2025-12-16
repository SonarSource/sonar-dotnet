using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace ClassLibrary1
{
    public class Class1
    {
        public void ExtractArchive(ZipArchive archive)
        {
            foreach (var entry in archive.Entries)
            {
                entry.ExtractToFileAsync("");                           // Compliant FN https://sonarsource.atlassian.net/browse/NET-2879
            }
            for (int i = 0; i < archive.Entries.Count; i++)
            {
                archive.Entries[i].ExtractToFileAsync("");              // Compliant FN 
            }
            archive.Entries.ToList()
                .ForEach(e => e.ExtractToFileAsync(""));                // Compliant FN
            ZipFileExtensions.ExtractToDirectoryAsync(archive, "");     // Compliant FN
            archive.ExtractToDirectoryAsync("");                        // Compliant FN
        }

        public void ExtractEntry(ZipArchiveEntry entry)
        {
            string fullName;
            entry.ExtractToFileAsync("");                               // Compliant FN
            entry?.ExtractToFileAsync("");                              // Compliant FN
            entry.ExtractToFileAsync("", true);                         // Compliant FN
            ZipFileExtensions.ExtractToFileAsync(entry, "");            // Compliant FN
            ZipFileExtensions.ExtractToFileAsync(entry, "", true);      // Compliant FN
            ZipFile.ExtractToDirectoryAsync("", "");                    // Compliant FN
            ZipFile.ExtractToDirectoryAsync("", "", Encoding.Default);  // Compliant FN
            var stream = entry.OpenAsync();                             // Compliant, method is not tracked
            entry.Delete();                                             // Compliant, method is not tracked
            fullName = entry.FullName;                                  // Compliant, properties are not tracked
            ExtractToFileAsync(entry);                                  // Compliant, method is not tracked
        }
        public void ExtractToFileAsync(ZipArchiveEntry entry) { }
    }
}
