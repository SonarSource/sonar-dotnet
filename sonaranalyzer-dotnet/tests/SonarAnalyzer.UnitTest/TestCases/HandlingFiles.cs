using System;
using System.Text;
using System.IO;
using Microsoft.Win32.SafeHandles;
using System.Security.AccessControl;
using System.IO.Compression;
using System.IO.IsolatedStorage;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Security.Policy;

namespace Tests.Diagnostics
{
    class Program
    {
        void Main(IntPtr intPtr, IsolatedStorageFile isolatedStorageFile, MemoryMappedFile memoryMappedFile, MemoryMappedFileSecurity memoryMappedFileSecurity, Stream stream)
        {
            // Any static method call on File or Directory will raise
            File.Exists(""); // Noncompliant {{Make sure this file handling is safe here.}}
//          ^^^^^^^^^^^^^^^
            File.Create(""); // Noncompliant
            File.Delete(""); // Noncompliant
            File.ReadLines(""); // Noncompliant
            Directory.Exists(""); // Noncompliant
            Directory.Delete(""); // Noncompliant
            Directory.EnumerateFiles(""); // Noncompliant

            // Any FileInfo or DirectoryInfo creation
            var fileInfo = new FileInfo(""); // Noncompliant {{Make sure this file handling is safe here.}}
//                         ^^^^^^^^^^^^^^^^
            var dirInfo = new DirectoryInfo(""); // Noncompliant

            // Calls to extern CreateFile
            SafeFileHandle handle;
            handle = CreateFile("", 0, 0, intPtr, 0, 0, intPtr); // Noncompliant
            handle = CreateFile(); // Compliant, not extern

            // Creation of SafeFileHandle
            handle = new SafeFileHandle(IntPtr.Zero, false); // Noncompliant

            // All constructors of FileStream
            FileStream fileStream;
            fileStream = new FileStream(IntPtr.Zero, FileAccess.Read); // Noncompliant
            fileStream = new FileStream(handle, FileAccess.Read); // Noncompliant
            fileStream = new FileStream("", FileMode.Append); // Noncompliant
            fileStream = new FileStream(IntPtr.Zero, FileAccess.Read, true); // Noncompliant
            fileStream = new FileStream(handle, FileAccess.Read, 0); // Noncompliant
            fileStream = new FileStream("", FileMode.Append, FileAccess.Read); // Noncompliant
            fileStream = new FileStream(IntPtr.Zero, FileAccess.Read, true, 0); // Noncompliant
            fileStream = new FileStream(handle, FileAccess.Read, 0, true); // Noncompliant
            fileStream = new FileStream("", FileMode.Append, FileAccess.Read, FileShare.Read); // Noncompliant
            fileStream = new FileStream(IntPtr.Zero, FileAccess.Read, true, 0, true); // Noncompliant
            fileStream = new FileStream("", FileMode.Append, FileAccess.Read, FileShare.Read, 0); // Noncompliant
            fileStream = new FileStream("", FileMode.Append, FileAccess.Read, FileShare.Read, 0, true); // Noncompliant
            fileStream = new FileStream("", FileMode.Append, FileAccess.Read, FileShare.Read, 0, FileOptions.Asynchronous); // Noncompliant
            fileStream = new FileStream("", FileMode.Append, FileSystemRights.Read, FileShare.Read, 0, FileOptions.Asynchronous); // Noncompliant
            fileStream = new FileStream("", FileMode.Append, FileSystemRights.Read, FileShare.Read, 0, FileOptions.Asynchronous, new FileSecurity()); // Noncompliant

            // All constructors of StreamWriter, whos first argument is string
            StreamWriter streamWriter;
            streamWriter = new StreamWriter(stream);
            streamWriter = new StreamWriter(""); // Noncompliant
            streamWriter = new StreamWriter(stream, Encoding.Unicode);
            streamWriter = new StreamWriter("", true); // Noncompliant
            streamWriter = new StreamWriter(stream, Encoding.Unicode, 0);
            streamWriter = new StreamWriter("", true, Encoding.Unicode); // Noncompliant
            streamWriter = new StreamWriter(stream, Encoding.Unicode, 0, true);
            streamWriter = new StreamWriter("", true, Encoding.Unicode, 0); // Noncompliant

            // All constructors of StreamReader, whos first argument is string
            StreamReader streamReader;
            streamReader = new StreamReader(stream);
            streamReader = new StreamReader(""); // Noncompliant
            streamReader = new StreamReader(stream, true);
            streamReader = new StreamReader(stream, Encoding.Unicode);
            streamReader = new StreamReader("", true); // Noncompliant
            streamReader = new StreamReader("", Encoding.Unicode); // Noncompliant
            streamReader = new StreamReader(stream, Encoding.Unicode, true);
            streamReader = new StreamReader("", Encoding.Unicode, true); // Noncompliant
            streamReader = new StreamReader(stream, Encoding.Unicode, true, 0);
            streamReader = new StreamReader("", Encoding.Unicode, true, 0); // Noncompliant
            streamReader = new StreamReader(stream, Encoding.Unicode, true, 0, true);

            Path.GetTempFileName(); // Noncompliant
            Path.GetTempPath(); // Noncompliant

            FileSecurity fileSecurity;
            fileSecurity = new FileSecurity();
            fileSecurity = new FileSecurity("", AccessControlSections.All); // Noncompliant

            // All static methods of ZipFile (all methods are static!)
            ZipFile.CreateFromDirectory("", ""); // Noncompliant
            ZipFile.CreateFromDirectory("", "", CompressionLevel.Fastest, true); // Noncompliant
            ZipFile.CreateFromDirectory("", "", CompressionLevel.Fastest, true, Encoding.Unicode); // Noncompliant
            ZipFile.Open("", ZipArchiveMode.Read); // Noncompliant
            ZipFile.Open("", ZipArchiveMode.Read, Encoding.Unicode); // Noncompliant
            ZipFile.OpenRead(""); // Noncompliant
            ZipFile.ExtractToDirectory("", ""); // Noncompliant
            ZipFile.ExtractToDirectory("", "", Encoding.Unicode); // Noncompliant

            // All static methods of IsolatedStorageFile
            IsolatedStorageFile.GetMachineStoreForApplication(); // Noncompliant
            IsolatedStorageFile.GetMachineStoreForAssembly(); // Noncompliant
            IsolatedStorageFile.GetMachineStoreForDomain(); // Noncompliant
            IsolatedStorageFile.GetStore(IsolatedStorageScope.Application, typeof(Program), typeof(Program)); // Noncompliant
            IsolatedStorageFile.GetStore(IsolatedStorageScope.Application, new Evidence(), typeof(Program), new Evidence(), typeof(Program)); // Noncompliant
            IsolatedStorageFile.GetStore(IsolatedStorageScope.Application, typeof(Program)); // Noncompliant
            IsolatedStorageFile.GetStore(IsolatedStorageScope.Application, null); // Noncompliant
            IsolatedStorageFile.GetStore(IsolatedStorageScope.Application, null, null); // Noncompliant
            IsolatedStorageFile.GetUserStoreForApplication(); // Noncompliant
            IsolatedStorageFile.GetUserStoreForAssembly(); // Noncompliant
            IsolatedStorageFile.GetUserStoreForDomain(); // Noncompliant
            IsolatedStorageFile.GetUserStoreForSite(); // Noncompliant
            isolatedStorageFile.CopyFile("", ""); // Compliant, not static

            // All constructors of IsolatedStorageFileStream
            IsolatedStorageFileStream isolatedStorageFileStream;
            isolatedStorageFileStream = new IsolatedStorageFileStream("", FileMode.Append); // Noncompliant
            isolatedStorageFileStream = new IsolatedStorageFileStream("", FileMode.Append, FileAccess.Read); // Noncompliant
            isolatedStorageFileStream = new IsolatedStorageFileStream("", FileMode.Append, isolatedStorageFile); // Noncompliant
            isolatedStorageFileStream = new IsolatedStorageFileStream("", FileMode.Append, FileAccess.Read, FileShare.Read); // Noncompliant
            isolatedStorageFileStream = new IsolatedStorageFileStream("", FileMode.Append, FileAccess.Read, isolatedStorageFile); // Noncompliant
            isolatedStorageFileStream = new IsolatedStorageFileStream("", FileMode.Append, FileAccess.Read, FileShare.Read, 0); // Noncompliant
            isolatedStorageFileStream = new IsolatedStorageFileStream("", FileMode.Append, FileAccess.Read, FileShare.Read, isolatedStorageFile); // Noncompliant
            isolatedStorageFileStream = new IsolatedStorageFileStream("", FileMode.Append, FileAccess.Read, FileShare.Read, 0, isolatedStorageFile); // Noncompliant

            // All static methods that start with Create* on MemoryMappedFile
            MemoryMappedFile.CreateFromFile(""); // Noncompliant
            MemoryMappedFile.CreateFromFile("", FileMode.Append); // Noncompliant
            MemoryMappedFile.CreateFromFile("", FileMode.Append, ""); // Noncompliant
            MemoryMappedFile.CreateFromFile("", FileMode.Append, "", 0); // Noncompliant
            MemoryMappedFile.CreateFromFile("", FileMode.Append, "", 0, MemoryMappedFileAccess.CopyOnWrite); // Noncompliant
            MemoryMappedFile.CreateFromFile(fileStream, "", 0, MemoryMappedFileAccess.CopyOnWrite, memoryMappedFileSecurity, HandleInheritability.Inheritable, true); // Noncompliant
            MemoryMappedFile.CreateNew("", 0); // Noncompliant
            MemoryMappedFile.CreateNew("", 0, MemoryMappedFileAccess.CopyOnWrite); // Noncompliant
            MemoryMappedFile.CreateNew("", 0, MemoryMappedFileAccess.CopyOnWrite, MemoryMappedFileOptions.DelayAllocatePages, memoryMappedFileSecurity, HandleInheritability.Inheritable); // Noncompliant
            MemoryMappedFile.CreateOrOpen("", 0); // Noncompliant
            MemoryMappedFile.CreateOrOpen("", 0, MemoryMappedFileAccess.CopyOnWrite); // Noncompliant
            MemoryMappedFile.CreateOrOpen("", 0, MemoryMappedFileAccess.CopyOnWrite, MemoryMappedFileOptions.DelayAllocatePages, memoryMappedFileSecurity, HandleInheritability.Inheritable); // Noncompliant
            memoryMappedFile.CreateViewAccessor(0, 0); // Compliant, not static

            // All static methods that start with Open* on MemoryMappedFile
            MemoryMappedFile.OpenExisting(""); // Noncompliant
            MemoryMappedFile.OpenExisting("", MemoryMappedFileRights.CopyOnWrite); // Noncompliant
            MemoryMappedFile.OpenExisting("", MemoryMappedFileRights.CopyOnWrite, HandleInheritability.Inheritable); // Noncompliant
            memoryMappedFile.GetAccessControl(); // Compliant, not static, not starting with Open or Create
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern SafeFileHandle CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        static SafeFileHandle CreateFile()
        {
            return null;
        }
    }
}
