using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CSharp12
{
    public class ModelWithPrimaryConstructor(int vp, int rvp, int nvp)
    {
        public int ValueProperty { get; set; } = vp;                        // Compliant - no parameterless constructor, type cannot be used for Model Binding
    }

    public class ModelWithPrimaryAndParameterlessConstructor(int vp, int rvp, int nvp)
    {
        public ModelWithPrimaryAndParameterlessConstructor() : this(0, 0, 0) { }
        public int ValueProperty { get; set; } = vp;                        // Noncompliant
    }

    public class DerivedFromController : Controller
    {
        [HttpPost] public IActionResult Create(ModelWithPrimaryConstructor model) => View(model);
        [HttpDelete] public IActionResult Remove(ModelWithPrimaryAndParameterlessConstructor model) => View(model);
    }
}
