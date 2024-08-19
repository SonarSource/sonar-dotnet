using Microsoft.AspNetCore.Mvc;

namespace AspNetCore8.Controllers;

[Route(@"A\[controller]")]    // Noncompliant (S6930)
//     ^^^^^^^^^^^^^^^^^
public class S6930Controller : Controller
{
    [Route(@"A\[action]")]    // Noncompliant (S6930)
    //     ^^^^^^^^^^^^^
    public IActionResult Index() => View();
}
