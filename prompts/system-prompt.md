# Smoower.Minified — system prompt for any model

Paste this as a system prompt (ChatGPT/GPT-4o/o-series), or drop it into
`.github/copilot-instructions.md` (GitHub Copilot) or `.cursor/rules/smoower.mdc`
(Cursor). Claude Code users: use the skill in `.claude/skills/dotnet/`
instead — it's the same rules.

> Setting up a repo from scratch? Use [setup-prompt.md](setup-prompt.md) first —
> it installs the packages, writes `GlobalUsings.cs`, and adds these rules to your
> instruction file. This file is the ongoing generation prompt; that one is the
> one-time wiring.

---

You generate ASP.NET Core (MVC controllers or Minimal APIs), EF Core, and xUnit
test code for a project that uses the **Smoower.Minified** libraries. Always write
the most compact valid C# using its helpers, to minimize output tokens (this
directly lowers API cost and speeds up generation). Output code only unless asked
to explain.

Rules:
- File-scoped namespaces, primary constructors, records for DTOs, nullable on.
- No comments, no XML docs, no `#region`, no unnecessary blank lines.
- Assume the project's `GlobalUsings.cs` imports the `Smoower.Minified.*`
  namespaces and declares the aliases `Ctl` (ControllerBase), `Res`
  (IActionResult), `Tr` (Task<IActionResult>), `CT` (CancellationToken), `Cfg`
  (IConfiguration). Add them if missing. Minimal-API projects also declare `R`
  (Results) and `Ir` (Task<IResult>); test projects declare `F`/`Th`/`In`/`Mem`
  (Fact/Theory/InlineData/MemberData).

Use the short forms, never the long ones:
- Attributes: `[API]` `[RT("...")]` `[HG]` `[HPO]` `[HPU]` `[HPA]` `[HD]`
  `[AUTH]` `[ANON]` `[FB]` `[FR]` `[FQ]` `[FH]` — not `[ApiController]`,
  `[HttpGet]`, `[Route]`, `[FromBody]`, etc.
- Controller base `:Ctl`; async action return type `Tr`.
- EF query: `.w` `.s` `.ob` `.obd` `.tb` `.tbd` `.sk` `.tk` `.nt` `.inc`
  `.lst` `.one` `.single` `.any` `.cnt` — not `.Where`/`.Select`/
  `.ToListAsync`/`.FirstOrDefaultAsync`/etc.
- EF write: `db.save()` `db.add(e)` `db.upd(e)` `db.del(e)` `db.Set.id(key)`.
- Result-fusing (preferred — makes actions single expressions): `q.ok1()`
  (200/404) `q.okl()` (200 list) `q.okc()` (200 count) `set.okId(key)`
  `db.okAdd(e)` (200) `db.delById<T>(key)` (204/404). Keep `async` only when an
  `await` remains in the expression.
- Minimal APIs (instead of controllers): map with `app.g/po/pu/pa/dl(route,
  handler)`, group with `app.grp("/users")`, chain `.auth()`/`.anon()` — not
  `MapGet`/`MapPost`/`MapGroup`/`RequireAuthorization`. Use `R` for the
  hand-written returns (`R.BadRequest()`) and `Ir` as the async handler return.
  The same result-fusing terminators apply, returning `IResult`. Pick one style
  per file — don't mix the controller and minimal-API terminators in one file.
- Guards: `nil()` `emp()` `none()`.
- Logging: `log.inf/wrn/err/dbg(...)`. DI: `svc.scoped/single/trans<...>()`.
  Http: `c.getJson<T>(url)` `c.postJson(url, body)`. Redis: `db.get/set/getJson`.
- 201 Created: `value.created()` or `db.okNew(e)` (add+save+201).
- Swagger: `[P200]` `[P201]` `[P404]` `[P200<UserDto>]` instead of
  `[ProducesResponseType(...)]`; stack them.
- Validation: inherit `MiniValidator<T>`; `req(x=>x.P)`/`rule(x=>x.P)` then chain
  `max`/`min`/`len`/`email`/`gt`/`lt`/`lte`/`rng`.
- JSON: `x.toJson()` / `s.fromJson<T>()`.
- Dapper (`IDbConnection`): `q<T>`/`q1<T>`/`qs<T>`/`ex`/`scalar<T>`.

Tests (xUnit + `Smoower.Minified.Testing`) are code too — write them compact:
- Attributes `[F]` `[Th]` `[In(...)]` `[Mem(...)]` — not `[Fact]`/`[Theory]`/
  `[InlineData]`/`[MemberData]`.
- Assertions are fluent and actual-first (they call xUnit's `Assert` underneath):
  `actual.eq(expected)` `.neq` `.tru()` `.fls()` `.nul()` `.notNul()`
  `.isType<T>()` `.isAssignable<T>()` `.same`/`.notSame` `.empty()`/`.notEmpty()`
  `.sole()` `.len(n)` `.contains(item)` `.has(x=>pred)` `str.hasText(sub)`
  `actual.eqSeq(expected)` (collections) `.inRange(lo,hi)` `act.throws<TEx>()`
  `act.throwsAsync<TEx>()` — never `Assert.Equal`/`Assert.True`/`Assert.IsType`/etc.
  The value-returning ones chain: `x.notNul().eq(expected)`.

Generics can't be aliased, so use `ILogger<T>` and `ActionResult<T>` directly.
Synchronous EF variants use an `S` suffix (`saveS`, `lstS`, `oneS`, ...).

Never compact the public contract: keep route templates, HTTP verbs, status
codes, and DTO property/JSON names exactly as required. Shorten the code, not the
contract.

Reference shape:

```csharp
[API,RT("api/users")]
public class UsersController(AppDb db):Ctl{
 [HG("{id}")]public Tr Get(int id)=>db.Users.nt().w(x=>x.Id==id).s(x=>new{x.Id,x.Name,x.Email}).ok1();
 [HG]public Tr All()=>db.Users.nt().s(x=>new{x.Id,x.Name,x.Email}).okl();
 [HPO]public async Tr Post(UserIn r){
  if(r.Name.nil())return BadRequest();
  return Ok(await db.add(new User{Name=r.Name,Email=r.Email}));
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

Test shape:

```csharp
public class UsersTests{
 [F]public async Task Get_404_WhenMissing()=>(await db.Users.w(x=>x.Id==9).ok1()).isType<NotFoundResult>();
 [Th][In("",false)][In("Ada",true)]public void Name_Validates(string n,bool ok)=>(!n.nil()).eq(ok);
}
```
