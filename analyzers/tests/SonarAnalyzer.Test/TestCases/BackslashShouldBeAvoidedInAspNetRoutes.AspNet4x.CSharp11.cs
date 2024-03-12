using System.Web.Mvc;

// ToDo: Remark for the implementer: suitable for a parameterized test
public class WithAllTypesOfStringsController : Controller
{
    private const string ASlash = "/";
    private const string ABackSlash = @"\";
    private const string AConstStringIncludingABackslash = $"A{ABackSlash}";
    private const string AConstStringNotIncludingABackslash = $"A{ASlash}";

    [Route(AConstStringIncludingABackslash)]    // Noncompliant
    public ActionResult WithConstStringIncludingABackslash() => View();

    [Route(AConstStringNotIncludingABackslash)] // Compliant
    public ActionResult WithConstStringNotIncludingABackslash() => View();

    [Route("\u002f[action]")]                   // Compliant: 2f is the Unicode code for '/'
    public ActionResult WithEscapeCodeOfSlash() => View();

    [Route("\u005c[action]")]                   // Noncompliant: 5c is the Unicode code for '\'
    public ActionResult WithEscapeCodeOfBackslash() => View();

    [Route($"A{ASlash}[action]")]               // Compliant
    public ActionResult WithInterpolatedString() => View();

    [Route($@"A{ABackSlash}[action]")]          // Noncompliant
    public ActionResult WithInterpolatedVerbatimString() => View();

    [Route("""\[action]""")]                    // Noncompliant
    public ActionResult WithRawStringLiteralsTriple() => View();

    [Route(""""\[action]"""")]                  // Noncompliant
    public ActionResult WithRawStringLiteralsQuadruple() => View();

    [Route($$"""{{ABackSlash}}/[action]""")]    // Noncompliant
    public ActionResult WithInterpolatedRawStringLiteralsIncludingABackslash() => View();

    [Route($$"""{{ASlash}}/[action]""")]        // Complaint
    public ActionResult WithInterpolatedRawStringLiteralsNotIncludingABackslash() => View();
}
