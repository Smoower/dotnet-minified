---
name: dotnet
description: Generate compact ASP.NET Core (MVC or Minimal API), EF Core, and xUnit test code using the Smoower.Minified helpers to cut output tokens (cost + generation time). Use whenever writing or editing .NET API controllers, minimal-API endpoints, EF Core queries, DI registration, logging, HttpClient, Redis, or tests in a project that references the Smoower.Minified.* packages.
---

# Smoower.Minified generation rules

Write the most compact valid C# using the `Smoower.Minified.*` helpers. The point
is fewer output tokens: lower API cost and faster generation — and it compounds on
every further agent step, since each turn re-reads and re-emits the code. Output
**code only** unless asked to explain.

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

A **Minimal API** project (`Smoower.Minified.MinimalApi`) also declares `R`
(`Microsoft.AspNetCore.Http.Results`) and `Ir` (`Task<IResult>`). A **test**
project imports `Smoower.Minified.Testing` and declares the attribute aliases
`F`/`Th`/`In`/`Mem` (Fact/Theory/InlineData/MemberData).

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

## Minimal APIs (alternative to controllers)

When the project uses Minimal APIs instead of MVC controllers, map endpoints with
the verb shorteners and group with `grp`; the same result-fusing terminators apply,
returning `IResult`.

| Use | Not |
| --- | --- |
| `app.g(r,h)` `app.po(r,h)` `app.pu(r,h)` `app.pa(r,h)` `app.dl(r,h)` | `MapGet` `MapPost` `MapPut` `MapPatch` `MapDelete` |
| `app.grp("/users")` | `MapGroup("/users")` |
| `.auth()` `.anon()` | `RequireAuthorization()` `AllowAnonymous()` |
| `R.BadRequest()` `R.NotFound()` `R.NoContent()` | `Results.BadRequest()` `Results.NotFound()` ... |
| `q.ok1()` `q.okl()` `set.okId(k)` `db.okNew(e)` `db.delById<T>(k)` → `IResult` | manual `Results.Ok(...)`/`Results.NotFound()` |
| async handler return `Ir` | `Task<IResult>` |

Pick one style per file — don't mix the controller and minimal-API terminators in
the same file (the names are identical; only the return type differs, `IActionResult`
vs `IResult`).

## Tests (xUnit + `Smoower.Minified.Testing`)

Tests are code too — write them compact. The assertions are fluent, **actual-first**,
and call xUnit's `Assert` underneath (same behavior); the value-returning ones chain
(`x.notNul().eq(expected)`).

| Use | Not |
| --- | --- |
| `[F]` `[Th]` `[In(...)]` `[Mem(...)]` | `[Fact]` `[Theory]` `[InlineData]` `[MemberData]` |
| `actual.eq(e)` `.neq(e)` `.eqSeq(e)` (collections) | `Assert.Equal` `Assert.NotEqual` |
| `x.tru()` `x.fls()` `x.nul()` `x.notNul()` | `Assert.True` `Assert.False` `Assert.Null` `Assert.NotNull` |
| `x.isType<T>()` `x.isAssignable<T>()` | `Assert.IsType` `Assert.IsAssignableFrom` |
| `a.same(e)` `a.notSame(e)` | `Assert.Same` `Assert.NotSame` |
| `xs.empty()` `xs.notEmpty()` `xs.sole()` `xs.len(n)` | `Assert.Empty` `Assert.NotEmpty` `Assert.Single` |
| `xs.contains(i)` `xs.has(x=>p)` `str.hasText(s)` | `Assert.Contains` (value / predicate / substring) |
| `x.inRange(lo,hi)` `act.throws<TEx>()` `act.throwsAsync<TEx>()` | `Assert.InRange` `Assert.Throws` `Assert.ThrowsAsync` |

## Never compact the contract

Route templates, HTTP verbs, status codes, and the **wire/DB values** (JSON
property names, column names) must stay exactly as the API/schema requires.
At **L1** keep the C# names too. At **L2/L3** you may shorten the C# identifier,
but only if you pin the unchanged wire/DB value in an attribute (`[JPN("realName")]`,
`[Col("RealColumn")]`) — the promise to clients and the database never changes,
only the in-code handle does. Never short-name an enum *type* via `global using`,
and never change a serialized enum *value* without `[JSEM("realValue")]`.

## Target shapes

Controller:

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

Same endpoints as a Minimal API (in `Program.cs`):

```csharp
var u=app.grp("/api/users");
u.g("/{id}",(int id,AppDb db)=>db.Users.nt().w(x=>x.Id==id).ok1());
u.g("",(AppDb db)=>db.Users.nt().okl());
u.po("",async(UserIn r,AppDb db)=>r.Name.nil()?R.BadRequest():await db.okNew(new User{Name=r.Name,Email=r.Email}));
u.dl("/{id}",(int id,AppDb db)=>db.delById<User>(id));
```

Test:

```csharp
public class UsersTests{
 [F]public async Task Get_404_WhenMissing()=>(await db.Users.w(x=>x.Id==9).ok1()).isType<NotFoundResult>();
 [Th][In("",false)][In("Ada",true)]public void Name_Validates(string n,bool ok)=>(!n.nil()).eq(ok);
}
```
