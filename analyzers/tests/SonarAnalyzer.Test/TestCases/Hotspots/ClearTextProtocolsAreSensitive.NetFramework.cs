using System.Web.Services;
using System.Web.Services.Protocols;

// https://github.com/SonarSource/sonar-dotnet/issues/9017
public class MyWebService : WebService
{
    [SoapDocumentMethod(Action = "http://www.contoso.com/GetUserName")] // Noncompliant - FP
    public string GetUserName() {
        return User.Identity.Name;
    }
}
