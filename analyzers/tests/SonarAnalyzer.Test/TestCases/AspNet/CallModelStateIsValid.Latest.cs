using Microsoft.AspNetCore.Mvc;

namespace Repro_NET1044
{
    public class PositionalPatternClause : Controller
    {
        public int Pattern((int X, int Y) tuple) => // Compliant
            tuple switch
            {
                (_, _) => 42     // This was throwing NullReferenceException
            };
    }
}
