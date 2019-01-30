'SonarAnalyzer for .NET
'Copyright(C) 2015-2019 SonarSource SA
'mailto: contact AT sonarsource DOT com

'This program Is free software; you can redistribute it And/Or
'modify it under the terms Of the GNU Lesser General Public
'License as published by the Free Software Foundation; either
'version 3 Of the License, Or (at your option) any later version.

'This program Is distributed In the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty Of
'MERCHANTABILITY Or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
'Lesser General Public License For more details.

'You should have received a copy Of the GNU Lesser General Public License
'along with this program; if Not, write To the Free Software Foundation,
'Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

Namespace Tests.Diagnostics
    Class Foo
        Sub New() ' TEST
            ' HELLO
            REM This is also a syntax for line comment
        End Sub
    End Class

End Namespace


' Some comment here
