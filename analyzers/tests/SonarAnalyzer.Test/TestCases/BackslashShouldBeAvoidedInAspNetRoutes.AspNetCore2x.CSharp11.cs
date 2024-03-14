using Microsoft.AspNetCore.Mvc;

// ToDo: Remark for the implementer: suitable for a parameterized test
public class WithAllTypesOfStrings : Controller
{
    private const string ASlash = "/";
    private const string ABackslash = @"\";
    private const string AConstStringIncludingABackslash = $"A{ABackslash}";
    private const string AConstStringNotIncludingABackslash = $"A{ASlash}";

    [Route(AConstStringIncludingABackslash)]    // Noncompliant
    public IActionResult WithConstStringIncludingABackslash() => View();

    [Route(AConstStringNotIncludingABackslash)] // Compliant
    public IActionResult WithConstStringNotIncludingABackslash() => View();

    [Route("\u002f[action]")]                   // Compliant: 2f is the Unicode code for '/'
    public IActionResult WithEscapeCodeOfSlash() => View();

    [Route("\u005c[action]")]                   // Noncompliant: 5c is the Unicode code for '\'
    public IActionResult WithEscapeCodeOfBackslash() => View();

    [Route($"A{ASlash}[action]")]               // Compliant
    public IActionResult WithInterpolatedString() => View();

    [Route($@"A{ABackslash}[action]")]          // Noncompliant
    public IActionResult WithInterpolatedVerbatimString() => View();

    [Route("""\[action]""")]                    // Noncompliant
    public IActionResult WithRawStringLiteralsTriple() => View();

    [Route(""""\[action]"""")]                  // Noncompliant
    public IActionResult WithRawStringLiteralsQuadruple() => View();

    [Route($$"""{{ABackslash}}/[action]""")]    // Noncompliant
    public IActionResult WithInterpolatedRawStringLiteralsIncludingABackslash() => View();

    [Route($$"""{{ASlash}}/[action]""")]        // Complaint
    public IActionResult WithInterpolatedRawStringLiteralsNotIncludingABackslash() => View();
}

// ToDo: Remark for the implementer: suitable for a parameterized test
public class WithHttpMethodAttributeAndAllTypesOfStrings : Controller
{
    private const string ASlash = "/";
    private const string ABackslash = @"\";
    private const string AConstStringIncludingABackslash = $"A{ABackslash}";
    private const string AConstStringNotIncludingABackslash = $"A{ASlash}";

    [HttpGet(AConstStringIncludingABackslash)]     // Noncompliant
    public IActionResult WithConstStringIncludingABackslash() => View();

    [HttpPost(AConstStringNotIncludingABackslash)] // Compliant
    public IActionResult WithConstStringNotIncludingABackslash() => View();

    [HttpPatch("\u002f[action]")]                  // Compliant: 2f is the Unicode code for '/'
    public IActionResult WithEscapeCodeOfSlash() => View();

    [HttpHead("\u005c[action]")]                   // Noncompliant: 5c is the Unicode code for '\'
    public IActionResult WithEscapeCodeOfBackslash() => View();

    [HttpDelete($"A{ASlash}[action]")]             // Compliant
    public IActionResult WithInterpolatedString() => View();

    [HttpOptions($@"A{ABackslash}[action]")]       // Noncompliant
    public IActionResult WithInterpolatedVerbatimString() => View();

    [HttpGet("""\[action]""")]                     // Noncompliant
    public IActionResult WithRawStringLiteralsTriple() => View();

    [HttpPost(""""\[action]"""")]                  // Noncompliant
    public IActionResult WithRawStringLiteralsQuadruple() => View();

    [HttpPatch($$"""{{ABackslash}}/[action]""")]   // Noncompliant
    public IActionResult WithInterpolatedRawStringLiteralsIncludingABackslash() => View();

    [HttpHead($$"""{{ASlash}}/[action]""")]        // Complaint
    public IActionResult WithInterpolatedRawStringLiteralsNotIncludingABackslash() => View();
}
