// ASP.NET Core MVC aliases. Using aliases are not transitive across assemblies,
// so consuming projects re-declare these in their own GlobalUsings.cs.
//
// LIMITATION: C# has no open-generic using aliases, so Log<T> and AR<T> cannot
// be aliased. Use ILogger<T> / ActionResult<T> directly. The CLOSED generic
// Task<IActionResult> CAN be aliased, which is what `Tr` exploits.

global using Res = Microsoft.AspNetCore.Mvc.IActionResult;
global using AR = Microsoft.AspNetCore.Mvc.ActionResult;
global using Tr = System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.IActionResult>;
