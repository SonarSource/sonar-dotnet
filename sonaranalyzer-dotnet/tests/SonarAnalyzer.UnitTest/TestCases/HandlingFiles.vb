Imports System
Imports System.Text
Imports System.IO
Imports Microsoft.Win32.SafeHandles
Imports System.Security.AccessControl
Imports System.IO.Compression
Imports System.IO.IsolatedStorage
Imports System.IO.MemoryMappedFiles
Imports System.Runtime.InteropServices
Imports System.Security.Policy

Namespace Tests.Diagnostics
    Class Program
        Private Sub Main(ByVal intPtr As IntPtr, ByVal isolatedStorageFile As IsolatedStorageFile, ByVal memoryMappedFile As MemoryMappedFile, ByVal memoryMappedFileSecurity As MemoryMappedFileSecurity, ByVal stream As Stream)
            ' Any static method call on File or Directory will raise
            File.Exists("") ' Noncompliant {{Make sure this file handling is safe here.}}
'           ^^^^^^^^^^^^^^^
            File.Create("") ' Noncompliant
            File.Delete("") ' Noncompliant
            File.ReadLines("") ' Noncompliant
            Directory.Exists("") ' Noncompliant
            Directory.Delete("") ' Noncompliant
            Directory.EnumerateFiles("") ' Noncompliant

            ' Any FileInfo or DirectoryInfo creation
            Dim fileInfo = New FileInfo("") ' Noncompliant {{Make sure this file handling is safe here.}}
'                          ^^^^^^^^^^^^^^^^
            Dim dirInfo = New DirectoryInfo("") ' Noncompliant

            ' Calls to extern CreateFile
            Dim handle As SafeFileHandle
            handle = CreateFile("", 0, 0, intPtr, 0, 0, intPtr) ' Noncompliant
            handle = CreateFile() ' Compliant, not extern

            ' Creation of SafeFileHandle
            handle = New SafeFileHandle(IntPtr.Zero, False) ' Noncompliant

            ' All constructors of FileStream
            Dim fileStream As FileStream
            fileStream = New FileStream(IntPtr.Zero, FileAccess.Read) ' Noncompliant
            fileStream = New FileStream(handle, FileAccess.Read) ' Compliant, created from SafeFileHandle, which should be already reported
            fileStream = New FileStream("", FileMode.Append) ' Noncompliant
            fileStream = New FileStream(IntPtr.Zero, FileAccess.Read, True) ' Noncompliant
            fileStream = New FileStream(handle, FileAccess.Read, 0) ' Compliant, created from SafeFileHandle, which should be already reported
            fileStream = New FileStream("", FileMode.Append, FileAccess.Read) ' Noncompliant
            fileStream = New FileStream(IntPtr.Zero, FileAccess.Read, True, 0) ' Noncompliant
            fileStream = New FileStream(handle, FileAccess.Read, 0, True) ' Compliant, created from SafeFileHandle, which should be already reported
            fileStream = New FileStream("", FileMode.Append, FileAccess.Read, FileShare.Read) ' Noncompliant
            fileStream = New FileStream(IntPtr.Zero, FileAccess.Read, True, 0, True) ' Noncompliant
            fileStream = New FileStream("", FileMode.Append, FileAccess.Read, FileShare.Read, 0) ' Noncompliant
            fileStream = New FileStream("", FileMode.Append, FileAccess.Read, FileShare.Read, 0, True) ' Noncompliant
            fileStream = New FileStream("", FileMode.Append, FileAccess.Read, FileShare.Read, 0, FileOptions.Asynchronous) ' Noncompliant
            fileStream = New FileStream("", FileMode.Append, FileSystemRights.Read, FileShare.Read, 0, FileOptions.Asynchronous) ' Noncompliant
            fileStream = New FileStream("", FileMode.Append, FileSystemRights.Read, FileShare.Read, 0, FileOptions.Asynchronous, New FileSecurity()) ' Noncompliant

            ' All constructors of StreamWriter, whos first argument is string
            Dim streamWriter As StreamWriter
            streamWriter = New StreamWriter(stream)
            streamWriter = New StreamWriter("") ' Noncompliant
            streamWriter = New StreamWriter(stream, Encoding.Unicode)
            streamWriter = New StreamWriter("", True) ' Noncompliant
            streamWriter = New StreamWriter(stream, Encoding.Unicode, 0)
            streamWriter = New StreamWriter("", True, Encoding.Unicode) ' Noncompliant
            streamWriter = New StreamWriter(stream, Encoding.Unicode, 0, True)
            streamWriter = New StreamWriter("", True, Encoding.Unicode, 0) ' Noncompliant

            ' All constructors of StreamReader, whos first argument is string
            Dim streamReader As StreamReader
            streamReader = New StreamReader(stream)
            streamReader = New StreamReader("") ' Noncompliant
            streamReader = New StreamReader(stream, True)
            streamReader = New StreamReader(stream, Encoding.Unicode)
            streamReader = New StreamReader("", True) ' Noncompliant
            streamReader = New StreamReader("", Encoding.Unicode) ' Noncompliant
            streamReader = New StreamReader(stream, Encoding.Unicode, True)
            streamReader = New StreamReader("", Encoding.Unicode, True) ' Noncompliant
            streamReader = New StreamReader(stream, Encoding.Unicode, True, 0)
            streamReader = New StreamReader("", Encoding.Unicode, True, 0) ' Noncompliant
            streamReader = New StreamReader(stream, Encoding.Unicode, True, 0, True)

            Path.GetTempFileName() ' Noncompliant
            Path.GetTempPath() ' Noncompliant

            Dim fileSecurity As FileSecurity
            fileSecurity = New FileSecurity() ' Compliant
            fileSecurity = New FileSecurity("", AccessControlSections.All) ' Noncompliant

            ' All static methods of ZipFile (all methods are static!)
            ZipFile.CreateFromDirectory("", "") ' Noncompliant
            ZipFile.CreateFromDirectory("", "", CompressionLevel.Fastest, True) ' Noncompliant
            ZipFile.CreateFromDirectory("", "", CompressionLevel.Fastest, True, Encoding.Unicode) ' Noncompliant
            ZipFile.Open("", ZipArchiveMode.Read) ' Noncompliant
            ZipFile.Open("", ZipArchiveMode.Read, Encoding.Unicode) ' Noncompliant
            ZipFile.OpenRead("") ' Noncompliant
            ZipFile.ExtractToDirectory("", "") ' Noncompliant
            ZipFile.ExtractToDirectory("", "", Encoding.Unicode) ' Noncompliant

            ' All static methods of IsolatedStorageFile
            IsolatedStorageFile.GetMachineStoreForApplication() ' Noncompliant
            IsolatedStorageFile.GetMachineStoreForAssembly() ' Noncompliant
            IsolatedStorageFile.GetMachineStoreForDomain() ' Noncompliant
            IsolatedStorageFile.GetStore(IsolatedStorageScope.Application, GetType(Program), GetType(Program)) ' Noncompliant
            IsolatedStorageFile.GetStore(IsolatedStorageScope.Application, New Evidence(), GetType(Program), New Evidence(), GetType(Program)) ' Noncompliant
            IsolatedStorageFile.GetStore(IsolatedStorageScope.Application, GetType(Program)) ' Noncompliant
            IsolatedStorageFile.GetStore(IsolatedStorageScope.Application, Nothing) ' Noncompliant
            IsolatedStorageFile.GetStore(IsolatedStorageScope.Application, Nothing, Nothing) ' Noncompliant
            IsolatedStorageFile.GetUserStoreForApplication() ' Noncompliant
            IsolatedStorageFile.GetUserStoreForAssembly() ' Noncompliant
            IsolatedStorageFile.GetUserStoreForDomain() ' Noncompliant
            IsolatedStorageFile.GetUserStoreForSite() ' Noncompliant
            isolatedStorageFile.CopyFile("", "") ' Compliant, not static

            ' All constructors of IsolatedStorageFileStream
            Dim isolatedStorageFileStream As IsolatedStorageFileStream
            isolatedStorageFileStream = New IsolatedStorageFileStream("", FileMode.Append) ' Noncompliant
            isolatedStorageFileStream = New IsolatedStorageFileStream("", FileMode.Append, FileAccess.Read) ' Noncompliant
            isolatedStorageFileStream = New IsolatedStorageFileStream("", FileMode.Append, isolatedStorageFile) ' Noncompliant
            isolatedStorageFileStream = New IsolatedStorageFileStream("", FileMode.Append, FileAccess.Read, FileShare.Read) ' Noncompliant
            isolatedStorageFileStream = New IsolatedStorageFileStream("", FileMode.Append, FileAccess.Read, isolatedStorageFile) ' Noncompliant
            isolatedStorageFileStream = New IsolatedStorageFileStream("", FileMode.Append, FileAccess.Read, FileShare.Read, 0) ' Noncompliant
            isolatedStorageFileStream = New IsolatedStorageFileStream("", FileMode.Append, FileAccess.Read, FileShare.Read, isolatedStorageFile) ' Noncompliant
            isolatedStorageFileStream = New IsolatedStorageFileStream("", FileMode.Append, FileAccess.Read, FileShare.Read, 0, isolatedStorageFile) ' Noncompliant

            ' All static methods that start with Create* on MemoryMappedFile
            MemoryMappedFile.CreateFromFile("") ' Noncompliant
            MemoryMappedFile.CreateFromFile("", FileMode.Append) ' Noncompliant
            MemoryMappedFile.CreateFromFile("", FileMode.Append, "") ' Noncompliant
            MemoryMappedFile.CreateFromFile("", FileMode.Append, "", 0) ' Noncompliant
            MemoryMappedFile.CreateFromFile("", FileMode.Append, "", 0, MemoryMappedFileAccess.CopyOnWrite) ' Noncompliant
            MemoryMappedFile.CreateFromFile(fileStream, "", 0, MemoryMappedFileAccess.CopyOnWrite, memoryMappedFileSecurity, HandleInheritability.Inheritable, True) ' Noncompliant
            MemoryMappedFile.CreateNew("", 0) ' Noncompliant
            MemoryMappedFile.CreateNew("", 0, MemoryMappedFileAccess.CopyOnWrite) ' Noncompliant
            MemoryMappedFile.CreateNew("", 0, MemoryMappedFileAccess.CopyOnWrite, MemoryMappedFileOptions.DelayAllocatePages, memoryMappedFileSecurity, HandleInheritability.Inheritable) ' Noncompliant
            MemoryMappedFile.CreateOrOpen("", 0) ' Noncompliant
            MemoryMappedFile.CreateOrOpen("", 0, MemoryMappedFileAccess.CopyOnWrite) ' Noncompliant
            MemoryMappedFile.CreateOrOpen("", 0, MemoryMappedFileAccess.CopyOnWrite, MemoryMappedFileOptions.DelayAllocatePages, memoryMappedFileSecurity, HandleInheritability.Inheritable) ' Noncompliant
            memoryMappedFile.CreateViewAccessor(0, 0) ' Compliant, not static

            ' All static methods that start with Open* on MemoryMappedFile
            MemoryMappedFile.OpenExisting("") ' Noncompliant
            MemoryMappedFile.OpenExisting("", MemoryMappedFileRights.CopyOnWrite) ' Noncompliant
            MemoryMappedFile.OpenExisting("", MemoryMappedFileRights.CopyOnWrite, HandleInheritability.Inheritable) ' Noncompliant
            memoryMappedFile.GetAccessControl() ' Compliant, not static
        End Sub

        <DllImport("kernel32.dll", SetLastError:=True, CharSet:=CharSet.Unicode)>
        Private Shared Function CreateFile(ByVal lpFileName As String, ByVal dwDesiredAccess As UInteger, ByVal dwShareMode As UInteger, ByVal lpSecurityAttributes As IntPtr, ByVal dwCreationDisposition As UInteger, ByVal dwFlagsAndAttributes As UInteger, ByVal hTemplateFile As IntPtr) As SafeFileHandle
        End Function

        Private Shared Function CreateFile() As SafeFileHandle
            Return Nothing
        End Function
    End Class
End Namespace
