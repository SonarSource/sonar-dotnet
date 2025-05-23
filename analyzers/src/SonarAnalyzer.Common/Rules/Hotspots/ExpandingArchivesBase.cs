﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.Rules
{
    public abstract class ExpandingArchivesBase<TSyntaxKind> : TrackerHotspotDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S5042";
        protected const string MessageFormat = "Make sure that decompressing this archive file is safe.";

        protected ExpandingArchivesBase(IAnalyzerConfiguration configuration) : base(configuration, DiagnosticId, MessageFormat) { }

        protected override void Initialize(TrackerInput input)
        {
            var t = Language.Tracker.Invocation;
            t.Track(input,
                t.MatchMethod(
                    new MemberDescriptor(KnownType.System_IO_Compression_ZipFileExtensions, "ExtractToFile"),
                    new MemberDescriptor(KnownType.System_IO_Compression_ZipFileExtensions, "ExtractToDirectory"),
                    new MemberDescriptor(KnownType.System_IO_Compression_ZipFile, "ExtractToDirectory")));
        }
    }
}
