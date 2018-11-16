/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class HandlingFilesBase<TSyntaxKind> : SonarDiagnosticAnalyzer
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S4797";
        protected const string MessageFormat = "Make sure this file handling is safe here.";

        protected InvocationTracker<TSyntaxKind> InvocationTracker { get; set; }

        protected ObjectCreationTracker<TSyntaxKind> ObjectCreationTracker { get; set; }

        protected override void Initialize(SonarAnalysisContext context)
        {
            InvocationTracker.Track(context,
                InvocationTracker.MatchMethod(
                    new MemberDescriptor(KnownType.System_IO_File, "AppendAllLines"),
                    new MemberDescriptor(KnownType.System_IO_File, "AppendAllText"),
                    new MemberDescriptor(KnownType.System_IO_File, "AppendText"),
                    new MemberDescriptor(KnownType.System_IO_File, "Copy"),
                    new MemberDescriptor(KnownType.System_IO_File, "Create"),
                    new MemberDescriptor(KnownType.System_IO_File, "CreateText"),
                    new MemberDescriptor(KnownType.System_IO_File, "Decrypt"),
                    new MemberDescriptor(KnownType.System_IO_File, "Delete"),
                    new MemberDescriptor(KnownType.System_IO_File, "Encrypt"),
                    new MemberDescriptor(KnownType.System_IO_File, "Exists"),
                    new MemberDescriptor(KnownType.System_IO_File, "GetAccessControl"),
                    new MemberDescriptor(KnownType.System_IO_File, "GetAttributes"),
                    new MemberDescriptor(KnownType.System_IO_File, "GetCreationTime"),
                    new MemberDescriptor(KnownType.System_IO_File, "GetCreationTimeUtc"),
                    new MemberDescriptor(KnownType.System_IO_File, "GetLastAccessTime"),
                    new MemberDescriptor(KnownType.System_IO_File, "GetLastAccessTimeUtc"),
                    new MemberDescriptor(KnownType.System_IO_File, "GetLastWriteTime"),
                    new MemberDescriptor(KnownType.System_IO_File, "GetLastWriteTimeUtc"),
                    new MemberDescriptor(KnownType.System_IO_File, "Move"),
                    new MemberDescriptor(KnownType.System_IO_File, "Open"),
                    new MemberDescriptor(KnownType.System_IO_File, "OpenRead"),
                    new MemberDescriptor(KnownType.System_IO_File, "OpenText"),
                    new MemberDescriptor(KnownType.System_IO_File, "OpenWrite"),
                    new MemberDescriptor(KnownType.System_IO_File, "ReadAllBytes"),
                    new MemberDescriptor(KnownType.System_IO_File, "ReadAllLines"),
                    new MemberDescriptor(KnownType.System_IO_File, "ReadAllText"),
                    new MemberDescriptor(KnownType.System_IO_File, "ReadLines"),
                    new MemberDescriptor(KnownType.System_IO_File, "Replace"),
                    new MemberDescriptor(KnownType.System_IO_File, "SetAccessControl"),
                    new MemberDescriptor(KnownType.System_IO_File, "SetAttributes"),
                    new MemberDescriptor(KnownType.System_IO_File, "SetCreationTime"),
                    new MemberDescriptor(KnownType.System_IO_File, "SetCreationTimeUtc"),
                    new MemberDescriptor(KnownType.System_IO_File, "SetLastAccessTime"),
                    new MemberDescriptor(KnownType.System_IO_File, "SetLastAccessTimeUtc"),
                    new MemberDescriptor(KnownType.System_IO_File, "SetLastWriteTime"),
                    new MemberDescriptor(KnownType.System_IO_File, "SetLastWriteTimeUtc"),
                    new MemberDescriptor(KnownType.System_IO_File, "WriteAllBytes"),
                    new MemberDescriptor(KnownType.System_IO_File, "WriteAllLines"),
                    new MemberDescriptor(KnownType.System_IO_File, "WriteAllText"),

                    new MemberDescriptor(KnownType.System_IO_Directory, "CreateDirectory"),
                    new MemberDescriptor(KnownType.System_IO_Directory, "Delete"),
                    new MemberDescriptor(KnownType.System_IO_Directory, "EnumerateDirectories"),
                    new MemberDescriptor(KnownType.System_IO_Directory, "EnumerateFiles"),
                    new MemberDescriptor(KnownType.System_IO_Directory, "EnumerateFileSystemEntries"),
                    new MemberDescriptor(KnownType.System_IO_Directory, "Exists"),
                    new MemberDescriptor(KnownType.System_IO_Directory, "GetAccessControl"),
                    new MemberDescriptor(KnownType.System_IO_Directory, "GetCreationTime"),
                    new MemberDescriptor(KnownType.System_IO_Directory, "GetCreationTimeUtc"),
                    new MemberDescriptor(KnownType.System_IO_Directory, "GetCurrentDirectory"),
                    new MemberDescriptor(KnownType.System_IO_Directory, "GetDirectories"),
                    new MemberDescriptor(KnownType.System_IO_Directory, "GetDirectoryRoot"),
                    new MemberDescriptor(KnownType.System_IO_Directory, "GetFiles"),
                    new MemberDescriptor(KnownType.System_IO_Directory, "GetFileSystemEntries"),
                    new MemberDescriptor(KnownType.System_IO_Directory, "GetLastAccessTime"),
                    new MemberDescriptor(KnownType.System_IO_Directory, "GetLastAccessTimeUtc"),
                    new MemberDescriptor(KnownType.System_IO_Directory, "GetLastWriteTime"),
                    new MemberDescriptor(KnownType.System_IO_Directory, "GetLastWriteTimeUtc"),
                    new MemberDescriptor(KnownType.System_IO_Directory, "GetLogicalDrives"),
                    new MemberDescriptor(KnownType.System_IO_Directory, "GetParent"),
                    new MemberDescriptor(KnownType.System_IO_Directory, "Move"),
                    new MemberDescriptor(KnownType.System_IO_Directory, "SetAccessControl"),
                    new MemberDescriptor(KnownType.System_IO_Directory, "SetCreationTime"),
                    new MemberDescriptor(KnownType.System_IO_Directory, "SetCreationTimeUtc"),
                    new MemberDescriptor(KnownType.System_IO_Directory, "SetCurrentDirectory"),
                    new MemberDescriptor(KnownType.System_IO_Directory, "SetLastAccessTime"),
                    new MemberDescriptor(KnownType.System_IO_Directory, "SetLastAccessTimeUtc"),
                    new MemberDescriptor(KnownType.System_IO_Directory, "SetLastWriteTime"),
                    new MemberDescriptor(KnownType.System_IO_Directory, "SetLastWriteTimeUtc"),

                    new MemberDescriptor(KnownType.System_IO_Path, "GetTempFileName"),
                    new MemberDescriptor(KnownType.System_IO_Path, "GetTempPath"),

                    new MemberDescriptor(KnownType.System_IO_IsolatedStorage_IsolatedStorageFile, "GetEnumerator"),
                    new MemberDescriptor(KnownType.System_IO_IsolatedStorage_IsolatedStorageFile, "GetMachineStoreForApplication"),
                    new MemberDescriptor(KnownType.System_IO_IsolatedStorage_IsolatedStorageFile, "GetMachineStoreForAssembly"),
                    new MemberDescriptor(KnownType.System_IO_IsolatedStorage_IsolatedStorageFile, "GetMachineStoreForDomain"),
                    new MemberDescriptor(KnownType.System_IO_IsolatedStorage_IsolatedStorageFile, "GetStore"),
                    new MemberDescriptor(KnownType.System_IO_IsolatedStorage_IsolatedStorageFile, "GetUserStoreForApplication"),
                    new MemberDescriptor(KnownType.System_IO_IsolatedStorage_IsolatedStorageFile, "GetUserStoreForAssembly"),
                    new MemberDescriptor(KnownType.System_IO_IsolatedStorage_IsolatedStorageFile, "GetUserStoreForDomain"),
                    new MemberDescriptor(KnownType.System_IO_IsolatedStorage_IsolatedStorageFile, "GetUserStoreForSite"),
                    new MemberDescriptor(KnownType.System_IO_IsolatedStorage_IsolatedStorageFile, "Remove"),

                    new MemberDescriptor(KnownType.System_IO_MemoryMappedFiles_MemoryMappedFile, "CreateFromFile"),
                    new MemberDescriptor(KnownType.System_IO_MemoryMappedFiles_MemoryMappedFile, "CreateNew"),
                    new MemberDescriptor(KnownType.System_IO_MemoryMappedFiles_MemoryMappedFile, "CreateOrOpen"),
                    new MemberDescriptor(KnownType.System_IO_MemoryMappedFiles_MemoryMappedFile, "OpenExisting"),

                    new MemberDescriptor(KnownType.System_IO_Compression_ZipFile, "CreateFromDirectory"),
                    new MemberDescriptor(KnownType.System_IO_Compression_ZipFile, "ExtractToDirectory"),
                    new MemberDescriptor(KnownType.System_IO_Compression_ZipFile, "Open"),
                    new MemberDescriptor(KnownType.System_IO_Compression_ZipFile, "OpenRead")));

            InvocationTracker.Track(context,
                InvocationTracker.MethodNameIs("CreateFile"),
                InvocationTracker.IsExtern());

            ObjectCreationTracker.Track(context,
                ObjectCreationTracker.MatchConstructor(
                    KnownType.System_IO_StreamWriter,
                    KnownType.System_IO_StreamReader,
                    KnownType.System_Security_AccessControl_FileSecurity),
                ObjectCreationTracker.FirstArgumentIs(
                    KnownType.System_String));

            ObjectCreationTracker.Track(context,
                ObjectCreationTracker.MatchConstructor(
                    KnownType.System_IO_FileInfo,
                    KnownType.System_IO_DirectoryInfo,
                    KnownType.System_IO_IsolatedStorage_IsolatedStorageFileStream,
                    KnownType.Microsoft_Win32_SafeHandles_SafeFileHandle));

            ObjectCreationTracker.Track(context,
                ObjectCreationTracker.MatchConstructor(
                    KnownType.System_IO_FileStream),
                Conditions.ExceptWhen(
                    ObjectCreationTracker.FirstArgumentIs(
                        KnownType.Microsoft_Win32_SafeHandles_SafeFileHandle)));
        }
    }
}
