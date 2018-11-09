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
                InvocationTracker.MatchSimpleNames(
                    new MethodSignature(KnownType.System_IO_File, "AppendAllLines"),
                    new MethodSignature(KnownType.System_IO_File, "AppendAllText"),
                    new MethodSignature(KnownType.System_IO_File, "AppendText"),
                    new MethodSignature(KnownType.System_IO_File, "Copy"),
                    new MethodSignature(KnownType.System_IO_File, "Create"),
                    new MethodSignature(KnownType.System_IO_File, "CreateText"),
                    new MethodSignature(KnownType.System_IO_File, "Decrypt"),
                    new MethodSignature(KnownType.System_IO_File, "Delete"),
                    new MethodSignature(KnownType.System_IO_File, "Encrypt"),
                    new MethodSignature(KnownType.System_IO_File, "Exists"),
                    new MethodSignature(KnownType.System_IO_File, "GetAccessControl"),
                    new MethodSignature(KnownType.System_IO_File, "GetAttributes"),
                    new MethodSignature(KnownType.System_IO_File, "GetCreationTime"),
                    new MethodSignature(KnownType.System_IO_File, "GetCreationTimeUtc"),
                    new MethodSignature(KnownType.System_IO_File, "GetLastAccessTime"),
                    new MethodSignature(KnownType.System_IO_File, "GetLastAccessTimeUtc"),
                    new MethodSignature(KnownType.System_IO_File, "GetLastWriteTime"),
                    new MethodSignature(KnownType.System_IO_File, "GetLastWriteTimeUtc"),
                    new MethodSignature(KnownType.System_IO_File, "Move"),
                    new MethodSignature(KnownType.System_IO_File, "Open"),
                    new MethodSignature(KnownType.System_IO_File, "OpenRead"),
                    new MethodSignature(KnownType.System_IO_File, "OpenText"),
                    new MethodSignature(KnownType.System_IO_File, "OpenWrite"),
                    new MethodSignature(KnownType.System_IO_File, "ReadAllBytes"),
                    new MethodSignature(KnownType.System_IO_File, "ReadAllLines"),
                    new MethodSignature(KnownType.System_IO_File, "ReadAllText"),
                    new MethodSignature(KnownType.System_IO_File, "ReadLines"),
                    new MethodSignature(KnownType.System_IO_File, "Replace"),
                    new MethodSignature(KnownType.System_IO_File, "SetAccessControl"),
                    new MethodSignature(KnownType.System_IO_File, "SetAttributes"),
                    new MethodSignature(KnownType.System_IO_File, "SetCreationTime"),
                    new MethodSignature(KnownType.System_IO_File, "SetCreationTimeUtc"),
                    new MethodSignature(KnownType.System_IO_File, "SetLastAccessTime"),
                    new MethodSignature(KnownType.System_IO_File, "SetLastAccessTimeUtc"),
                    new MethodSignature(KnownType.System_IO_File, "SetLastWriteTime"),
                    new MethodSignature(KnownType.System_IO_File, "SetLastWriteTimeUtc"),
                    new MethodSignature(KnownType.System_IO_File, "WriteAllBytes"),
                    new MethodSignature(KnownType.System_IO_File, "WriteAllLines"),
                    new MethodSignature(KnownType.System_IO_File, "WriteAllText"),

                    new MethodSignature(KnownType.System_IO_Directory, "CreateDirectory"),
                    new MethodSignature(KnownType.System_IO_Directory, "Delete"),
                    new MethodSignature(KnownType.System_IO_Directory, "EnumerateDirectories"),
                    new MethodSignature(KnownType.System_IO_Directory, "EnumerateFiles"),
                    new MethodSignature(KnownType.System_IO_Directory, "EnumerateFileSystemEntries"),
                    new MethodSignature(KnownType.System_IO_Directory, "Exists"),
                    new MethodSignature(KnownType.System_IO_Directory, "GetAccessControl"),
                    new MethodSignature(KnownType.System_IO_Directory, "GetCreationTime"),
                    new MethodSignature(KnownType.System_IO_Directory, "GetCreationTimeUtc"),
                    new MethodSignature(KnownType.System_IO_Directory, "GetCurrentDirectory"),
                    new MethodSignature(KnownType.System_IO_Directory, "GetDirectories"),
                    new MethodSignature(KnownType.System_IO_Directory, "GetDirectoryRoot"),
                    new MethodSignature(KnownType.System_IO_Directory, "GetFiles"),
                    new MethodSignature(KnownType.System_IO_Directory, "GetFileSystemEntries"),
                    new MethodSignature(KnownType.System_IO_Directory, "GetLastAccessTime"),
                    new MethodSignature(KnownType.System_IO_Directory, "GetLastAccessTimeUtc"),
                    new MethodSignature(KnownType.System_IO_Directory, "GetLastWriteTime"),
                    new MethodSignature(KnownType.System_IO_Directory, "GetLastWriteTimeUtc"),
                    new MethodSignature(KnownType.System_IO_Directory, "GetLogicalDrives"),
                    new MethodSignature(KnownType.System_IO_Directory, "GetParent"),
                    new MethodSignature(KnownType.System_IO_Directory, "Move"),
                    new MethodSignature(KnownType.System_IO_Directory, "SetAccessControl"),
                    new MethodSignature(KnownType.System_IO_Directory, "SetCreationTime"),
                    new MethodSignature(KnownType.System_IO_Directory, "SetCreationTimeUtc"),
                    new MethodSignature(KnownType.System_IO_Directory, "SetCurrentDirectory"),
                    new MethodSignature(KnownType.System_IO_Directory, "SetLastAccessTime"),
                    new MethodSignature(KnownType.System_IO_Directory, "SetLastAccessTimeUtc"),
                    new MethodSignature(KnownType.System_IO_Directory, "SetLastWriteTime"),
                    new MethodSignature(KnownType.System_IO_Directory, "SetLastWriteTimeUtc"),

                    new MethodSignature(KnownType.System_IO_Path, "GetTempFileName"),
                    new MethodSignature(KnownType.System_IO_Path, "GetTempPath"),

                    new MethodSignature(KnownType.System_IO_IsolatedStorage_IsolatedStorageFile, "GetEnumerator"),
                    new MethodSignature(KnownType.System_IO_IsolatedStorage_IsolatedStorageFile, "GetMachineStoreForApplication"),
                    new MethodSignature(KnownType.System_IO_IsolatedStorage_IsolatedStorageFile, "GetMachineStoreForAssembly"),
                    new MethodSignature(KnownType.System_IO_IsolatedStorage_IsolatedStorageFile, "GetMachineStoreForDomain"),
                    new MethodSignature(KnownType.System_IO_IsolatedStorage_IsolatedStorageFile, "GetStore"),
                    new MethodSignature(KnownType.System_IO_IsolatedStorage_IsolatedStorageFile, "GetUserStoreForApplication"),
                    new MethodSignature(KnownType.System_IO_IsolatedStorage_IsolatedStorageFile, "GetUserStoreForAssembly"),
                    new MethodSignature(KnownType.System_IO_IsolatedStorage_IsolatedStorageFile, "GetUserStoreForDomain"),
                    new MethodSignature(KnownType.System_IO_IsolatedStorage_IsolatedStorageFile, "GetUserStoreForSite"),
                    new MethodSignature(KnownType.System_IO_IsolatedStorage_IsolatedStorageFile, "Remove"),

                    new MethodSignature(KnownType.System_IO_MemoryMappedFiles_MemoryMappedFile, "CreateFromFile"),
                    new MethodSignature(KnownType.System_IO_MemoryMappedFiles_MemoryMappedFile, "CreateNew"),
                    new MethodSignature(KnownType.System_IO_MemoryMappedFiles_MemoryMappedFile, "CreateOrOpen"),
                    new MethodSignature(KnownType.System_IO_MemoryMappedFiles_MemoryMappedFile, "OpenExisting"),

                    new MethodSignature(KnownType.System_IO_Compression_ZipFile, "CreateFromDirectory"),
                    new MethodSignature(KnownType.System_IO_Compression_ZipFile, "ExtractToDirectory"),
                    new MethodSignature(KnownType.System_IO_Compression_ZipFile, "Open"),
                    new MethodSignature(KnownType.System_IO_Compression_ZipFile, "OpenRead")));

            InvocationTracker.Track(context,
                InvocationTracker.MethodNameIs("CreateFile"),
                InvocationTracker.IsExtern());

            ObjectCreationTracker.Track(context,
                ObjectCreationTracker.MatchConstructors(
                    KnownType.System_IO_StreamWriter,
                    KnownType.System_IO_StreamReader,
                    KnownType.System_Security_AccessControl_FileSecurity),
                ObjectCreationTracker.FirstArgumentIs(KnownType.System_String));

            ObjectCreationTracker.Track(context,
                ObjectCreationTracker.MatchConstructors(
                    KnownType.System_IO_FileInfo,
                    KnownType.System_IO_DirectoryInfo,
                    KnownType.System_IO_IsolatedStorage_IsolatedStorageFileStream,
                    KnownType.Microsoft_Win32_SafeHandles_SafeFileHandle));

            ObjectCreationTracker.Track(context,
                ObjectCreationTracker.MatchConstructors(
                    KnownType.System_IO_FileStream),
                Conditions.ExceptWhen(ObjectCreationTracker.FirstArgumentIs(KnownType.Microsoft_Win32_SafeHandles_SafeFileHandle)));
        }
    }
}
