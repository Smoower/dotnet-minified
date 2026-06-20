#!/usr/bin/env python3
"""Generate the Smoower.Minified documentation site (multi-page, static).

Single source of truth for the docs. No token computation here - the cheat sheet
lists what each long form becomes, nothing more. Re-run after changing a helper:

    python docs/build.py
"""
import html
import os

HERE = os.path.dirname(os.path.abspath(__file__))


def esc(s):
    return html.escape(s)


# ── Package catalogue (Libraries page) ──────────────────────────────────────
PACKAGES = [
    ("Smoower.Minified.Core", "Guards (nil/emp/none) and the base type aliases. Zero web or EF dependency, so everything else sits on top of it."),
    ("Smoower.Minified.AspNetCore", "Compact MVC attributes, type aliases, and the result-fusing terminators that turn a controller action into a single expression."),
    ("Smoower.Minified.MinimalApi", "Minimal API route mappers (g/po/pu/pa/dl/grp), auth/anon convention shorteners, and IResult result-fusing terminators for endpoint handlers."),
    ("Smoower.Minified.EFCore", "EF Core query and write helpers, plus model-configuration helpers for OnModelCreating."),
    ("Smoower.Minified.Extensions", "Helpers over core .NET / BCL types: an injectable Clock, DateTime and TimeSpan shorteners, and environment-variable access."),
    ("Smoower.Minified.Hosting", "Dependency-injection registration and resolution helpers over IServiceCollection / IServiceProvider."),
    ("Smoower.Minified.Logging", "ILogger helpers (inf/wrn/err/dbg), declared on the non-generic base so they apply to ILogger<T> too."),
    ("Smoower.Minified.Http", "HttpClient JSON helpers over System.Net.Http.Json."),
    ("Smoower.Minified.Validation", "Compact FluentValidation rule shorteners via MiniValidator<T>."),
    ("Smoower.Minified.Json", "System.Text.Json round-trip helpers. A Newtonsoft variant ships the identical surface."),
    ("Smoower.Minified.Redis", "StackExchange.Redis helpers on IDatabase, with JSON value helpers."),
    ("Smoower.Minified.Dapper", "Compact Dapper helpers on IDbConnection for projects not using EF Core."),
    ("Smoower.Minified.Testing", "Fluent, actual-first xUnit assertion shorteners (eq/neq/tru/notNul/isType/throws) plus the F/Th/In attribute aliases for AI-generated tests."),
    ("Smoower.Minified.EFCore.* providers", "Thin registration helpers for the InMemory, Sqlite, Npgsql, and SqlServer providers (mem/sqlite/pg/sql)."),
]


# ── Cheat-sheet mappings (long form, compact form) per package ──────────────
LIBS = [
    ("Smoower.Minified.Core", [
        ("Guards", [
            ("string.IsNullOrWhiteSpace(s)", "s.nil()"),
            ("string.IsNullOrEmpty(s)", "s.emp()"),
            ("!items.Any()", "items.none()"),
        ]),
        ("Aliases", [
            ("CancellationToken ct", "CT ct"),
            ("IConfiguration cfg", "Cfg cfg"),
        ]),
    ]),
    ("Smoower.Minified.AspNetCore", [
        ("Attributes", [
            ("[ApiController]", "[API]"),
            ('[Route("api/users")]', '[RT("api/users")]'),
            ('[HttpGet("{id}")]', '[HG("{id}")]'),
            ("[HttpPost]", "[HPO]"),
            ("[HttpPut]", "[HPU]"),
            ("[HttpPatch]", "[HPA]"),
            ('[HttpDelete("{id}")]', '[HD("{id}")]'),
            ("[Authorize]", "[AUTH]"),
            ("[AllowAnonymous]", "[ANON]"),
            ("[FromBody]", "[FB]"),
            ("[FromRoute]", "[FR]"),
            ("[FromQuery]", "[FQ]"),
            ("[FromHeader]", "[FH]"),
        ]),
        ("Aliases", [
            (": ControllerBase", ":Ctl"),
            ("IActionResult", "Res"),
            ("ActionResult", "AR"),
            ("Task<IActionResult>", "Tr"),
        ]),
        ("Result-fusing terminators", [
            ("var x=await q.FirstOrDefaultAsync();return x==null?NotFound():Ok(x);", "q.ok1()"),
            ("Ok(await q.ToListAsync())", "q.okl()"),
            ("Ok(await q.CountAsync())", "q.okc()"),
            ("var x=await set.FindAsync(id);return x==null?NotFound():Ok(x);", "set.okId(id)"),
            ("db.Add(e);await db.SaveChangesAsync();return Ok(e);", "db.okAdd(e)"),
            ("db.Add(e);await db.SaveChangesAsync();return CreatedAtAction(...);", "db.okNew(e)"),
            ("CreatedAtAction(nameof(Get), new { id = value.Id }, value)", "value.created()"),
            ("FindAsync+NotFound+Remove+SaveChanges+NoContent", "db.delById<User>(id)"),
        ]),
        ("Response-type attributes (Swagger)", [
            ("[ProducesResponseType(StatusCodes.Status200OK)]", "[P200]"),
            ("[ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]", "[P200<UserDto>]"),
            ("[ProducesResponseType(StatusCodes.Status201Created)]", "[P201]"),
            ("[ProducesResponseType(StatusCodes.Status400BadRequest)]", "[P400]"),
            ("[ProducesResponseType(StatusCodes.Status404NotFound)]", "[P404]"),
        ]),
    ]),
    ("Smoower.Minified.MinimalApi", [
        ("Route mapping", [
            ('app.MapGet("/x", handler)', 'app.g("/x", handler)'),
            ('app.MapPost("/x", handler)', 'app.po("/x", handler)'),
            ('app.MapPut("/x", handler)', 'app.pu("/x", handler)'),
            ('app.MapPatch("/x", handler)', 'app.pa("/x", handler)'),
            ('app.MapDelete("/x", handler)', 'app.dl("/x", handler)'),
            ('app.MapGroup("/users")', 'app.grp("/users")'),
            (".RequireAuthorization()", ".auth()"),
            (".AllowAnonymous()", ".anon()"),
        ]),
        ("Aliases", [
            ("Microsoft.AspNetCore.Http.Results", "R"),
            ("Task<IResult>", "Ir"),
        ]),
        ("Result-fusing terminators (IResult)", [
            ("var x=await q.FirstOrDefaultAsync();return x is null?Results.NotFound():Results.Ok(x);", "q.ok1()"),
            ("Results.Ok(await q.ToListAsync())", "q.okl()"),
            ("Results.Ok(await q.CountAsync())", "q.okc()"),
            ("var x=await set.FindAsync(id);return x is null?Results.NotFound():Results.Ok(x);", "set.okId(id)"),
            ("db.Add(e);await db.SaveChangesAsync();return Results.Created((string?)null,e);", "db.okNew(e)"),
            ("Results.Created((string?)null, value)", "value.created()"),
            ("FindAsync+NotFound+Remove+SaveChanges+NoContent", "db.delById<User>(id)"),
        ]),
    ]),
    ("Smoower.Minified.EFCore", [
        ("Query — composition", [
            ("q.Where(x=>x.Id==id)", "q.w(x=>x.Id==id)"),
            ("q.Select(x=>x.Name)", "q.s(x=>x.Name)"),
            ("q.OrderBy(x=>x.Name)", "q.ob(x=>x.Name)"),
            ("q.OrderByDescending(x=>x.Name)", "q.obd(x=>x.Name)"),
            ("q.ThenBy(x=>x.Name)", "q.tb(x=>x.Name)"),
            ("q.ThenByDescending(x=>x.Name)", "q.tbd(x=>x.Name)"),
            ("q.Skip(10)", "q.sk(10)"),
            ("q.Take(10)", "q.tk(10)"),
            ("q.AsNoTracking()", "q.nt()"),
            ("q.AsNoTrackingWithIdentityResolution()", "q.ntir()"),
            ("q.Include(x=>x.Orders)", "q.inc(x=>x.Orders)"),
            ("q.ThenInclude(x=>x.Lines)", "q.tinc(x=>x.Lines)"),
            ("q.GroupBy(x=>x.Status)", "q.gb(x=>x.Status)"),
        ]),
        ("Query — async terminators", [
            ("q.ToListAsync()", "q.lst()"),
            ("q.FirstOrDefaultAsync()", "q.one()"),
            ("q.SingleOrDefaultAsync()", "q.single()"),
            ("q.AnyAsync()", "q.any()"),
            ("q.CountAsync()", "q.cnt()"),
            ("q.MaxAsync(x=>x.Total)", "q.max(x=>x.Total)"),
            ("q.MinAsync(x=>x.Total)", "q.min(x=>x.Total)"),
        ]),
        ("Write (async; S-suffixed sync variants exist)", [
            ("set.FindAsync(key)", "set.id(key)"),
            ("await db.SaveChangesAsync()", "await db.save()"),
            ("db.Add(e);await db.SaveChangesAsync();", "await db.add(e)"),
            ("db.Update(e);await db.SaveChangesAsync();", "await db.upd(e)"),
            ("db.Remove(e);await db.SaveChangesAsync();", "await db.del(e)"),
        ]),
        ("Model config (OnModelCreating)", [
            ("modelBuilder.Entity<T>().HasQueryFilter(f)", "mb.qf<T>(f)"),
            ("entity.HasKey(e=>e.Id)", "entity.key(e=>e.Id)"),
            ("entity.HasIndex(e=>e.Email)", "entity.idx(e=>e.Email)"),
            (".IsUnique()", ".uniq()"),
            ("entity.Property(e=>e.Name)", "entity.p(e=>e.Name)"),
            (".IsRequired()", ".req()"),
            (".HasMaxLength(100)", ".max(100)"),
            (".HasConversion<string>()", ".conv<string>()"),
            ("entity.HasOne(e=>e.Author)", "entity.one(e=>e.Author)"),
            (".WithMany(a=>a.Books)", ".many(a=>a.Books)"),
            ("entity.HasMany(e=>e.Books)", "entity.hasM(e=>e.Books)"),
            (".WithOne(b=>b.Author)", ".wOne(b=>b.Author)"),
            (".HasForeignKey(e=>e.AuthorId)", ".fk(e=>e.AuthorId)"),
            (".OnDelete(DeleteBehavior.Cascade)", ".cascade()"),
            (".OnDelete(DeleteBehavior.Restrict)", ".restrict()"),
            (".OnDelete(DeleteBehavior.SetNull)", ".setNull()"),
            (".OnDelete(DeleteBehavior.ClientSetNull)", ".clientSetNull()"),
            (".OnDelete(behavior)", ".onDel(behavior)"),
        ]),
    ]),
    ("Smoower.Minified.Extensions", [
        ("Clock (injectable) / Clk (static)", [
            ("DateTime.UtcNow", "clock.utc"),
            ("DateTime.Now", "clock.now"),
            ("DateOnly.FromDateTime(DateTime.UtcNow)", "clock.today"),
            ("DateTimeOffset.UtcNow.ToUnixTimeSeconds()", "clock.unix"),
        ]),
        ("DateTime", [
            ("dt.ToUniversalTime()", "dt.utc()"),
            ("dt.ToShortDateString()", "dt.sd()"),
            ("dt.ToLongDateString()", "dt.ld()"),
            ("dt.ToShortTimeString()", "dt.st()"),
            ("dt.ToLongTimeString()", "dt.lt()"),
        ]),
        ("TimeSpan factories (on int)", [
            ("TimeSpan.FromMilliseconds(250)", "250.ms()"),
            ("TimeSpan.FromSeconds(30)", "30.secs()"),
            ("TimeSpan.FromMinutes(5)", "5.mins()"),
            ("TimeSpan.FromHours(2)", "2.hrs()"),
            ("TimeSpan.FromDays(7)", "7.days()"),
        ]),
        ("Environment", [
            ('Environment.GetEnvironmentVariable("X")', 'Env.get("X")'),
            ('Environment.SetEnvironmentVariable("X", v)', 'Env.set("X", v)'),
        ]),
    ]),
    ("Smoower.Minified.Hosting", [
        ("IServiceCollection / IServiceProvider", [
            ("services.AddScoped<IFoo, Foo>()", "svc.scoped<IFoo, Foo>()"),
            ("services.AddSingleton<Bar>()", "svc.single<Bar>()"),
            ("services.AddTransient<Baz>()", "svc.trans<Baz>()"),
            ("provider.GetRequiredService<T>()", "provider.svc<T>()"),
        ]),
    ]),
    ("Smoower.Minified.Logging", [
        ("ILogger", [
            ('log.LogInformation("created {Id}", id)', 'log.inf("created {Id}", id)'),
            ('log.LogWarning("slow {Ms}", ms)', 'log.wrn("slow {Ms}", ms)'),
            ('log.LogError("failed {Id}", id)', 'log.err("failed {Id}", id)'),
            ('log.LogDebug("state {S}", s)', 'log.dbg("state {S}", s)'),
        ]),
    ]),
    ("Smoower.Minified.Http", [
        ("HttpClient", [
            ("c.GetFromJsonAsync<T>(url)", "c.getJson<T>(url)"),
            ("c.PostAsJsonAsync(url, body)", "c.postJson(url, body)"),
            ("c.PutAsJsonAsync(url, body)", "c.putJson(url, body)"),
            ("c.PatchAsJsonAsync(url, body)", "c.patchJson(url, body)"),
            ("c.DeleteAsync(url)", "c.del(url)"),
        ]),
    ]),
    ("Smoower.Minified.Validation", [
        ("Rules (on MiniValidator<T>)", [
            ("RuleFor(x=>x.Name).NotEmpty()", "req(x=>x.Name)"),
            ("RuleFor(x=>x.Age)", "rule(x=>x.Age)"),
            ("r.MaximumLength(100)", "r.max(100)"),
            ("r.MinimumLength(2)", "r.min(2)"),
            ("r.EmailAddress()", "r.email()"),
            ("r.GreaterThan(0)", "r.gt(0)"),
            ("r.LessThanOrEqualTo(120)", "r.lte(120)"),
            ("r.InclusiveBetween(1, 5)", "r.rng(1, 5)"),
        ]),
    ]),
    ("Smoower.Minified.Json", [
        ("System.Text.Json (Newtonsoft variant identical)", [
            ("JsonSerializer.Serialize(x)", "x.toJson()"),
            ("JsonSerializer.Deserialize<T>(s)", "s.fromJson<T>()"),
        ]),
    ]),
    ("Smoower.Minified.Dapper", [
        ("IDbConnection", [
            ("c.QueryAsync<T>(sql, p)", "c.q<T>(sql, p)"),
            ("c.QueryFirstOrDefaultAsync<T>(sql, p)", "c.q1<T>(sql, p)"),
            ("c.QuerySingleOrDefaultAsync<T>(sql, p)", "c.qs<T>(sql, p)"),
            ("c.ExecuteAsync(sql, p)", "c.ex(sql, p)"),
            ("c.ExecuteScalarAsync<T>(sql, p)", "c.scalar<T>(sql, p)"),
        ]),
    ]),
    ("Smoower.Minified.Testing", [
        ("Assertions (over xUnit, actual-first)", [
            ("Assert.Equal(expected, actual)", "actual.eq(expected)"),
            ("Assert.NotEqual(expected, actual)", "actual.neq(expected)"),
            ("Assert.Equal(expected, actual)  // collections", "actual.eqSeq(expected)"),
            ("Assert.True(x)", "x.tru()"),
            ("Assert.False(x)", "x.fls()"),
            ("Assert.Null(x)", "x.nul()"),
            ("Assert.NotNull(x)", "x.notNul()"),
            ("Assert.IsType<T>(x)", "x.isType<T>()"),
            ("Assert.IsAssignableFrom<T>(x)", "x.isAssignable<T>()"),
            ("Assert.Same(expected, actual)", "actual.same(expected)"),
            ("Assert.NotSame(expected, actual)", "actual.notSame(expected)"),
            ("Assert.Empty(xs)", "xs.empty()"),
            ("Assert.NotEmpty(xs)", "xs.notEmpty()"),
            ("Assert.Single(xs)", "xs.sole()"),
            ("Assert.Equal(n, xs.Count())", "xs.len(n)"),
            ("Assert.Contains(item, xs)", "xs.contains(item)"),
            ("Assert.Contains(xs, x => pred)", "xs.has(x => pred)"),
            ("Assert.Contains(sub, str)", "str.hasText(sub)"),
            ("Assert.InRange(x, lo, hi)", "x.inRange(lo, hi)"),
            ("Assert.Throws<TEx>(act)", "act.throws<TEx>()"),
            ("Assert.ThrowsAsync<TEx>(act)", "act.throwsAsync<TEx>()"),
        ]),
        ("Attribute aliases (re-declare in your test GlobalUsings)", [
            ("[Fact]", "[F]"),
            ("[Theory]", "[Th]"),
            ("[InlineData(...)]", "[In(...)]"),
            ("[MemberData(...)]", "[Mem(...)]"),
        ]),
    ]),
]


# ── Rendering ───────────────────────────────────────────────────────────────
NAV = [
    ("Getting started", [
        ("index.html", "Introduction"),
        ("quickstart.html", "Quickstart"),
        ("installation.html", "Installation"),
    ]),
    ("Concepts", [
        ("compaction-levels.html", "Compaction levels"),
        ("how-it-works.html", "How it works"),
        ("economics.html", "Does it pay off?"),
    ]),
    ("Reference", [
        ("libraries.html", "Libraries"),
        ("cheat-sheet.html", "Cheat sheet"),
    ]),
    ("Tooling", [
        ("tooling.html", "CLI &amp; editor tooling"),
    ]),
]


def render_nav(current):
    out = []
    for group, items in NAV:
        out.append(f'<div class="navgroup">{group}</div>')
        for href, label in items:
            cls = ' class="active"' if href == current else ""
            out.append(f'<a href="{href}"{cls}>{label}</a>')
    return "\n".join(out)


def cards(items):
    out = ['<div class="cards">']
    for href, title, desc in items:
        out.append(
            f'<a class="card" href="{href}"><div class="t">{esc(title)} '
            f'<span class="arr">&rarr;</span></div><div class="d">{esc(desc)}</div></a>')
    out.append("</div>")
    return "\n".join(out)


def map_table(rows):
    out = ['<table><thead><tr><th>Long form</th><th>Compact</th></tr></thead><tbody>']
    for long, short in rows:
        out.append(
            f'<tr><td class="long"><code>{esc(long)}</code></td>'
            f'<td class="short"><code>{esc(short)}</code></td></tr>'
        )
    out.append("</tbody></table>")
    return "\n".join(out)


SHELL = """<!doctype html>
<html lang="en" data-theme="light">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>{title}</title>
<meta name="description" content="{desc}">
<link rel="stylesheet" href="styles.css">
<script>(function(){{try{{var t=localStorage.getItem('sm-theme');if(t)document.documentElement.setAttribute('data-theme',t);}}catch(e){{}}}})();</script>
</head>
<body>
<div class="layout">
<aside class="sidebar">
  <div class="brand">
    <a class="name" href="index.html"><b>Smoower<em>.Minified</em></b><span>{subtitle}</span></a>
    <button class="theme-toggle" type="button" aria-label="Toggle light / dark theme" onclick="(function(){{var d=document.documentElement,n=d.getAttribute('data-theme')==='dark'?'light':'dark';d.setAttribute('data-theme',n);try{{localStorage.setItem('sm-theme',n);}}catch(e){{}}}})()">&#9728;</button>
  </div>
  <nav>
{nav}
  </nav>
</aside>
<main class="content" id="top">
{content}
<footer>
  Generated by <code>docs/build.py</code> &middot; part of the <code>&middot;minified</code> family &middot; source-available under a non-compete license (see <code>LICENSE</code>).
</footer>
</main>
</div>
</body>
</html>
"""


def page(filename, title, desc, subtitle, content):
    out = SHELL.format(title=title, desc=desc, subtitle=subtitle,
                       nav=render_nav(filename), content=content)
    with open(os.path.join(HERE, filename), "w", encoding="utf-8") as f:
        f.write(out)
    print(f"wrote {filename}")


# ── Page content ─────────────────────────────────────────────────────────────
INDEX = """<section>
  <div class="hero">
    <h1>Same .NET code. Fewer tokens.</h1>
    <p class="tagline">same .NET code &middot; fewer tokens &middot; no magic</p>
    <p class="sub">Smoower.Minified is a set of small C# libraries that strip the boilerplate out of .NET APIs (controllers, EF Core queries, DI, logging) and replace it with short, stable forms an AI can emit in a fraction of the tokens. It's plain C#: same IL, no transpiler.</p>
    <div class="badges">
      <span class="badge brand">.NET 8 &middot; 9 &middot; 10</span>
      <span class="badge">no transpiler &middot; same IL</span>
      <span class="badge">source-available</span>
      <span class="badge">ships as a Claude Code skill</span>
    </div>
    <div class="cta">
      <a class="btn primary" href="quickstart.html">Get started &rarr;</a>
      <a class="btn ghost" href="compaction-levels.html">Compaction levels</a>
      <a class="btn ghost" href="cheat-sheet.html">Cheat sheet</a>
    </div>
  </div>

  <h2>What it is</h2>
  <p class="lead">When an AI writes ASP.NET Core or EF Core code, most of what it types isn't your logic. It's framework ceremony: <code>Task&lt;IActionResult&gt;</code>, <code>FirstOrDefaultAsync</code>, <code>AddScoped</code>, attribute noise, the same controller scaffolding over and over. Every token of it is regenerated on each write, rewrite, and refactor.</p>
  <p class="lead">Smoower.Minified swaps that ceremony for short, stable helpers (<code>Tr</code>, <code>.one()</code>, <code>scoped&lt;&gt;</code>, <code>[HG]</code>, <code>.ok1()</code>) that compile to the exact same IL. The alias layer is plain C#: extension methods, attributes, and type aliases. An opt-in <code>[Crud&lt;&gt;]</code> source generator and the deeper levels go further when you want them. The behavior never changes. Only the token count does.</p>

  <h2>Why it helps</h2>
  <div class="stat">
    <div class="box"><div class="n hl">Faster</div><div class="l">generation time tracks output length. Less to type, less to wait for.</div></div>
    <div class="box"><div class="n hl">Cheaper</div><div class="l">output tokens are billed, and there are simply fewer of them</div></div>
    <div class="box"><div class="n hl">Lighter</div><div class="l">shorter code leaves more room in the context window over a session</div></div>
  </div>
  <p class="lead">Expect <strong>10-25% fewer output tokens across a whole project</strong>, and <strong>up to 50% on the boilerplate-heavy controller files</strong> an assistant rewrites most often. The full arithmetic is on <a href="economics.html">Does it pay off?</a></p>
  <p class="lead"><strong>And it compounds with every agent step.</strong> A coding assistant doesn't write a file once. It works in a loop: read the code, edit it, re-read it, refactor, run again. Each step re-emits and re-reads the same source, so a token saved isn't saved once. It's saved again on every turn that touches that code, and again as that smaller output becomes the input the next step reads. The leaner each step, the leaner the one built on top of it, so the gap against the verbose form widens the longer an agent runs.</p>

  <h2>What it saves on real code</h2>
  <p class="lead">Not a toy benchmark. These are untouched production files from a live .NET app, rewritten in the compact style and measured with Claude&rsquo;s own tokenizer (<code>claude-opus-4-8</code>). On files that are mostly framework ceremony, the compact form cuts up to 50% of the tokens. Across a whole application, where most of the code is business logic that shouldn't compress, the saving settles around 25%.</p>
  <table>
  <thead><tr><th>File</th><th>Original</th><th>Smoower</th><th>Saved</th></tr></thead>
  <tbody>
    <tr><td><code>AdminAuditController.cs</code></td><td>7,326</td><td>5,304</td><td><span class="delta pos">&minus;2,022 &middot; 28%</span></td></tr>
    <tr><td><code>EntryExpiryController.cs</code></td><td>3,640</td><td>2,510</td><td><span class="delta pos">&minus;1,130 &middot; 31%</span></td></tr>
    <tr><td><code>ExpiryDashboardController.cs</code></td><td>4,023</td><td>2,272</td><td><span class="delta pos">&minus;1,751 &middot; 44%</span></td></tr>
    <tr><td><code>RasepiDbContext.cs</code> <span class="deps">(~2k-line EF schema)</span></td><td>31,713</td><td>23,993</td><td><span class="delta pos">&minus;7,720 &middot; 24%</span></td></tr>
    <tr><td><strong>Total</strong></td><td><strong>46,702</strong></td><td><strong>34,079</strong></td><td><span class="delta pos">&minus;12,623 &middot; 27%</span></td></tr>
  </tbody>
  </table>
  <p class="lead">At Opus 4.8&rsquo;s rate of <strong>$25 per million output tokens</strong>, emitting this sample the verbose way costs <strong>$1.17</strong>. The compact way costs <strong>$0.85</strong>. That's <strong>$0.32 saved every time</strong> these files are written, and AI-generated code gets written, rewritten, and refactored over and over across a project's life.</p>

  <div class="callout"><strong>The one rule: never compact the contract.</strong> Route templates, HTTP verbs, status codes, and DTO/JSON names stay exactly as your API requires. Smoower changes how code is written, never what it does at runtime.</div>

  <h2>Where to next</h2>
"""


QUICKSTART = """<section>
  <h1>Quickstart</h1>
  <p class="lead">The fastest path: add the packages, drop in the aliases, and point your assistant at the style. If your assistant has tool access, it can do all of this for you.</p>

  <h2>Let your AI set it up</h2>
  <p class="lead">Paste <code>prompts/setup-prompt.md</code> into your assistant in a new or existing repo. It detects the project (or scaffolds a Web API in an empty one), installs the packages it needs, writes <code>GlobalUsings.cs</code>, adds the compact-style rules to your <code>CLAUDE.md</code> / <code>copilot-instructions.md</code>, and builds to verify. It is safe to re-run; it only adds what is missing.</p>
  <div class="callout info"><strong>Claude Code</strong> users can skip the paste &mdash; the repo ships a skill at <code>.claude/skills/dotnet/</code>; just ask it to &ldquo;set up Smoower.Minified in this project.&rdquo;</div>

  <h2>Or install the Claude Code plugin</h2>
  <p class="lead">Smoower.Minified ships as a Claude Code plugin on Anthropic&rsquo;s community marketplace. Add it once and the skill applies the compact style automatically &mdash; it even asks which <a href="compaction-levels.html">compaction level</a> to use before it generates:</p>
  <pre>/plugin marketplace add anthropics/claude-plugins-community
/plugin install smoower-minified@claude-community</pre>
  <p class="lead">It auto-invokes on any project that references the <code>Smoower.Minified.*</code> packages; call it explicitly with <code>/smoower-minified:dotnet</code>.</p>

  <h2>Or three small steps by hand</h2>
  <pre># 1 — add the packages (ASP.NET Core backend set)
dotnet add package Smoower.Minified.AspNetCore
dotnet add package Smoower.Minified.EFCore

# 2 — drop the usings + aliases into a GlobalUsings.cs
#     (copy from samples/Smoower.Minified.SampleApi/GlobalUsings.cs)

# 3 — point your AI at the style
#     Claude Code -> just ask it to "use Smoower.Minified"  (ships as a skill)
#     Copilot / Cursor / GPT -> paste prompts/system-prompt.md</pre>
  <p class="lead">That&rsquo;s it &mdash; your next controller comes out compact. For the full package list and the aliases to copy, see <a href="installation.html">Installation</a>.</p>

  <h2>What compact looks like</h2>
  <p class="lead">A normal controller action and its Smoower equivalent &mdash; identical behaviour, identical compiled IL:</p>
  <pre>[HttpGet("{id}")]
public async Task&lt;IActionResult&gt; Get(int id)
{
    var x = await _db.Users
        .AsNoTracking()
        .Where(u =&gt; u.Id == id)
        .Select(u =&gt; new { u.Id, u.Name, u.Email })
        .FirstOrDefaultAsync();
    return x == null ? NotFound() : Ok(x);
}</pre>
  <pre>[HG("{id}")]public Tr Get(int id)=&gt;db.Users.nt().w(x=&gt;x.Id==id).s(x=&gt;new{x.Id,x.Name,x.Email}).ok1();</pre>
  <p class="lead"><code>ok1()</code> runs the query and returns <code>200</code> with the row, or <code>404</code> if missing &mdash; exactly the <code>x == null ? NotFound() : Ok(x)</code> above, folded into the call. <a href="how-it-works.html">How it works</a> walks through why this saves tokens.</p>

  <h2>Where to next</h2>
"""


INSTALLATION = """<section>
  <h1>Installation</h1>
  <p class="lead">Add the packages you need, drop the aliases into your project, and point your assistant at the style. Every package is independent &mdash; take only what you use.</p>

  <h2>1 &mdash; Add the packages</h2>
  <p class="lead">All packages are <code>0.4.0</code>, multi-targeting net8.0 / net9.0 / net10.0. The default set for an ASP.NET Core backend:</p>
  <pre>dotnet add package Smoower.Minified.Core
dotnet add package Smoower.Minified.AspNetCore
dotnet add package Smoower.Minified.EFCore
dotnet add package Smoower.Minified.Hosting
dotnet add package Smoower.Minified.Logging
dotnet add package Smoower.Minified.Validation</pre>
  <p class="lead">Or as <code>PackageReference</code> entries &mdash; take only what you use (see <a href="libraries.html">Libraries</a>):</p>
  <pre>&lt;PackageReference Include="Smoower.Minified.AspNetCore" Version="0.4.0" /&gt;
&lt;PackageReference Include="Smoower.Minified.EFCore" Version="0.4.0" /&gt;</pre>

  <h2>2 &mdash; Drop in the aliases</h2>
  <p class="lead">Add the imports and aliases to a <code>GlobalUsings.cs</code> in your project. Aliases are not transitive across assemblies, so they live in <em>your</em> code (copy from the <code>samples/Smoower.Minified.SampleApi/GlobalUsings.cs</code>):</p>
  <pre>global using Smoower.Minified.Core;
global using Smoower.Minified.AspNetCore;
global using Smoower.Minified.EFCore;
global using Smoower.Minified.Hosting;
global using Smoower.Minified.Logging;
global using Res = Microsoft.AspNetCore.Mvc.IActionResult;
global using AR  = Microsoft.AspNetCore.Mvc.ActionResult;
global using CT  = System.Threading.CancellationToken;
global using Cfg = Microsoft.Extensions.Configuration.IConfiguration;
global using Tr  = System.Threading.Tasks.Task&lt;Microsoft.AspNetCore.Mvc.IActionResult&gt;;</pre>

  <h2>3 &mdash; Point your assistant at the style</h2>
  <p class="lead"><strong>Claude Code</strong> uses the bundled skill at <code>.claude/skills/dotnet/</code> &mdash; just ask it to use Smoower.Minified. <strong>GPT / Copilot / Cursor</strong> take <code>prompts/system-prompt.md</code> as a system prompt or rules file. A short version to drop into any chat:</p>
  <pre>Generate ASP.NET Core / EF Core code using the Smoower.Minified compact
helpers ([API]/[HG]/[HPO], :Ctl, Tr, .w/.s/.nt/.lst/.one,
db.save/db.add, ok1/okl/okId/delById, nil()). Code only, no comments,
file-scoped namespaces, primary constructors, records for DTOs.
Never change route templates, status codes, or DTO/JSON names.</pre>
  <p class="lead">Keep the <a href="cheat-sheet.html">Cheat sheet</a> handy &mdash; that is it.</p>

  <h2>Where to next</h2>
"""


COMPACTION_LEVELS = """<section>
  <h1>Compaction levels</h1>
  <p class="tagline">a dial, not a switch</p>
  <p class="lead">Smoower.Minified is a dial. You choose how far to push token savings against how readable the code stays on disk. The Claude Code skill <strong>asks which level</strong> before it generates; planned <a href="tooling.html">tooling</a> (a VS Code virtual view + a <code>names.map</code>) restores full readability at the deeper levels. At every level the contract &mdash; routes, status codes, JSON/DB <em>values</em> &mdash; is unchanged; only the in-code handle moves.</p>

  <div class="ladder">
    <div class="level">
      <div class="tag">L1 &mdash; Aliases</div>
      <h3>Short handles for framework ceremony</h3>
      <p>The smoower short handles plus the optional <code>[Crud&lt;&gt;]</code> generator &mdash; <code>[HG]</code>, <code>:Ctl</code>, <code>Tr</code>, <code>.w</code>/<code>.s</code>/<code>.nt</code>, <code>.ok1()</code>. 100% ordinary C#. Reaches the framework ceremony (roughly 18&ndash;35% on a controller) and stays completely readable in any editor.</p>
      <div class="meta"><span class="badge on">readable on disk</span><span class="badge">same IL</span><span class="badge">no tooling needed</span></div>
    </div>
    <div class="level">
      <div class="tag">L2 &mdash; Mapped</div>
      <h3>Short domain names, contract pinned</h3>
      <p>Short domain identifiers, with the long form pinned once in <code>[JPN]</code>/<code>[Col]</code>/<code>global using</code> and recorded in a <code>names.map</code>. Reaches the business-logic and contract floor, and the saving compounds as the codebase grows: the long name is paid once at the declaration, the short name everywhere it is referenced.</p>
      <div class="meta"><span class="badge on">readable with tooling</span><span class="badge">contract values unchanged</span></div>
    </div>
    <div class="level">
      <div class="tag">L3 &mdash; Max</div>
      <h3>Whitespace packed</h3>
      <p>Everything from L2 with newlines and indentation removed &mdash; the densest form. Authored and read through the planned pack/expand tooling, which round-trips it back to fully formatted C# on the way in.</p>
      <div class="meta"><span class="badge on">tooling view</span><span class="badge">deterministic round-trip</span></div>
    </div>
  </div>

  <h2>The ladder, side by side</h2>
  <table>
  <thead><tr><th>Level</th><th>What it adds</th><th>Reaches</th><th>Readable on disk?</th></tr></thead>
  <tbody>
    <tr><td><strong>L1 &mdash; Aliases</strong></td><td>smoower short handles + optional <code>[Crud&lt;&gt;]</code> generator</td><td>framework ceremony (~18&ndash;35% on a controller)</td><td>yes</td></tr>
    <tr><td><strong>L2 &mdash; Mapped</strong></td><td>short domain names, long form pinned in <code>[JPN]</code>/<code>[Col]</code>/<code>global using</code> + a <code>names.map</code></td><td>the business-logic / contract floor; compounds with codebase size</td><td>with tooling</td></tr>
    <tr><td><strong>L3 &mdash; Max</strong></td><td>whitespace packed &mdash; every newline + indentation removed</td><td>everything</td><td>tooling view</td></tr>
  </tbody>
  </table>

  <h2>Measured on a real API</h2>
  <p class="lead">On a real task-management API (<code>samples/TodoApi</code>) the ladder measured traditional &rarr; smoower &rarr; packed at <strong>5,049 &rarr; 4,121 (~18%) &rarr; 3,785 (~25%)</strong> Claude tokens. Short-naming the hot domain vocabulary (L2) keeps paying as the codebase grows (see <code>bench/FINDINGS.md</code> &sect;4&ndash;6).</p>
  <div class="stat">
    <div class="box"><div class="n hl">~18%</div><div class="l">L1 aliases vs traditional</div></div>
    <div class="box"><div class="n hl">~25%</div><div class="l">packed (L3) vs traditional</div></div>
    <div class="box"><div class="n hl">compounding</div><div class="l">L2 domain naming, as the codebase grows</div></div>
  </div>

  <div class="callout info"><strong>Pick by taste.</strong> If you value readable-on-disk above all, stay at L1 &mdash; it is plain C# and needs no tooling. Reach for L2/L3 when raw token count matters most and you are using the <a href="tooling.html">pack/expand tooling</a> that restores readability.</div>

  <h2>Where to next</h2>
"""


HOW_IT_WORKS = """<section>
  <h1>How it works</h1>
  <p class="lead">Smoower saves tokens in two ways: it collapses long identifiers that tokenize into several sub-tokens, and it fuses whole phrases of ceremony into a single call. Everything compiles to the same IL.</p>

  <h2>The bill nobody talks about</h2>
  <p class="lead">Here is a perfectly normal controller action. Count the ceremony:</p>
  <pre>[HttpGet("{id}")]
public async Task&lt;IActionResult&gt; Get(int id)
{
    var x = await _db.Users
        .AsNoTracking()
        .Where(u =&gt; u.Id == id)
        .Select(u =&gt; new { u.Id, u.Name, u.Email })
        .FirstOrDefaultAsync();
    return x == null ? NotFound() : Ok(x);
}</pre>
  <p class="lead"><code>HttpGet</code>, <code>Task&lt;IActionResult&gt;</code>, <code>AsNoTracking</code>, <code>FirstOrDefaultAsync</code>, <code>NotFound</code>, <code>Ok</code> &mdash; none of that is your logic. It is the framework tax, and your assistant pays it again in output tokens on every write, rewrite, and refactor.</p>
  <p class="lead">The same action with Smoower.Minified:</p>
  <pre>[HG("{id}")]public Tr Get(int id)=&gt;db.Users.nt().w(x=&gt;x.Id==id).s(x=&gt;new{x.Id,x.Name,x.Email}).ok1();</pre>
  <p class="lead">One line, same behaviour. <code>ok1()</code> runs the query and returns <code>200</code> with the row, or <code>404</code> if missing &mdash; the exact <code>x == null ? NotFound() : Ok(x)</code>, folded into the call.</p>

  <h2>Where the savings come from</h2>
  <p class="lead">Two things do the work. First, collapsing long PascalCase identifiers that tokenize into three to five sub-tokens &mdash; <code>FirstOrDefaultAsync</code>, <code>Task&lt;IActionResult&gt;</code>, <code>AddScoped</code> &mdash; into short handles. Second, the result-fusing terminators that delete whole <code>async</code>/<code>await</code>/<code>return</code>/<code>Ok</code> phrases: <code>q.ok1()</code>, <code>q.okl()</code>, <code>set.okId(id)</code>, <code>db.okNew(e)</code>, <code>db.delById&lt;T&gt;(id)</code>.</p>
  <p class="lead">Dropping the long <code>...Async</code> suffix is why async is the unmarked default and synchronous code takes the <code>S</code> suffix (<code>saveS</code>, <code>idS</code>) &mdash; the common path is the short one. Every mapping is listed on the <a href="cheat-sheet.html">Cheat sheet</a>.</p>

  <h2>Still ordinary C#</h2>
  <p class="lead">The alias layer (L1) is plain C# extension methods, attributes, and type aliases &mdash; no transpiler, no source generator, the same IL the verbose form produces. You can step through it in a debugger like any other code. The opt-in <code>[Crud&lt;&gt;]</code> generator and the deeper <a href="compaction-levels.html">compaction levels</a> go further when you want them, always keeping the public contract intact.</p>

  <div class="callout"><strong>Never compact the contract.</strong> Route templates, HTTP verbs, status codes, and DTO / JSON names stay exactly as your API requires. Shorten the code, not the contract.</div>

  <h2>Where to next</h2>
"""


ECONOMICS = """<section>
  <h1>Does it pay off?</h1>
  <p class="tagline">faster, cheaper, lighter on context. measured on real production code.</p>
  <p class="lead">The promise is simple: an AI that writes less boilerplate writes faster, costs less, and keeps more of its context window free. To see how much of that survives real code, we ran Smoower over a slice of a live .NET app: a handful of production API controllers and one sprawling EF Core <code>DbContext</code> of nearly 2,000 lines. Then measured before and after with the model&rsquo;s own tokenizer.</p>

  <h2>What the real code showed</h2>
  <p class="lead">The compact form came out smaller everywhere it touched framework ceremony. Typical controllers dropped around 30%. The most boilerplate-heavy lost up to 50%. Even the giant <code>DbContext</code>, almost all schema config, came down around 25% once the EF Core helpers were applied. Across the whole sample, on untouched real-world code, the saving landed near 25%.</p>
  <div class="stat">
    <div class="box"><div class="n hl">up to 50%</div><div class="l">on the most boilerplate-heavy controllers</div></div>
    <div class="box"><div class="n hl">around 25%</div><div class="l">across a whole real-world slice, schema config included</div></div>
  </div>

  <h2>Faster: the strongest claim</h2>
  <p class="lead">Models emit output one token at a time, so the wall-clock time to produce a file tracks its length almost linearly. Cut the ceremony in half and you roughly halve the time spent streaming it out. The rules prompt is read once, in parallel, and cached, so it barely touches latency. This is the benefit that holds up most cleanly.</p>

  <h2>Cheaper: after a small, one-time cost</h2>
  <p class="lead">Output tokens are billed, and they cost several times more than input (around 5x on current Claude models). The compact code emits fewer of them on every file. Against that sits one cost: a short rules prompt the assistant keeps in context. That's input, not output, paid once per session, and cached. On any project that generates more than a file or two it pays for itself fast, and everything after is saving.</p>

  <h2>Lighter on context: compounds over a session</h2>
  <p class="lead">In a long session, everything already written gets re-read on every later turn. Code that's 25% to 50% smaller leaves more headroom before the window fills, makes each later turn a little cheaper, and pushes summarization further out. On subscription tools (Claude Pro/Max, Copilot, Cursor) this is where the value lands: more of your codebase fits at once, agents spend less of their budget regenerating ceremony, and you hit usage limits later in the day.</p>

  <div class="callout info"><strong>The bottom line.</strong> For an assistant writing ASP.NET Core and EF Core across a session, all three show up on real code: faster (the cleanest win), cheaper (after a sub-one-file break-even), and lighter on context (it compounds over turns). The runtime behavior is identical, since the compiled IL doesn't change. The full arithmetic and the per-file numbers are in <code>bench/FINDINGS.md</code>.</div>

  <h2>Where to next</h2>
"""


def render_packages():
    rows = "\n".join(
        f"<tr><td><code>{esc(n)}</code></td><td>{esc(b)}</td></tr>" for n, b in PACKAGES)
    body = f"""<section>
  <h1>Supported libraries &amp; packages</h1>
  <p class="lead">Each package is independent &mdash; take only what you use. The data and web layers are split so a console worker can reference <code>EFCore</code> without dragging in ASP.NET Core. Every mapping each package provides is on the <a href="cheat-sheet.html">Cheat sheet</a>.</p>
  <table><thead><tr><th>Package</th><th>What it covers</th></tr></thead><tbody>
{rows}
  </tbody></table>
  <div class="callout info"><strong>Want to see your favourite library here?</strong> Smoower grows by the libraries its users lean on. If there is a package you would like covered, <a href="https://github.com/smoower/dotnet-minified/issues">open an issue and let us know</a>.</div>

  <h2>Where to next</h2>
"""
    return body + cards([
        ("cheat-sheet.html", "Cheat sheet", "Every long-form to compact mapping, grouped by package."),
        ("compaction-levels.html", "Compaction levels", "How far to push the dial, L1 through L3."),
        ("tooling.html", "Tooling", "The CLI and editor integrations on the roadmap."),
    ]) + "\n</section>"


def render_cheatsheet():
    out = ['<section>', '<h1>Cheat sheet</h1>',
           '<p class="lead">Every mapping, grouped by package. The long form on the left, the compact form Smoower uses on the right. Same behaviour, same compiled IL.</p>']
    for name, groups in LIBS:
        anchor = name.replace("Smoower.Minified.", "").replace(".", "-").lower()
        out.append(f'<h2 id="{anchor}">{esc(name)}</h2>')
        for title, rows in groups:
            out.append(f"<h3>{esc(title)}</h3>")
            out.append(map_table(rows))
    out.append("</section>")
    return "\n".join(out)


TOOLING = """<section>
  <h1>CLI &amp; editor tooling</h1>
  <p class="tagline">author packed, read expanded</p>
  <p class="lead">Smoower&rsquo;s libraries are the foundation; a tooling layer on top makes the deeper <a href="compaction-levels.html">compaction levels</a> practical day to day. The libraries stay source-available and free.</p>

  <h2>Recently shipped</h2>
  <p class="lead">The most recent release added the <code>[Crud&lt;&gt;]</code> source generator, <code>whereIf</code>/<code>paged</code> query terminators, exception aliases (<code>KNF</code>/<code>IOE</code>), the <code>Ctl</code> result helpers (<code>nf</code>/<code>un</code>/<code>unp</code>), the <code>[JPN]</code>/<code>[Col]</code>/<code>[JSEM]</code> attribute aliases for L2 short-naming, and the <code>Smoower.Minified.Identity</code> package. Earlier releases landed <code>created()</code>, the <code>[P200]</code> Swagger attributes, <code>cfg.bind&lt;T&gt;()</code>, and the JSON shorteners.</p>

  <h2>On the roadmap</h2>
  <div class="cards">
    <div class="card"><div class="t">Pack / expand CLI</div><div class="d">A command-line tool to apply, lint, and round-trip the compact style, with the <code>names.map</code> and the <code>[Crud&lt;&gt;]</code> generator as the deterministic expanders.</div></div>
    <div class="card"><div class="t">VS Code virtual view</div><div class="d">Author packed (L3), read expanded &mdash; a virtual document that shows fully formatted C# while the file on disk stays dense. This is what makes L3 practical.</div></div>
    <div class="card"><div class="t">More of the framework</div><div class="d">ASP.NET Identity is in; next are the surfaces a real app still writes long, and beyond ASP.NET, other .NET flavors such as Blazor and WPF/XAML.</div></div>
    <div class="card"><div class="t">The minified family</div><div class="d"><code>react-minified</code>, <code>vue-minified</code> and more &mdash; the same idea for the front-end ceremony AI rewrites most.</div></div>
  </div>

  <h2>The measured discipline</h2>
  <p class="lead">A shortener ships only when it saves Claude tokens <em>and</em> the model reaches for it. That keeps the surface tight: every helper on the <a href="cheat-sheet.html">Cheat sheet</a> earned its place against the model&rsquo;s own tokenizer.</p>

  <div class="callout info"><strong>Want a runtime covered, or building one?</strong> Smoower grows by the libraries its users lean on. <a href="https://github.com/smoower/dotnet-minified/issues">Open an issue</a> and let us know.</div>

  <h2>Where to next</h2>
"""


NEXT_INDEX = cards([
    ("quickstart.html", "Quickstart", "Add the packages and point your assistant at the style."),
    ("compaction-levels.html", "Compaction levels", "L1 to L3 — how far to push the dial."),
    ("how-it-works.html", "How it works", "Why the compact form costs fewer tokens."),
    ("cheat-sheet.html", "Cheat sheet", "Every long-form to compact mapping."),
]) + "\n</section>"

NEXT_QUICKSTART = cards([
    ("installation.html", "Installation", "The full package list and the aliases to copy."),
    ("how-it-works.html", "How it works", "Why the compact form saves tokens."),
    ("cheat-sheet.html", "Cheat sheet", "Every mapping, grouped by package."),
]) + "\n</section>"

NEXT_INSTALLATION = cards([
    ("cheat-sheet.html", "Cheat sheet", "Every long-form to compact mapping."),
    ("compaction-levels.html", "Compaction levels", "Choose how far to push token savings."),
    ("libraries.html", "Libraries", "What each package covers."),
]) + "\n</section>"

NEXT_LEVELS = cards([
    ("tooling.html", "Tooling", "The pack/expand CLI and VS Code view that make L2/L3 practical."),
    ("how-it-works.html", "How it works", "Why the compact form saves tokens."),
    ("economics.html", "Does it pay off?", "Faster, cheaper, lighter on context."),
]) + "\n</section>"

NEXT_HOW = cards([
    ("compaction-levels.html", "Compaction levels", "Push the dial further with L2 and L3."),
    ("cheat-sheet.html", "Cheat sheet", "Every mapping, grouped by package."),
    ("economics.html", "Does it pay off?", "What it saves on real code."),
]) + "\n</section>"

NEXT_ECON = cards([
    ("compaction-levels.html", "Compaction levels", "How far to push token savings."),
    ("quickstart.html", "Quickstart", "Get set up in a few steps."),
    ("cheat-sheet.html", "Cheat sheet", "Every long-form to compact mapping."),
]) + "\n</section>"

NEXT_TOOLING = cards([
    ("compaction-levels.html", "Compaction levels", "What the tooling round-trips."),
    ("cheat-sheet.html", "Cheat sheet", "Every helper, grouped by package."),
    ("quickstart.html", "Quickstart", "Get set up in a few steps."),
]) + "\n</section>"


def main():
    page("index.html", "Smoower.Minified", "Compact, ordinary C# that cuts the tokens an AI spends on .NET boilerplate.", "same code, fewer tokens", INDEX + NEXT_INDEX)
    page("quickstart.html", "Smoower.Minified: quickstart", "Get started with Smoower.Minified in a few steps — AI-assisted or by hand.", "getting started", QUICKSTART + NEXT_QUICKSTART)
    page("installation.html", "Smoower.Minified: installation", "Install the packages, drop in the aliases, and point your assistant at the style.", "getting started", INSTALLATION + NEXT_INSTALLATION)
    page("compaction-levels.html", "Smoower.Minified: compaction levels", "L1 aliases, L2 mapped names, L3 packed — choose how far to push token savings.", "concepts", COMPACTION_LEVELS + NEXT_LEVELS)
    page("how-it-works.html", "Smoower.Minified: how it works", "Why the compact form costs fewer tokens, with the same compiled IL.", "concepts", HOW_IT_WORKS + NEXT_HOW)
    page("economics.html", "Smoower.Minified: does it pay off?", "Is compact AI-generated .NET code faster, cheaper, and lighter on context? Measured on real production code.", "concepts", ECONOMICS + NEXT_ECON)
    page("libraries.html", "Smoower.Minified: libraries", "The libraries and packages Smoower.Minified currently supports.", "reference", render_packages())
    page("cheat-sheet.html", "Smoower.Minified: cheat sheet", "Every long-form to compact mapping, grouped by package.", "reference", render_cheatsheet())
    page("tooling.html", "Smoower.Minified: tooling", "The CLI and editor integrations that round-trip the compact style.", "tooling", TOOLING + NEXT_TOOLING)


if __name__ == "__main__":
    main()
