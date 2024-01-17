Imports Mono.Unix

Public Class S2612TestCasesUnix
    Public Sub UpdatePermissionsOnUnixUsingMonoPosix()
            Dim fileSystemEntry = UnixFileSystemInfo.GetFileSystemEntry("path")

            fileSystemEntry.FileAccessPermissions = FileAccessPermissions.UserRead
            fileSystemEntry.FileAccessPermissions = FileAccessPermissions.UserWrite
            fileSystemEntry.FileAccessPermissions = FileAccessPermissions.UserExecute
            fileSystemEntry.FileAccessPermissions = FileAccessPermissions.UserReadWriteExecute

            fileSystemEntry.FileAccessPermissions = FileAccessPermissions.GroupRead
            fileSystemEntry.FileAccessPermissions = FileAccessPermissions.GroupWrite
            fileSystemEntry.FileAccessPermissions = FileAccessPermissions.GroupExecute
            fileSystemEntry.FileAccessPermissions = FileAccessPermissions.GroupReadWriteExecute

            fileSystemEntry.FileAccessPermissions = FileAccessPermissions.UserRead OR FileAccessPermissions.GroupRead

            fileSystemEntry.FileAccessPermissions = FileAccessPermissions.AllPermissions ' Noncompliant (DefaultPermissions | OtherExecute | GroupExecute | UserExecute, // 0x000001FF)
            fileSystemEntry.FileAccessPermissions = FileAccessPermissions.DefaultPermissions ' Noncompliant (OtherWrite | OtherRead | GroupWrite | GroupRead | UserWrite | UserRead, // 0x000001B6)
            fileSystemEntry.FileAccessPermissions = FileAccessPermissions.OtherReadWriteExecute ' Noncompliant
            fileSystemEntry.FileAccessPermissions = FileAccessPermissions.OtherExecute ' Noncompliant
            fileSystemEntry.FileAccessPermissions = FileAccessPermissions.OtherWrite ' Noncompliant
            fileSystemEntry.FileAccessPermissions = FileAccessPermissions.OtherRead ' Noncompliant {{Make sure this permission is safe.}}

            fileSystemEntry.FileAccessPermissions ^= FileAccessPermissions.AllPermissions ' Noncompliant - depending on the case this might add or remove permissions
            fileSystemEntry.FileAccessPermissions ^= FileAccessPermissions.DefaultPermissions ' Noncompliant
            fileSystemEntry.FileAccessPermissions ^= FileAccessPermissions.OtherReadWriteExecute ' Noncompliant
            fileSystemEntry.FileAccessPermissions ^= FileAccessPermissions.OtherExecute ' Noncompliant
            fileSystemEntry.FileAccessPermissions ^= FileAccessPermissions.OtherWrite ' Noncompliant
            fileSystemEntry.FileAccessPermissions ^= FileAccessPermissions.OtherRead ' Noncompliant

            fileSystemEntry.FileAccessPermissions = FileAccessPermissions.UserRead Or FileAccessPermissions.GroupRead Or FileAccessPermissions.OtherRead ' Noncompliant
'                                                                                                                                              ^^^^^^^^^
            UnixFileInfo.GetFileSystemEntry("path").FileAccessPermissions = FileAccessPermissions.AllPermissions ' Noncompliant
            UnixSymbolicLinkInfo.GetFileSystemEntry("path").FileAccessPermissions = FileAccessPermissions.AllPermissions ' Noncompliant

            if fileSystemEntry.FileAccessPermissions <> FileAccessPermissions.OtherExecute
            End If

            while fileSystemEntry.FileAccessPermissions <> FileAccessPermissions.OtherExecute
            End While
    End Sub

    Public Class Base
        Protected Sub New(permissions As FileAccessPermissions)
        End Sub

        Public Sub New()
            Me.New(FileAccessPermissions.DefaultPermissions) ' Noncompliant
'                                        ^^^^^^^^^^^^^^^^^^
        End Sub
    End Class

    Public Class Subclass
        Inherits Base
        Sub New()
            MyBase.New(FileAccessPermissions.DefaultPermissions) ' Noncompliant
        End Sub
    End Class

End Class
