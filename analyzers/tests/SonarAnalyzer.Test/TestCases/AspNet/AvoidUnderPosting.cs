using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Basics
{
    public class ClassNotUsedInRequests
    {
        int ValueProperty { get; set; }                                             // Compliant
    }

    public class ModelUsedInController
    {
        public int ValueProperty { get; set; }                                      // Noncompliant {{Property used as input in a controller action should be nullable or annotated with the Required attribute to avoid under-posting.}}
//      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        public int? NullableValueProperty { get; set; }
        [Required] public int RequiredValueProperty { get; set; }
        [ValidateNever] public int NotValidatedValueProperty { get; set; }
        [Range(0, 10)] public int ValuePropertyWithRangeValidation { get; set; }    // Noncompliant
        [Required] public int? RequiredNullableValueProperty { get; set; }
        public int PropertyWithPrivateSetter { get; private set; }
        protected int ProtectedProperty { get; set; }
        internal int InternalProperty { get; set; }
        protected internal int ProtectedInternalProperty { get; set; }
        private int PrivateProperty { get; set; }
        public int ReadOnlyProperty => 42;
        public int field = 42;

        public (int, int) ImplicitTuple { get; set; }                               // Noncompliant
        public Tuple<int, int> TupleProperty { get; set; }
        public ValueTuple<int, int> ValueTupleProperty { get; set; }                // Noncompliant

        public string ReferenceProperty { get; set; }
    }

    public class DerivedFromController : Controller
    {
        [HttpPost]
        public IActionResult Create(ModelUsedInController model)
        {
            return View(model);
        }

        private void NotActionMethod(ClassNotUsedInRequests arg) { }
    }
}

namespace HttpVerbs
{
    public class SingleModel { public int Property { get; set; } }          // Noncompliant
    public class MultipleModel { public int Property { get; set; } }        // Noncompliant
    public class VerbModel { public int Property { get; set; } }            // Noncompliant
    public class MultipleVerbsModel { public int Property { get; set; } }   // Noncompliant

    [ApiController]
    [Route("api/[controller]")]
    public class DecoratedWithApiControlerAttribute : ControllerBase
    {
        [HttpGet]
        public int Single(SingleModel model) => 42;

        [HttpGet]
        [HttpPost]
        [HttpPut]
        [HttpDelete]
        public int Multiple(MultipleModel model) => 42;

        [AcceptVerbs("POST")]
        public int Verb(VerbModel model) => 42;

        [AcceptVerbs("GET", "POST", "PUT", "DELETE")]
        public int MultipleVerbs(MultipleVerbsModel model) => 42;
    }
}

namespace ModelHierarchy
{
    public class Parent
    {
        public int ParentProperty { get; set; }         // Noncompliant
    }

    public class Child : Parent
    {
        public int ChildProperty { get; set; }          // Noncompliant
    }

    public class GrandChild : Child
    {
        public int GrandChildProperty { get; set; }     // Noncompliant
    }

    public class ControllerClass : Controller
    {
        [HttpPost] public IActionResult Create(GrandChild model) => View(model);
        [HttpPost] public IActionResult Update(Parent model) => View(model);
    }
}

namespace CompositeModels
{
    public class Model
    {
        public int Property { get; set; }               // Noncompliant
        public Model SameModel { get; set; }
        public AnotherModel AnotherModel { get; set; }
    }

    public class AnotherModel
    {
        public int AnotherProperty { get; set; }        // Noncompliant
        public Model Model { get; set; }
    }

    public class ControllerClass : Controller
    {
        [HttpPost] public IActionResult Create(Model model) => View(model);
    }
}

namespace GenericCollections
{
    public class EnumerableItem { public int Property { get; set; } }           // Noncompliant
    public class ListItem { public int Property { get; set; } }                 // Noncompliant
    public class DictionaryKeyItem { public int Property { get; set; } }        // Noncompliant
    public class DictionaryValueItem { public int Property { get; set; } }      // Noncompliant

    public class ControllerClass : Controller
    {
        [HttpPost] public IActionResult CreateEnumerable(IEnumerable<EnumerableItem> model) => View(model);
        [HttpPost] public IActionResult CreateList(List<ListItem> model) => View(model);
        [HttpPost] public IActionResult CreateDictionary(Dictionary<DictionaryKeyItem, DictionaryValueItem> model) => View(model);
    }
}

namespace GenericModelWithTypeConstraint
{
    public class MyController : ControllerBase
    {
        public void Create(Model<Developer> model)
        {
        }
    }

    public class Model<T> where T : Person
    {
        public T Person { get; set; }
    }

    public abstract class Person
    {
        public int Age { get; set; }            // Noncompliant
    }

    public class Developer : Person
    {
        public string ProgramingLanguage { get; set; }
    }
}

namespace RecursiveTypeConstraint
{
    public class MyController : ControllerBase
    {
        public void Create(MyModel model)
        {
        }
    }

    public class Model<T> where T : Model<T>
    {
        public Model<T> SubModel { get; set; }
        public int ValueProperty { get; set; }  // Noncompliant
    }

    public class MyModel : Model<MyModel>
    {
    }
}
