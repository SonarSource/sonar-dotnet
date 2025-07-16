using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;

namespace Basics
{
    public class ClassNotUsedInRequests
    {
        int ValueProperty { get; set; }                                             // Compliant
    }

    public class ModelUsedInController
    {
        public int ValueProperty { get; set; }                                      // Noncompliant {{Value type property used as input in a controller action should be nullable, required or annotated with the JsonRequiredAttribute to avoid under-posting.}}
//                 ^^^^^^^^^^^^^
        public int? NullableValueProperty { get; set; }
        [Required] public int RequiredValueProperty { get; set; }                   // Noncompliant, RequiredAttribute has no effect on value types
        [Range(0, 10)] public int ValuePropertyWithRangeValidation { get; set; }    // Compliant
        [Required] public int? RequiredNullableValueProperty { get; set; }
        [JsonProperty(Required = Required.Always)] public int JsonRequiredValuePropertyAlways { get; set; }              // Compliant
        [JsonProperty(Required = Required.AllowNull)] public int JsonRequiredValuePropertyAllowNull { get; set; }        // Compliant
        [JsonProperty(Required = Required.DisallowNull)] public int JsonRequiredValuePropertyDisallowNull { get; set; }  // Noncompliant
        [JsonProperty] public int JsonRequiredValuePropertyDefault { get; set; }                                         // Noncompliant
        [Newtonsoft.Json.JsonIgnore] public int JsonIgnoredProperty { get; set; }                                        // Compliant
        [Newtonsoft.Json.JsonRequired] public int JsonRequiredNewtonsoftValueProperty { get; set; }                      // Compliant
        [System.Text.Json.Serialization.JsonRequired] public int JsonRequiredValueProperty { get; set; }                 // Compliant
        [System.Text.Json.Serialization.JsonIgnore] public int JsonIgnoreValueProperty { get; set; }                     // Compliant
        [JsonProperty(Required = Required.AllowNull)] [FromQuery] public int PropertyWithMultipleAttributesCompliant { get; set; }   // Compliant
        [Required] [FromQuery] public int PropertyWithMultipleAttributesNonCompliant { get; set; }   // Noncompliant
        public int PropertyWithPrivateSetter { get; private set; }
        protected int ProtectedProperty { get; set; }
        internal int InternalProperty { get; set; }
        protected internal int ProtectedInternalProperty { get; set; }
        private int PrivateProperty { get; set; }
        public int PropertyWithDefaultValue { get; set; } = 42;
        public int ReadOnlyProperty => 42;
        public int field = 42;

        public (int, int) ImplicitTuple { get; set; }                               // Compliant - tuple types are not supported in model binding
        public Tuple<int, int> TupleProperty { get; set; }
        public ValueTuple<int, int> ValueTupleProperty { get; set; }

        public string ReferenceProperty { get; set; }
        public dynamic DynamicProprty { get; set; }
    }

    public class NoDefaultConstructor
    {
        public int ValueProperty { get; set; }                                      // Compliant - non-record types cannot be used in model binding without having a default constructor

        public NoDefaultConstructor(int arg)
        {
        }
    }

    public class ExplicitDefaultConstructor
    {
        public int ValueProperty { get; set; }                                      // Noncompliant

        public ExplicitDefaultConstructor()
        {
        }

        public ExplicitDefaultConstructor(int arg)
        {
        }
    }

    public class DerivedFromController : Controller
    {
        [HttpPost] public IActionResult Create(ModelUsedInController model) => View(model);
        [HttpPut] public IActionResult Update(NoDefaultConstructor model) => View(model);
        [HttpDelete] public IActionResult Remove(ExplicitDefaultConstructor model) => View(model);
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

namespace Collections
{
    public class ArrayItem { public int Property { get; set; } }                // Noncompliant
    public class NestedArrayItem { public int Property { get; set; } }          // Noncompliant
    public class EnumerableItem { public int Property { get; set; } }           // Noncompliant
    public class ListItem { public int Property { get; set; } }                 // Noncompliant
    public class DictionaryKeyItem { public int Property { get; set; } }        // Noncompliant
    public class DictionaryValueItem { public int Property { get; set; } }      // Noncompliant
    public class NestedCollectionItem { public int Property { get; set; } }     // Noncompliant

    public class ControllerClass : Controller
    {
        [HttpPost] public IActionResult CreateArray(ArrayItem[] model) => View(model);
        [HttpPost] public IActionResult CreateNestedArray(NestedArrayItem[] model) => View(model);
        [HttpPost] public IActionResult CreateEnumerable(IEnumerable<EnumerableItem> model) => View(model);
        [HttpPost] public IActionResult CreateList(List<ListItem> model) => View(model);
        [HttpPost] public IActionResult CreateDictionary(Dictionary<DictionaryKeyItem, DictionaryValueItem> model) => View(model);
        [HttpPost] public IActionResult CreateNestedCollection(Dictionary<string, IEnumerable<NestedCollectionItem[]>> model) => View(model);
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
        public int Age { get; set; }                    // Noncompliant
    }

    public class Developer : Person
    {
        public string ProgramingLanguage { get; set; }
        public int WorkExperienceInYears { get; set; }  // Noncompliant
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
        public int ValueProperty { get; set; }          // Noncompliant
    }

    public class MyModel : Model<MyModel>
    {
    }
}

namespace ValidateNeverOnCustomType
{
    public class NotVisited
    {
        public int Prop { get; set; }                                           // Compliant
    }

    public class Model
    {
        [ValidateNever] public NotVisited NotVisited { get; set; }
        [ValidateNever] public int NotValidatedValueProperty { get; set; }      // Compliant
    }

    [ValidateNever]
    public class NeverValidatedModel
    {
        public int ValueProperty { get; set; }                                  // Compliant
    }

    public class RegularModel
    {
        public int ValueProperty { get; set; }                                  // Compliant
    }

    public class CustomController : Controller
    {
        [HttpGet] public IActionResult Get(NeverValidatedModel model) => View(model);
        [HttpPost] public IActionResult Post(Model model) => View(model);
        [HttpPut] public IActionResult Put([ValidateNever] RegularModel model) => View(model);
    }
}

namespace MutlipleModelsInSameAction
{
    public class Model1
    {
        public int ValueProperty { get; set; }                                  // Noncompliant
    }

    public class Model2
    {
        public int ValueProperty { get; set; }                                  // Noncompliant
    }

    public class CustomController : Controller
    {
        [HttpPost] public IActionResult Post(Model1 model1, int other, Model2 model2) => View(model1);
    }
}

namespace Interfaces
{
    public interface IPerson
    {
        int Age { get; set; }                                                   // Noncompliant
    }

    public class Model
    {
        public IPerson Person { get; set; }
    }

    public class CustomController : Controller
    {
        [HttpPost] public IActionResult Post(Model model) => View(model);
    }
}

namespace GeneralTypes
{
    public class CustomController : Controller
    {
        [HttpPost] public IActionResult PostObject(object model) => View(model);
        [HttpPost] public IActionResult PostString(string model) => View(model);
        [HttpPost] public IActionResult PostDynamic(dynamic model) => View(model);
    }
}

namespace ModelIsAutogenerated
{
    public class HasAutogeneratedModelController : Controller
    {
        [HttpGet]
        public int Single(AutogeneratedModel model) => 42;
    }
}

public partial class AutogeneratedModel
{
    public bool IsProperty // Noncompliant
    {
        get;
        set;
    }
}

namespace UsingBindNeverAttribute
{
    public class ModelWithBindNeverProperty
    {
        [BindNever] public int ValueProperty { get; set; }
    }

    [BindNever]
    public class EntireModelWithBindNeverAttribute
    {
        public int ValueProperty { get; set; }
    }

    public class CustomController : Controller
    {
        [HttpGet] public IActionResult Get(ModelWithBindNeverProperty model) => View(model);
        [HttpPost] public IActionResult Post(EntireModelWithBindNeverAttribute model) => View(model);
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/9690
namespace Repro_GH9690
{
    public class DataModel
    {
        [BindRequired]
        public int PasswordMinLength { get; set; } // Compliant
    }

    public class DataModelController : Controller
    {
        [HttpPost]
        public IActionResult Post([FromBody] DataModel model) => View(model);
    }
}
