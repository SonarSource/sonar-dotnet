Imports System
Imports Microsoft.AspNetCore.Builder
Imports Microsoft.AspNetCore.Hosting

Namespace Tests.Diagnostics

    Public Class Startup

        Public Shared DefaultBuilder As IApplicationBuilder
        Public Builder As IApplicationBuilder = DefaultBuilder.UseDeveloperExceptionPage() ' Noncompliant

        Public Sub Configure(ByVal app As IApplicationBuilder, ByVal env As IHostingEnvironment)
            ' Invoking as extension methods
            If env.IsDevelopment() Then
                app.UseDeveloperExceptionPage() ' Compliant
                app.UseDatabaseErrorPage() ' Compliant
            End If

            ' Invoking as static methods
            If HostingEnvironmentExtensions.IsDevelopment(env) Then
                DeveloperExceptionPageExtensions.UseDeveloperExceptionPage(app) ' Compliant
                DatabaseErrorPageExtensions.UseDatabaseErrorPage(app) ' Compliant
            End If

            ' Not in development
            If Not env.IsDevelopment() Then
                DeveloperExceptionPageExtensions.UseDeveloperExceptionPage(app) ' Noncompliant
                DatabaseErrorPageExtensions.UseDatabaseErrorPage(app) ' Noncompliant
            End If

            ' Custom conditions are deliberately ignored
            Dim isDevelopment = env.IsDevelopment()
            If isDevelopment Then
                app.UseDeveloperExceptionPage() ' Noncompliant, False Positive
                app.UseDatabaseErrorPage() ' Noncompliant, False Positive
            End If

            ' Only simple IF checks are considered
            While env.IsDevelopment
                app.UseDeveloperExceptionPage   ' Noncompliant FP
                app.UseDatabaseErrorPage        ' Noncompliant FP
                Exit While
            End While

            ' These are called unconditionally
            app.UseDeveloperExceptionPage() ' Noncompliant
            app.UseDatabaseErrorPage() ' Noncompliant
            DeveloperExceptionPageExtensions.UseDeveloperExceptionPage(app) ' Noncompliant
            DatabaseErrorPageExtensions.UseDatabaseErrorPage(app) ' Noncompliant

            ' VB-specific syntax
            If env.IsDevelopment() Then app.UseDeveloperExceptionPage() ' Compliant
            If env.IsDevelopment() Then app.UseDatabaseErrorPage() ' Compliant
            If Not env.IsDevelopment() Then app.UseDeveloperExceptionPage() ' Noncompliant
            If Not env.IsDevelopment() Then app.UseDatabaseErrorPage() ' Noncompliant
        End Sub

        Public Shared Sub ConfigureNotProduction(ByVal app As IApplicationBuilder, ByVal env As IHostingEnvironment)
            ' https://sonarsource.atlassian.net/browse/NET-4818
            If Not env.IsProduction() Then
                app.UseDeveloperExceptionPage() ' Compliant
                app.UseDatabaseErrorPage() ' Compliant
                Dim configure As Action = Sub() app.UseDeveloperExceptionPage() ' Compliant
                configure()
            Else
                app.UseDeveloperExceptionPage() ' Noncompliant
            End If

            If Not HostingEnvironmentExtensions.IsProduction(env) Then
                DeveloperExceptionPageExtensions.UseDeveloperExceptionPage(app) ' Compliant
            End If

            If Not env.IsProduction() Then app.UseDeveloperExceptionPage() ' Compliant
            If Not env.IsProduction Then app.UseDatabaseErrorPage() ' Compliant

            If env.IsProduction() Then
                ' Production-only setup
            Else
                app.UseDeveloperExceptionPage() ' Compliant
            End If

            If env.IsProduction() Then
                app.UseExceptionHandler("/Error")
            ElseIf env.EnvironmentName = "Staging" Then
                app.UseDeveloperExceptionPage() ' Compliant
            End If

            If env.IsProduction() Then
                app.UseDeveloperExceptionPage() ' Noncompliant
            End If

            If env.IsProduction() Then app.UseExceptionHandler("/Error") Else app.UseDeveloperExceptionPage() ' Compliant
            If Not IsProduction(env) Then app.UseDeveloperExceptionPage() ' Noncompliant: unrelated method
        End Sub

        Public Shared Sub ConfigureElseIf(ByVal app As IApplicationBuilder, ByVal env As IHostingEnvironment)
            If env.EnvironmentName = "Staging" Then
                ' Staging-only setup
            ElseIf Not env.IsProduction() Then
                app.UseDeveloperExceptionPage() ' Compliant
            Else
                app.UseDeveloperExceptionPage() ' Noncompliant
            End If

            If env.EnvironmentName = "Staging" Then
                ' Staging-only setup
            ElseIf env.IsDevelopment() Then
                app.UseDeveloperExceptionPage() ' Compliant
            End If
        End Sub

        Private Shared Function IsProduction(env As IHostingEnvironment) As Boolean
            Return env.EnvironmentName = "Production"
        End Function

    End Class

End Namespace
