using System;
using System.Web.Services;
using System.Web.Services.Protocols;

// https://github.com/SonarSource/sonar-dotnet/issues/9017
public class MyWebService : WebService
{
    [SoapDocumentMethod(Action = "http://www.contoso.com/GetUserName")]
    public string GetUserName() {
        return User.Identity.Name;
    }

    [SoapDocumentMethod("http://www.contoso.com/GetUserName")]
    public void ConstructorArgument() { }

    [SoapDocumentMethod(action: "http://www.contoso.com/GetUserName")]
    public void NamedConstructorArgument() { }

    [SoapDocumentMethod(Action = ("http://www.contoso.com/GetUserName"))]
    public void ParenthesizedAction() { }

    [SoapDocumentMethod(Action = "http://www.contoso.com/" + nameof(ConcatenatedAction))]
    public void ConcatenatedAction() { }

    [SoapDocumentMethod("http://www.contoso.com/" + ("Service/" + nameof(ConcatenatedConstructorArgument)))]
    public void ConcatenatedConstructorArgument() { }

    private const string ActionUrl = "http://www.contoso.com/GetUserName"; // Noncompliant - FP, the value is not tracked to the attribute

    [SoapDocumentMethod(Action = ActionUrl)]
    public void ConstantAction() { }

    [SoapRpcMethod(Action = "http://www.contoso.com/GetUserName")]
    public void RpcAction() { }

    [SoapRpcMethod("http://www.contoso.com/GetUserName")]
    public void RpcConstructorArgument() { }

    [SoapDocumentMethod(Action = "http://www.contoso.com/GetUserName",
        RequestElementName = "http://www.contoso.com/GetUserName")] // Noncompliant
    public void UnrelatedProperty() { }

    [SoapDocumentMethod(Action = "ftp://user@www.contoso.com/GetUserName")] // Noncompliant
    public void OtherProtocol() { }
}

namespace Custom
{
    public class SoapDocumentMethodAttribute : Attribute
    {
        public SoapDocumentMethodAttribute() { }
        public SoapDocumentMethodAttribute(string action) { }
        public string Action { get; set; }
    }

    public class Service
    {
        [SoapDocumentMethod(Action = "http://www.contoso.com/GetUserName")] // Noncompliant
        public void CustomAttributeProperty() { }

        [SoapDocumentMethod("http://www.contoso.com/GetUserName")] // Noncompliant
        public void CustomAttributeConstructor() { }
    }
}
