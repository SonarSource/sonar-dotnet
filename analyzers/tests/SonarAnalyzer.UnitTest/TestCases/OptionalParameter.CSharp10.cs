using System.Runtime.CompilerServices;

namespace Tests.Diagnostics
{
    public class CallerMember
    {
        public void Caller_ArgumentExpression(bool condition, [CallerArgumentExpression("condition")] string message = null) { }
    }
}
