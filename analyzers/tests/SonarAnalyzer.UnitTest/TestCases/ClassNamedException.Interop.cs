using System.Reflection;
using System.Runtime.Serialization;
using System;

class UnamanagedException: System.Runtime.InteropServices._Exception // Compliant - allows access to System.Exception members from unmanaged code
{
    public string Message => "";
    public string StackTrace => "";
    public string HelpLink { get => ""; set => _ = value; }
    public string Source { get => ""; set => _ = value; }
    public Exception InnerException => null;
    public MethodBase TargetSite => null;
    public Exception GetBaseException() => null;

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
    }
}
