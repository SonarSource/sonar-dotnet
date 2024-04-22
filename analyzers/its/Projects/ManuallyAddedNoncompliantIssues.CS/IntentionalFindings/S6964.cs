using Microsoft.AspNetCore.Mvc;

namespace IntentionalFindings
{
    public class S6964
    {
        public class Model
        {
            public int ValueProperty { get; set; }  // Noncompliant
        }
    }
    
    public class ControllerClass : Controller
    {
        [HttpPost] public IActionResult Create(S6964.Model model) => View(model);
    }
}
