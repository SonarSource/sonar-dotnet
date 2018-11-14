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
    public abstract class SendingHttpRequestsBase<TSyntaxKind> : SonarDiagnosticAnalyzer
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S4825";
        protected const string MessageFormat = "Make sure that this http request is sent safely.";

        protected ObjectCreationTracker<TSyntaxKind> ObjectCreationTracker { get; set; }
        protected InvocationTracker<TSyntaxKind> InvocationTracker { get; set;  }

        protected override void Initialize(SonarAnalysisContext context)
        {
            ObjectCreationTracker.Track(context,
                ObjectCreationTracker.WhenDerivesOrImplements(KnownType.RestSharp_IRestRequest));

            InvocationTracker.Track(context,
                InvocationTracker.MatchSimpleNames(
                    new MethodSignature(KnownType.System_Net_Http_HttpClient, "GetAsync"),
                    new MethodSignature(KnownType.System_Net_Http_HttpClient, "GetByteArrayAsync"),
                    new MethodSignature(KnownType.System_Net_Http_HttpClient, "GetStreamAsync"),
                    new MethodSignature(KnownType.System_Net_Http_HttpClient, "GetStringAsync"),
                    new MethodSignature(KnownType.System_Net_Http_HttpClient, "SendAsync"),
                    new MethodSignature(KnownType.System_Net_Http_HttpClient, "PostAsync"),
                    new MethodSignature(KnownType.System_Net_Http_HttpClient, "PutAsync"),
                    new MethodSignature(KnownType.System_Net_Http_HttpClient, "DeleteAsync"),

                    new MethodSignature(KnownType.System_Net_WebClient, "DownloadData"),
                    new MethodSignature(KnownType.System_Net_WebClient, "DownloadDataAsync"),
                    new MethodSignature(KnownType.System_Net_WebClient, "DownloadDataTaskAsync"),
                    new MethodSignature(KnownType.System_Net_WebClient, "DownloadFile"),
                    new MethodSignature(KnownType.System_Net_WebClient, "DownloadFileAsync"),
                    new MethodSignature(KnownType.System_Net_WebClient, "DownloadFileTaskAsync"),
                    new MethodSignature(KnownType.System_Net_WebClient, "DownloadString"),
                    new MethodSignature(KnownType.System_Net_WebClient, "DownloadStringAsync"),
                    new MethodSignature(KnownType.System_Net_WebClient, "DownloadStringTaskAsync"),
                    new MethodSignature(KnownType.System_Net_WebClient, "OpenRead"),
                    new MethodSignature(KnownType.System_Net_WebClient, "OpenReadAsync"),
                    new MethodSignature(KnownType.System_Net_WebClient, "OpenReadTaskAsync"),
                    new MethodSignature(KnownType.System_Net_WebClient, "OpenWrite"),
                    new MethodSignature(KnownType.System_Net_WebClient, "OpenWriteAsync"),
                    new MethodSignature(KnownType.System_Net_WebClient, "OpenWriteTaskAsync"),
                    new MethodSignature(KnownType.System_Net_WebClient, "UploadData"),
                    new MethodSignature(KnownType.System_Net_WebClient, "UploadDataAsync"),
                    new MethodSignature(KnownType.System_Net_WebClient, "UploadDataTaskAsync"),
                    new MethodSignature(KnownType.System_Net_WebClient, "UploadFile"),
                    new MethodSignature(KnownType.System_Net_WebClient, "UploadFileAsync"),
                    new MethodSignature(KnownType.System_Net_WebClient, "UploadFileTaskAsync"),
                    new MethodSignature(KnownType.System_Net_WebClient, "UploadString"),
                    new MethodSignature(KnownType.System_Net_WebClient, "UploadStringAsync"),
                    new MethodSignature(KnownType.System_Net_WebClient, "UploadStringTaskAsync"),
                    new MethodSignature(KnownType.System_Net_WebClient, "UploadValues"),
                    new MethodSignature(KnownType.System_Net_WebClient, "UploadValuesAsync"),
                    new MethodSignature(KnownType.System_Net_WebClient, "UploadValuesTaskAsync"),

                    new MethodSignature(KnownType.System_Net_WebRequest, "Create"),
                    new MethodSignature(KnownType.System_Net_WebRequest, "CreateDefault"),
                    new MethodSignature(KnownType.System_Net_WebRequest, "CreateHttp")));
        }


        #region Syntax-specific abstract methods

        // Add abstract methods for language-specific checks here...

        #endregion
    }
}
