using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

[Route("/route/{BoolParam:bool}")] // Secondary [bool] {{This route parameter has a 'bool' type constraint.}}
//             ^^^^^^^^^^^^^^^^
[Route("/route/{DatetimeParam:datetime}")] // Secondary [datetime] {{This route parameter has a 'datetime' type constraint.}}
//             ^^^^^^^^^^^^^^^^^^^^^^^^
[Route("/route/{DatetimeParam:datetime}/{BoolParam:bool}")] // Secondary ^16#24 [datetime-multiple] {{This route parameter has a 'datetime' type constraint.}}
                                                            // Secondary@-1 ^41#16 [bool-multiple] {{This route parameter has a 'bool' type constraint.}}
[Route("/route/{CompliantBoolParam:bool}")]
[Route("/route/{CompliantDatetimeParam:datetime}")]
public class ParameterTypeShouldMatchRouteTypeConstraint : ComponentBase
{
    [Parameter]
    public string BoolParam { get; set; } // Noncompliant [bool, bool-multiple] {{Parameter type 'string' does not match route parameter type constraint.}}
    [Parameter]
    public decimal DatetimeParam { get; set; } // Noncompliant [datetime, datetime-multiple] {{Parameter type 'decimal' does not match route parameter type constraint.}}
    [Parameter]
    public bool CompliantBoolParam { get; set; } // Compliant
    [Parameter]
    public DateTime CompliantDatetimeParam { get; set; } // Compliant
    [Parameter]
    public DateTime NoncompliantFromConstant { get; set; } // FN
    [Parameter]
    public bool CompliantFromConstant { get; set; } // Compliant
}

[Route("/route/{FullyQualifiedParam:bool}")] // Secondary [fully-qualified] {{This route parameter has a 'bool' type constraint.}}
[Route("/route/{ListParam}")] // Secondary [list] {{This route parameter has an implicit 'string' type constraint.}}
[Route("/route/{DictionaryParam}")] // Secondary [dictionary] {{This route parameter has an implicit 'string' type constraint.}}
[Route("/route/{PointerParam:int}")] // Secondary [pointer] {{This route parameter has a 'int' type constraint.}}
[Route("/route/{DoublePointerParam:int}")] // Secondary [double-pointer] {{This route parameter has a 'int' type constraint.}}
[Route("/route/{NullableInt:int}")]
[Route("/route/{NonNullableInt:int}")]
[Route("/route/{NullableInt:int?}")]
[Route("/route/{NonNullableInt:int?}")]
[Route("/route/{ArrayInt:int}")] // Secondary [array] {{This route parameter has a 'int' type constraint.}}
[Route("/route/{ArrayInt:int?}")] // Secondary [array-optional] {{This route parameter has a 'int' type constraint.}}
public class ParameterTypeShouldMatchRouteTypeConstraint_EdgeCase : ComponentBase
{
    [Parameter]
    public System.String FullyQualifiedParam { get; set; } // Noncompliant [fully-qualified] {{Parameter type 'String' does not match route parameter type constraint.}}

    [Parameter]
    public IList<string> ListParam { get; set; } // Noncompliant [list] {{Parameter type 'IList' does not match route parameter type constraint.}}

    [Parameter]
    public IDictionary<string, string> DictionaryParam { get; set; } // Noncompliant [dictionary] {{Parameter type 'IDictionary' does not match route parameter type constraint.}}

    [Parameter]
    unsafe public int* PointerParam { get; set; } // Noncompliant [pointer] {{Parameter type 'int*' does not match route parameter type constraint.}}

    [Parameter]
    unsafe public int** DoublePointerParam { get; set; } // Noncompliant [double-pointer] {{Parameter type 'int**' does not match route parameter type constraint.}}

    [Parameter]
    public int? NullableInt { get; set; }

    [Parameter]
    public int NonNullableInt { get; set; }

    [Parameter]
    public int[] ArrayInt { get; set; } // Noncompliant [array, array-optional] {{Parameter type 'int[]' does not match route parameter type constraint.}}
}

public static class Constants
{
    public const string NONCOMPLIANT_ROUTE = "/route/{NoncompliantFromConstant:bool}";
    public const string COMPLIANT_ROUTE = "/route/{CompliantFromConstant:bool}";
    public const string BoolConstraint = "bool";
    public const string IntConstraint = "int";
}

[Route(Constants.COMPLIANT_ROUTE)]
[Route(Constants.NONCOMPLIANT_ROUTE)] // Secondary [constant]
//     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
[Route(template: "/route/{UseTemplateParam:bool}")]
[Route(template: "/route/{UseTemplateParam:int}")] // Secondary [template-int]
//                       ^^^^^^^^^^^^^^^^^^^^^^
[Route((("/route/{ParenthesisParam:bool}")))]
[Route((("/route/{ParenthesisParam:int}")))] // Secondary [parenthesis-int]
//               ^^^^^^^^^^^^^^^^^^^^^^
[Route("""/route/{RawStringLiteralParam:bool}""")]
[Route("""/route/{RawStringLiteralParam:int}""")] // Secondary [raw-int]
//               ^^^^^^^^^^^^^^^^^^^^^^^^^^^
[Route($$"""/route/{RawInterpolatedParam:{{Constants.BoolConstraint}}}""")]
[Route($$"""/route/{RawInterpolatedParam:{{Constants.IntConstraint}}}""")] // Secondary [raw-interpolated-int]
//     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
[Route("/something" + "/{ConcatenationParam:bool}")]
[Route("/something" + "/{ConcatenationParam:int}")] // Secondary [concatenation-int]
//                      ^^^^^^^^^^^^^^^^^^^^^^^^
public class ParameterTypeShouldMatchRouteTypeConstraint_Constant : ComponentBase
{
    [Parameter]
    public DateTime NoncompliantFromConstant { get; set; } // Noncompliant [constant]
    //     ^^^^^^^^
    [Parameter]
    public bool CompliantFromConstant { get; set; } // Compliant
    [Parameter]
    public bool UseTemplateParam { get; set; } // Noncompliant [template-int]
    //     ^^^^
    [Parameter]
    public bool ParenthesisParam { get; set; } // Noncompliant [parenthesis-int]
    [Parameter]
    public bool RawStringLiteralParam { get; set; } // Noncompliant [raw-int]
    [Parameter]
    public bool RawInterpolatedParam { get; set; } // Noncompliant [raw-interpolated-int]
    [Parameter]
    public bool ConcatenationParam { get; set; } // Noncompliant [concatenation-int]
}
