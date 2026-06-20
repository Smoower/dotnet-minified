# CLAUDE.md — generation rules for Smoower.Minified

These rules are **strict**. When generating .NET code in this repository (or any
repo that references the `Smoower.Minified.*` packages), follow them exactly.

## Output

- Output **code only**. Do not explain unless explicitly asked.
- **No comments. No XML docs (`///`). No `#region`. No unnecessary blank lines.**
- **File-scoped namespaces**, **primary constructors**, **records** for DTOs.
- Nullable reference types + implicit usings on; latest C# language version.

## Imports (declare once per project, in GlobalUsings.cs)

```csharp
global using Smoower.Minified.Core;
global using Smoower.Minified.AspNetCore;
global using Smoower.Minified.EFCore;
global using Smoower.Minified.Hosting;
global using Smoower.Minified.Logging;
global using Res = Microsoft.AspNetCore.Mvc.IActionResult;
global using AR  = Microsoft.AspNetCore.Mvc.ActionResult;
global using CT  = System.Threading.CancellationToken;
global using Cfg = Microsoft.Extensions.Configuration.IConfiguration;
global using Tr  = System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.IActionResult>;
global using KNF = System.Collections.Generic.KeyNotFoundException;
global using IOE = System.InvalidOperationException;
global using UAE = System.UnauthorizedAccessException;
global using AE  = System.ArgumentException;
global using JPN = System.Text.Json.Serialization.JsonPropertyNameAttribute;
global using JI   = System.Text.Json.Serialization.JsonIgnoreAttribute;
global using JC   = System.Text.Json.Serialization.JsonConverterAttribute;
global using JSEM = System.Text.Json.Serialization.JsonStringEnumMemberNameAttribute;
global using JPO  = System.Text.Json.Serialization.JsonPropertyOrderAttribute;
global using Tbl    = System.ComponentModel.DataAnnotations.Schema.TableAttribute;
global using NM     = System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute;
global using Req    = System.ComponentModel.DataAnnotations.RequiredAttribute;
global using MaxLen = System.ComponentModel.DataAnnotations.MaxLengthAttribute;
global using StrLen = System.ComponentModel.DataAnnotations.StringLengthAttribute;
global using Rng    = System.ComponentModel.DataAnnotations.RangeAttribute;
```

`[JPN("wireName")]` pins the JSON name (it's a `using` alias because
`JsonPropertyNameAttribute` is sealed). `[Col("ColumnName")]` (shipped in
`Smoower.Minified.EFCore`) pins the DB column. Both let a short C# identifier
carry a long wire/DB contract: the long name is paid **once** at the declaration,
the short name everywhere it's referenced — a net win for any identifier used
more than a couple of times. Reserve `global using` type-aliases for hot
entity/DTO types; **never `global using` an enum type** (it obscures switches and
`nameof` — bad code). Enum *values* may be shortened via `[JsonStringEnumMemberName]`
/ `[EnumMember]`, but since the enum type name can't be aliased it dominates the
reference, so value-only shortening rarely pays — skip it unless measured.

The exception aliases (`KNF`/`IOE`/`UAE`/`AE`) shorten the long framework
exception names in `catch`/`throw`. They change names only — keep every
`try`/`catch` whose handler does real work; only the names get shorter.

## Use the compact syntax

- Attributes: `[API]`, `[RT(...)]`, `[HG]`, `[HPO]`, `[HPU]`, `[HPA]`, `[HD]`,
  `[AUTH]`, `[ANON]`, `[FB]`, `[FR]`, `[FQ]`, `[FH]`.
- Types: `Ctl` (the smoower controller base class, from `Smoower.Minified.AspNetCore`
  — inherit it, not `ControllerBase`), `Tr` (async action return), `Res`/`AR` where
  they fit; use `ActionResult<T>` / `ILogger<T>` for generics (no open-generic aliases).
- Controller result helpers (on `Ctl`, for the hand-written guard/catch returns the
  terminators don't cover): `nf()` (404), `nc()` (204), `un()` (401), `forb()` (403),
  `bad()`/`bad(err)` (400), `unp(msg)` (422 `{error:msg}`). Prefer the result-fusing
  terminators below for the data path; use these for guards.
- EF query: `w`, `s`, `ob`, `obd`, `tb`, `tbd`, `sk`, `tk`, `nt`, `inc`, `lst`,
  `one`, `single`, `any`, `cnt`.
- EF write: `db.save()`, `db.Set.id(key)`, `db.add`/`db.upd`/`db.del`. Sync code
  uses the `S`-suffixed variants (`saveS`, `idS`, `addS`, `lstS`, `oneS`, ...);
  async is the default because that's where the token savings are.
- Result-fusing terminators (prefer these — they make actions single
  expressions): `q.ok1()` (200/404), `q.okl()` (200 list), `q.okc()` (200 count),
  `set.okId(key)` (200/404), `db.okAdd(e)` (200), `db.okNew(e)` (add+save, 201),
  `value.created()` (201), `db.delById<T>(key)` (204/404).
  Keep `async` only when the expression still contains an `await`.
- Swagger: `[P200]`/`[P201]`/`[P204]`/`[P400]`/`[P404]`/... and generic
  `[P200<UserDto>]` instead of `[ProducesResponseType(...)]`. Stack them.
- Guards: `nil()`, `emp()`, `none()`.
- HttpClient: `getJson<T>`, `postJson`, `putJson`, `patchJson`, `del`.
- Redis (`IDatabase`): `get`, `set`, `del`, `incr`, `getJson<T>`, `setJson<T>`.
- Logging (`ILogger`): `inf`, `wrn`, `err`, `dbg`.
- DI (`IServiceCollection`): `scoped`, `single`, `trans`.
- Validation: inherit `MiniValidator<T>` (not `AbstractValidator<T>`); use
  `req(x=>x.P)` / `rule(x=>x.P)` then chain `max`/`min`/`len`/`email`/`gt`/`lt`/`lte`/`rng`.
- JSON: `x.toJson()` / `s.fromJson<T>()` (System.Text.Json, or the Newtonsoft package).
- Dapper (`IDbConnection`): `q<T>`, `q1<T>`, `qs<T>`, `ex`, `scalar<T>`.

## Forbidden tokens

Never emit these long-form tokens in generated code:

- `HttpGet`, `HttpPost`, `HttpPut`, `HttpPatch`, `HttpDelete`
- `Route(`, `ApiController`, `ControllerBase`
- `IActionResult`, `ActionResult` (non-generic — use `Res`/`Tr`; generics use `ActionResult<T>`)
- `.Where(`, `.Select(`, `.ToListAsync(`, `.FirstOrDefaultAsync(`, `.SaveChangesAsync(`
- `AddScoped(` / `AddSingleton(` / `AddTransient(` (use `scoped`/`single`/`trans`)
- XML comments (`///`), `#region` / `#endregion`

A coarse checker in `tests/Smoower.Minified.Tests/ForbiddenTokenCheckerTests.cs`
scans the sample for most of these.

## Never compact the contract

Keep route templates, HTTP verbs, status codes, and DTO property/JSON names
**stable**. The compact style changes how code is written, never its behavior.

## Reference shape

```csharp
[API,RT("api/users")]
public class UsersController(AppDb db,ILogger<UsersController> log):Ctl{
 [HG("{id}")]public Tr Get(int id)=>db.Users.nt().w(x=>x.Id==id).s(x=>new{x.Id,x.Name,x.Email}).ok1();
 [HG]public Tr All()=>db.Users.nt().s(x=>new{x.Id,x.Name,x.Email}).okl();
 [HPO]public async Tr Post(UserIn r){
  if(r.Name.nil())return BadRequest();
  var x=await db.add(new User{Name=r.Name,Email=r.Email});
  log.inf("created {Id}",x.Id);
  return Ok(new{x.Id,x.Name,x.Email});
 }
 [HD("{id}")]public Tr Del(int id)=>db.delById<User>(id);
}
public record UserIn(string Name,string Email);
```
