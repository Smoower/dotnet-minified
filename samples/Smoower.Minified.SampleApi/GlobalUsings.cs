// Import each Smoower.Minified package's extension namespace once, here.
global using Smoower.Minified.Core;
global using Smoower.Minified.AspNetCore;
global using Smoower.Minified.EFCore;
global using Smoower.Minified.Hosting;
global using Smoower.Minified.Logging;
global using Smoower.Minified.Validation;

// Aliases are not transitive across assemblies, so re-declare the set here.
// Open generics (Log<T>, AR<T>) cannot be aliased; closed Task<IActionResult> can.
global using Res = Microsoft.AspNetCore.Mvc.IActionResult;
global using AR = Microsoft.AspNetCore.Mvc.ActionResult;
global using CT = System.Threading.CancellationToken;
global using Cfg = Microsoft.Extensions.Configuration.IConfiguration;
global using Tr = System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.IActionResult>;

// Exception-name aliases: the long framework exception types are expensive on
// Claude's tokenizer (KeyNotFoundException +8, UnauthorizedAccessException +10).
// Aliasing the name is a pure rename - it never changes which catches you keep.
global using KNF = System.Collections.Generic.KeyNotFoundException;
global using IOE = System.InvalidOperationException;
global using UAE = System.UnauthorizedAccessException;
global using AE = System.ArgumentException;
global using JPN = System.Text.Json.Serialization.JsonPropertyNameAttribute;
global using JI = System.Text.Json.Serialization.JsonIgnoreAttribute;
global using JC = System.Text.Json.Serialization.JsonConverterAttribute;
global using JSEM = System.Text.Json.Serialization.JsonStringEnumMemberNameAttribute;
global using JPO = System.Text.Json.Serialization.JsonPropertyOrderAttribute;
global using Tbl = System.ComponentModel.DataAnnotations.Schema.TableAttribute;
global using NM = System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute;
global using Req = System.ComponentModel.DataAnnotations.RequiredAttribute;
global using MaxLen = System.ComponentModel.DataAnnotations.MaxLengthAttribute;
global using StrLen = System.ComponentModel.DataAnnotations.StringLengthAttribute;
global using Rng = System.ComponentModel.DataAnnotations.RangeAttribute;
