using Mono.Unix;

namespace Tests.Diagnostics
{
    class Program
    {
        public void UpdatePermissionsOnUnixUsingMonoPosix()
        {
            var fileSystemEntry = UnixFileSystemInfo.GetFileSystemEntry("path");

            fileSystemEntry.FileAccessPermissions = FileAccessPermissions.UserRead;
            fileSystemEntry.FileAccessPermissions = FileAccessPermissions.UserWrite;
            fileSystemEntry.FileAccessPermissions = FileAccessPermissions.UserExecute;
            fileSystemEntry.FileAccessPermissions = FileAccessPermissions.UserReadWriteExecute;

            fileSystemEntry.FileAccessPermissions = FileAccessPermissions.GroupRead;
            fileSystemEntry.FileAccessPermissions = FileAccessPermissions.GroupWrite;
            fileSystemEntry.FileAccessPermissions = FileAccessPermissions.GroupExecute;
            fileSystemEntry.FileAccessPermissions = FileAccessPermissions.GroupReadWriteExecute;

            fileSystemEntry.FileAccessPermissions = FileAccessPermissions.UserRead | FileAccessPermissions.GroupRead;

            fileSystemEntry.FileAccessPermissions = FileAccessPermissions.AllPermissions; // Noncompliant (DefaultPermissions | OtherExecute | GroupExecute | UserExecute, // 0x000001FF)
            fileSystemEntry.FileAccessPermissions = FileAccessPermissions.DefaultPermissions; // Noncompliant (OtherWrite | OtherRead | GroupWrite | GroupRead | UserWrite | UserRead, // 0x000001B6)
            fileSystemEntry.FileAccessPermissions = FileAccessPermissions.OtherReadWriteExecute; // Noncompliant
            fileSystemEntry.FileAccessPermissions = FileAccessPermissions.OtherExecute; // Noncompliant
            fileSystemEntry.FileAccessPermissions = FileAccessPermissions.OtherWrite; // Noncompliant
            fileSystemEntry.FileAccessPermissions = FileAccessPermissions.OtherRead; // Noncompliant {{Make sure this permission is safe.}}

            fileSystemEntry.FileAccessPermissions |= FileAccessPermissions.AllPermissions; // Noncompliant (DefaultPermissions | OtherExecute | GroupExecute | UserExecute, // 0x000001FF)
            fileSystemEntry.FileAccessPermissions |= FileAccessPermissions.DefaultPermissions; // Noncompliant (OtherWrite | OtherRead | GroupWrite | GroupRead | UserWrite | UserRead, // 0x000001B6)
            fileSystemEntry.FileAccessPermissions |= FileAccessPermissions.OtherReadWriteExecute; // Noncompliant
            fileSystemEntry.FileAccessPermissions |= FileAccessPermissions.OtherExecute; // Noncompliant
            fileSystemEntry.FileAccessPermissions |= FileAccessPermissions.OtherWrite; // Noncompliant
            fileSystemEntry.FileAccessPermissions |= FileAccessPermissions.OtherRead; // Noncompliant

            fileSystemEntry.FileAccessPermissions ^= FileAccessPermissions.AllPermissions; // Noncompliant - depending on the case this might add or remove permissions
            fileSystemEntry.FileAccessPermissions ^= FileAccessPermissions.DefaultPermissions; // Noncompliant
            fileSystemEntry.FileAccessPermissions ^= FileAccessPermissions.OtherReadWriteExecute; // Noncompliant
            fileSystemEntry.FileAccessPermissions ^= FileAccessPermissions.OtherExecute; // Noncompliant
            fileSystemEntry.FileAccessPermissions ^= FileAccessPermissions.OtherWrite; // Noncompliant
            fileSystemEntry.FileAccessPermissions ^= FileAccessPermissions.OtherRead; // Noncompliant

            fileSystemEntry.FileAccessPermissions = FileAccessPermissions.UserRead | FileAccessPermissions.GroupRead | FileAccessPermissions.OtherRead; // Noncompliant
//                                                                                                                                           ^^^^^^^^^
            UnixFileInfo.GetFileSystemEntry("path").FileAccessPermissions = FileAccessPermissions.AllPermissions; // Noncompliant
            UnixSymbolicLinkInfo.GetFileSystemEntry("path").FileAccessPermissions = FileAccessPermissions.AllPermissions; // Noncompliant

            if (fileSystemEntry.FileAccessPermissions != FileAccessPermissions.OtherExecute) { }
            while (fileSystemEntry.FileAccessPermissions != FileAccessPermissions.OtherExecute) { }
            _ = fileSystemEntry.FileAccessPermissions == FileAccessPermissions.OtherWrite ? 1 : 2;

            fileSystemEntry.FileAccessPermissions = ~((FileAccessPermissions.OtherRead));
            fileSystemEntry.FileAccessPermissions = ~FileAccessPermissions.OtherRead;
        }
    }

    class Base
    {
        public Base(FileAccessPermissions permissions) { }

        public Base() : this(FileAccessPermissions.DefaultPermissions) { } // Noncompliant
//                                                 ^^^^^^^^^^^^^^^^^^
    }

    class Subclass : Base
    {
        public Subclass() : base(FileAccessPermissions.DefaultPermissions) { } // Noncompliant
    }
}
