using System;
using System.Threading;
using Microsoft.AspNetCore.Http;

[Obsolete(nameof(httpContext.User))] // Compliant, because HttpContext class was moved from System.Web to Microsoft.AspNetCore.Http and we do not track it
void HttpContextUser(HttpContext httpContext)
{
    var user = httpContext.User; // Compliant, FN, because HttpContext class was moved from System.Web to Microsoft.AspNetCore.Http and we do not track it
}

[Obsolete(nameof(Thread.CurrentPrincipal))] // Noncompliant, FP, this just gets resolved to "CurrentPrincipal"
void ThreadCurrentPrincipal()
{
}
