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

    End Class

End Namespace
