using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace IntentionalFindings
{
    public class S6967: Controller
    {
        [HttpPost] public IActionResult Create(Model model) => View(model); // Noncompliant

        public class Model
        {
            [Required] public int Id { get; set; }
        }
    }
}
