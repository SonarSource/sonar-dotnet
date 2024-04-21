using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace InitProperty
{
    public class ModelUsedInController
    {
        public int PropertyWithInit { get; init; }                                  // Noncompliant
    }

    public class DerivedFromController : Controller
    {
        [HttpPost]
        public IActionResult Create(ModelUsedInController model)
        {
            return View(model);
        }
    }
}
