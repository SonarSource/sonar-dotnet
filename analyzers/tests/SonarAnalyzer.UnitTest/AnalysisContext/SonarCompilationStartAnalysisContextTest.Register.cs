/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using System.Linq.Expressions;
using Moq;
using SonarAnalyzer.AnalysisContext;

namespace SonarAnalyzer.UnitTest.AnalysisContext
{
    public partial class SonarAnalysisContextTest
    {
        [TestMethod]
        public void SonarCompilationStartAnalysisContext_RegisterCompilationEndAction() =>
            TestStartContextRegistration<SonarCompilationReportingContext>(
                registrationSetup: x => x.RegisterCompilationEndAction(It.IsAny<Action<CompilationAnalysisContext>>()),
                registration: (context, action) => context.RegisterCompilationEndAction(action));

        [TestMethod]
        public void SonarCompilationStartAnalysisContext_RegisterSemanticModel()
        {
            TestStartContextRegistration<SonarSematicModelReportingContext>(
                registrationSetup: x => x.RegisterSemanticModelAction(It.IsAny<Action<SemanticModelAnalysisContext>>()),
                registration: (context, action) => context.RegisterSemanticModelAction(action));
        }

        [TestMethod]
        public void SonarCompilationStartAnalysisContext_RegisterSymbolAction() =>
            TestStartContextRegistration<SonarSymbolReportingContext>(
                registrationSetup: x => x.RegisterSymbolAction(It.IsAny<Action<SymbolAnalysisContext>>(), It.IsAny<ImmutableArray<SymbolKind>>()),
                registration: (context, action) => context.RegisterSymbolAction(action));

        public void TestStartContextRegistration<TSonarContext>(Expression<Action<CompilationStartAnalysisContext>> registrationSetup,
            Action<SonarCompilationStartAnalysisContext, Action<TSonarContext>> registration)
        {
            var context = new DummyAnalysisContext(TestContext);
            var roslynStartContextMock = new Mock<CompilationStartAnalysisContext>(context.Model.Compilation, context.Options, CancellationToken.None);
            roslynStartContextMock.Setup(registrationSetup).Callback(new InvocationAction(x =>
                (x.Arguments[0] as Delegate).DynamicInvoke(new object[] { null })));
            var startContext = new SonarCompilationStartAnalysisContext(new(context, DummyMainDescriptor), roslynStartContextMock.Object);
            var wasExecuted = 0;
            registration(startContext, x => wasExecuted++);
            wasExecuted.Should().Be(1);
            roslynStartContextMock.Verify(registrationSetup, Times.Once);
            roslynStartContextMock.VerifyNoOtherCalls();
        }
    }
}
