---
name: smoower-minified
description: Generate ASP.NET Core / EF Core code using the Smoower.Minified compact syntax to cut output tokens (cost + generation time). Use whenever writing or editing .NET API controllers, EF Core queries, DI registration, logging, HttpClient, or Redis code in a project that references the Smoower.Minified.* packages.
---

# Smoower.Minified generation rules

Write the most compact valid C# using the `Smoower.Minified.*` helpers. The point
is fewer output tokens: lower API cost and faster generation. Output **code only**
unless asked to explain.

## Setup — ask the level first

Before generating, **ask the user which compaction level to apply** (skip only if
they already said, or CLAUDE.md / the repo pins one). Ask interactively, then apply
that level for the rest of the session:

1. **L1 — Aliases** (default): smoower short handles + `[Crud<>]`. Valid, readable
   C# as-is. Cuts framework ceremony (~18–35% on a controller). No renaming.
2. **L2 — Mapped**: L1 **plus** short-named domain vocabulary — shorten
   *high-frequency, multi-token* identifiers and pin the long name where it crosses
   a boundary: `[JPN("wire")]` for JSON, `[Col("Column")]` for DB, a `global using`
   for hot **types** (never enum types). Emit a `names.map` (CSV: `short,long,kind`)
   for internal names that have no attribute carrier. Reaches the business-logic
   floor; pays off as the codebase grows.
3. **L3 — Max**: L2 **plus** whitespace packed — every newline + indentation
   removed. The fully compressed form; readability comes from tooling only.

Also ask **comments: keep or strip** (L1 strips filler by default; L2/L3 strip all).
Only shorten an identifier whose short form actually saves Claude tokens — single
words like `Id`/`Title`/`Status` are already ~1 token; target compound names
(`RecurrenceDays`, `OrganizationId`, `CreatedAt`).

## Hard rules

- File-scoped namespaces, primary constructors, records for DTOs. Nullable on.
- No comments, no XML docs (`///`), no `#region`, no filler blank lines.
- Assume this `GlobalUsings.cs` exists (add it if missing):

```csharp
global using Smoower.Minified.Core;
global using Smoower.Minified.AspNetCore;
global using Smoower.Minified.EFCore;
global using Smoower.Minified.Hosting;
global using Smoower.Minified.Logging;
global using Res = Microsoft.AspNetCore.Mvc.IActionResult;
global using Tr  = System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.IActionResult>;
global using CT  = System.Threading.CancellationToken;
global using Cfg = Microsoft.Extensions.Configuration.IConfiguration;
```

`Ctl` is a base **class** from `Smoower.Minified.AspNetCore` (inherit it, not
`ControllerBase`). CLAUDE.md carries the full alias set to declare once per project:
exception aliases (`KNF`/`IOE`/`UAE`/`AE`), attribute aliases (`JPN`/`JI`/`JC`/`JSEM`/
`JPO`/`Tbl`/`NM`/`Req`/`MaxLen`/`StrLen`/`Rng`), and the controller result helpers on
`Ctl` (`nf`/`nc`/`un`/`forb`/`bad`/`unp`). For auth add `Smoower.Minified.Identity`.

## Use these instead of the long forms

| Use | Not |
| --- | --- |
| `[API]` `[RT(...)]` `[HG]` `[HPO]` `[HPU]` `[HPA]` `[HD]` | `[ApiController]` `[Route]` `[HttpGet]` ... |
| `[AUTH]` `[ANON]` `[FB]` `[FR]` `[FQ]` `[FH]` | `[Authorize]` `[AllowAnonymous]` `[FromBody]` ... |
| `:Ctl` | `: ControllerBase` |
| `public Tr X(...)` | `public async Task<IActionResult> X(...)` |
| `.w(...)` `.s(...)` `.ob(...)` `.nt()` `.inc(...)` | `.Where` `.Select` `.OrderBy` `.AsNoTracking` `.Include` |
| `.lst()` `.one()` `.single()` `.any()` `.cnt()` | `.ToListAsync` `.FirstOrDefaultAsync` ... |
| `db.save()` `db.add(e)` `db.upd(e)` `db.del(e)` `db.Set.id(key)` | `SaveChangesAsync` `FindAsync` ... |
| `q.ok1()` `q.okl()` `q.okc()` `set.okId(k)` `db.okAdd(e)` `db.okNew(e)` `db.delById<T>(k)` | manual `await ...` + `Ok(...)`/`NotFound()`/`CreatedAtAction(...)` |
| `value.created()` | `CreatedAtAction(nameof(Get), new { id = x.Id }, x)` |
| `[P200]` `[P201]` `[P404]` `[P200<UserDto>]` | `[ProducesResponseType(StatusCodes.Status200OK)]` ... |
| `MiniValidator<T>` + `req`/`rule` + `max`/`email`/`gt`/`lte` | `AbstractValidator<T>` + `RuleFor(...).NotEmpty().MaximumLength(...)` |
| `x.toJson()` `s.fromJson<T>()` | `JsonSerializer.Serialize/Deserialize` (or `JsonConvert`) |
| `nil()` `emp()` `none()` | `string.IsNullOrWhiteSpace` ... |
| `log.inf(...)` `log.wrn(...)` `log.err(...)` | `LogInformation` `LogWarning` ... |
| `svc.scoped<I,T>()` `svc.single<T>()` `svc.trans<T>()` | `AddScoped` `AddSingleton` `AddTransient` |
| `c.getJson<T>(url)` `c.postJson(url,b)` | `GetFromJsonAsync` `PostAsJsonAsync` |
| `db.get(k)` `db.set(k,v)` `db.getJson<T>(k)` | StackExchange.Redis `StringGetAsync` ... |
| `c.q<T>(sql,p)` `c.q1<T>(...)` `c.ex(sql,p)` `c.scalar<T>(...)` | Dapper `QueryAsync`/`QueryFirstOrDefaultAsync`/`ExecuteAsync` ... |
| `[JPN("n")]` `[JI]` `[JC(...)]` `[JSEM("n")]` `[Col("n")]` `[Tbl("n")]` `[Req]` `[MaxLen(n)]` `[NM]` | `[JsonPropertyName]` `[JsonIgnore]` `[Column]` `[Table]` `[Required]` `[MaxLength]` `[NotMapped]` ... |
| `um.create(u,pw)` `um.byEmail(e)` `um.checkPw(u,pw)` `um.roles(u)` `sm.pwSignIn(...)` | Identity `CreateAsync`/`FindByEmailAsync`/`CheckPasswordAsync`/`PasswordSignInAsync` ... |

Prefer the result-fusing terminators (`ok1`/`okl`/`okId`/`okAdd`/`delById`) so an
action is a single expression with no `async`/`await`/`return`/`Ok`/`NotFound`.
Keep `async` only when the expression still has an `await`.

Generics can't be aliased, so use `ILogger<T>` and `ActionResult<T>` directly.
Sync EF variants exist with an `S` suffix (`saveS`, `lstS`, `oneS`, ...) for code
that must be synchronous.

## Never compact the contract

Route templates, HTTP verbs, status codes, and the **wire/DB values** (JSON
property names, column names) must stay exactly as the API/schema requires.
At **L1** keep the C# names too. At **L2/L3** you may shorten the C# identifier,
but only if you pin the unchanged wire/DB value in an attribute (`[JPN("realName")]`,
`[Col("RealColumn")]`) — the promise to clients and the database never changes,
only the in-code handle does. Never short-name an enum *type* via `global using`,
and never change a serialized enum *value* without `[JSEM("realValue")]`.

## Target shape

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
