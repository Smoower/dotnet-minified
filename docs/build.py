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
    ("Smoower.Minified.EFCore", "EF Core query and write helpers, plus model-configuration helpers for OnModelCreating."),
    ("Smoower.Minified.Extensions", "Helpers over core .NET / BCL types: an injectable Clock, DateTime and TimeSpan shorteners, and environment-variable access."),
    ("Smoower.Minified.Hosting", "Dependency-injection registration and resolution helpers over IServiceCollection / IServiceProvider."),
    ("Smoower.Minified.Logging", "ILogger helpers (inf/wrn/err/dbg), declared on the non-generic base so they apply to ILogger<T> too."),
    ("Smoower.Minified.Http", "HttpClient JSON helpers over System.Net.Http.Json."),
    ("Smoower.Minified.Validation", "Compact FluentValidation rule shorteners via MiniValidator<T>."),
    ("Smoower.Minified.Json", "System.Text.Json round-trip helpers. A Newtonsoft variant ships the identical surface."),
    ("Smoower.Minified.Redis", "StackExchange.Redis helpers on IDatabase, with JSON value helpers."),
    ("Smoower.Minified.Dapper", "Compact Dapper helpers on IDbConnection for projects not using EF Core."),
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
]


# ── Rendering ───────────────────────────────────────────────────────────────
NAV = [
    ("index.html", "Overview"),
    ("getting-started.html", "Getting started"),
    ("libraries.html", "Libraries"),
    ("economics.html", "Does it pay off?"),
    ("cheat-sheet.html", "Cheat sheet"),
]


def render_nav(current):
    out = []
    for href, label in NAV:
        cls = ' class="active"' if href == current else ""
        out.append(f'<a href="{href}"{cls}>{esc(label)}</a>')
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
<html lang="en">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>{title}</title>
<meta name="description" content="{desc}">
<link rel="stylesheet" href="styles.css">
</head>
<body>
<div class="layout">
<aside class="sidebar">
  <div class="brand"><b>Smoower.Minified</b><span>{subtitle}</span></div>
  <nav>
{nav}
  </nav>
</aside>
<main class="content" id="top">
{content}
<footer>
  Generated by <code>docs/build.py</code>. Smoower.Minified is source-available under a non-compete license; see <code>LICENSE</code>.
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
OVERVIEW = """<section>
  <h1>Smoower.Minified</h1>
  <p class="tagline">Compact, ordinary C# that cuts the tokens an AI spends on .NET boilerplate.</p>

  <p class="lead">When an AI assistant writes ASP.NET Core or EF Core code, most of what it types is not your logic &mdash; it is framework ceremony. <code>Task&lt;IActionResult&gt;</code>, <code>FirstOrDefaultAsync</code>, <code>AddScoped</code>, attribute noise, the same controller scaffolding again and again. Every token of that is generated afresh on each write, rewrite, and refactor.</p>

  <p class="lead">Smoower.Minified swaps that ceremony for short, stable helpers &mdash; <code>Tr</code>, <code>.one()</code>, <code>scoped&lt;&gt;</code>, <code>[HG]</code>, <code>.ok1()</code> &mdash; that compile to the exact same IL. There is no source generator and no transpiler; it is plain C# extension methods, attributes, and type aliases. The behaviour never changes. Only the number of tokens it takes to write the code does.</p>

  <h2>Why it helps</h2>
  <div class="stat">
    <div class="box"><div class="n hl">Faster</div><div class="l">generation time tracks output length &mdash; less to type, less to wait for</div></div>
    <div class="box"><div class="n hl">Cheaper</div><div class="l">output tokens are billed, and there are simply fewer of them</div></div>
    <div class="box"><div class="n hl">Lighter</div><div class="l">shorter code leaves more room in the context window over a session</div></div>
  </div>

  <h2>The honest trade-off</h2>
  <p class="lead">The compact form is terser than verbose C#, and at a first glance less familiar to a human reader. The assistant also needs a small rules prompt so it knows the helpers. Neither is free &mdash; but both are paid once and earned back quickly, and what remains is still completely ordinary, debuggable C# that any .NET developer can step through. For AI-heavy projects that is a trade most teams will gladly make. For a one-line script edited by hand, it is not.</p>

  <p class="lead">See whether it pays off for your workload on <a href="economics.html">Does it pay off?</a>, or go straight to <a href="getting-started.html">Getting started</a>.</p>

  <div class="callout">
    <strong>The one rule: never compact the contract.</strong> Route templates, HTTP verbs, status codes, and DTO / JSON names stay exactly as your API requires. Smoower changes how code is written, never what it does.
  </div>
</section>"""


GETTING_STARTED = """<section>
  <h1>Getting started</h1>
  <p class="lead">Two ways in. If your assistant has tool access, let it do the wiring; otherwise it is three small steps by hand.</p>

  <h2>Option A &mdash; let your AI set it up</h2>
  <p class="lead">Paste <code>prompts/setup-prompt.md</code> into your assistant in a new or existing repo. It detects the project (or scaffolds a Web API in an empty one), installs the packages it needs, writes <code>GlobalUsings.cs</code>, adds the compact-style rules to your <code>CLAUDE.md</code> / <code>copilot-instructions.md</code>, and builds to verify. It is safe to re-run; it only adds what is missing.</p>
  <div class="callout info"><strong>Claude Code</strong> users can skip the paste &mdash; the repo ships a skill at <code>.claude/skills/smoower-minified/</code>; just ask it to &ldquo;set up Smoower.Minified in this project.&rdquo;</div>

  <h2>Option B &mdash; install by hand</h2>
  <p class="lead">1. Add the packages you need (see <a href="libraries.html">Libraries</a>). The default ASP.NET Core backend set:</p>
  <pre>dotnet add package Smoower.Minified.Core
dotnet add package Smoower.Minified.AspNetCore
dotnet add package Smoower.Minified.EFCore
dotnet add package Smoower.Minified.Hosting</pre>

  <p class="lead">2. Drop the imports and aliases into a <code>GlobalUsings.cs</code> in your project. Aliases are not transitive across assemblies, so they live in your code:</p>
  <pre>global using Smoower.Minified.Core;
global using Smoower.Minified.AspNetCore;
global using Smoower.Minified.EFCore;
global using Ctl = Microsoft.AspNetCore.Mvc.ControllerBase;
global using Res = Microsoft.AspNetCore.Mvc.IActionResult;
global using Tr  = System.Threading.Tasks.Task&lt;Microsoft.AspNetCore.Mvc.IActionResult&gt;;
global using CT  = System.Threading.CancellationToken;</pre>

  <p class="lead">3. Point your assistant at the style. Claude Code uses the bundled skill; GPT, Copilot, and Cursor take <code>prompts/system-prompt.md</code> as a system prompt or rules file.</p>

  <p class="lead">Keep the <a href="cheat-sheet.html">Cheat sheet</a> handy, and that is it.</p>
</section>"""


def render_packages():
    rows = "\n".join(
        f"<tr><td><code>{esc(n)}</code></td><td>{esc(b)}</td></tr>" for n, b in PACKAGES)
    return f"""<section>
  <h1>Supported libraries &amp; packages</h1>
  <p class="lead">Each package is independent &mdash; take only what you use. The data and web layers are split so a console worker can reference <code>EFCore</code> without dragging in ASP.NET Core. Every mapping each package provides is in the <a href="cheat-sheet.html">Cheat sheet</a>.</p>
  <table><thead><tr><th>Package</th><th>What it covers</th></tr></thead><tbody>
{rows}
  </tbody></table>
  <div class="callout">
    <strong>Want to see your favourite library here?</strong> Smoower grows by the libraries its users lean on. If there is a package you would like covered, <a href="https://github.com/smoower/dotnet-minified/issues">open an issue and let us know</a>.
  </div>
</section>"""


ECONOMICS = """<section>
  <h1>Does it pay off?</h1>
  <p class="tagline">Faster, cheaper, lighter on context &mdash; and tried on real production code, not toy samples.</p>

  <p class="lead">The promise is simple: an AI that writes less boilerplate writes faster, costs less, and keeps more of its context window free. The question is how much of that survives contact with real code. To find out, Smoower was applied to a slice of a live .NET application &mdash; a handful of production API controllers and a single sprawling EF Core <code>DbContext</code> of nearly two thousand lines &mdash; and the before and after were measured with the model&rsquo;s own tokenizer.</p>

  <h2>What the real code showed</h2>
  <p class="lead">The compact form came out meaningfully smaller everywhere it touched framework ceremony. Typical API controllers shrank by roughly a third, and the most boilerplate-heavy of them by nearly half. Even the giant <code>DbContext</code> &mdash; almost entirely schema configuration, the least compressible code there is &mdash; came down by about a quarter once the EF Core configuration helpers were applied. Across the whole sample, on untouched real-world code, the saving landed at around a quarter fewer tokens.</p>
  <div class="stat">
    <div class="box"><div class="n hl">a third to a half</div><div class="l">on typical API controllers</div></div>
    <div class="box"><div class="n hl">about a quarter</div><div class="l">across a whole real-world slice, schema config included</div></div>
  </div>

  <h2>Faster &mdash; the strongest claim</h2>
  <p class="lead">Models emit output one token at a time, so the wall-clock time to produce a file tracks its length almost linearly. Halve the ceremony and you roughly halve the time spent streaming it out. The rules prompt is read once, in parallel, and cached &mdash; it barely touches latency. This is the benefit that holds up most cleanly.</p>

  <h2>Cheaper &mdash; after a small, one-time cost</h2>
  <p class="lead">Output tokens are billed, and they cost several times more than input. The compact code emits fewer of them on every file. Against that sits a one-time cost: a short rules prompt the assistant needs in context. It is input rather than output, it is paid once per session, and it caches. On any project that generates more than a file or two, it is recouped quickly &mdash; and everything after is saving.</p>

  <h2>Lighter on context &mdash; real, but second-order</h2>
  <p class="lead">In a long session, everything already written is re-read on every later turn. Code that is a quarter to a half smaller leaves more headroom before the window fills, and makes each subsequent turn a little cheaper. It is the same mechanism as &ldquo;cheaper,&rdquo; compounding over time.</p>

  <h2>Where it does not help &mdash; so the numbers stay honest</h2>
  <p class="lead">Smoower only shortens framework ceremony. Your domain &mdash; entity and property names, business rules, the logic itself &mdash; is the contract, and it does not shrink, nor should it. That is why a whole project lands at a portion rather than the headline figure: the savings are concentrated in controllers, data access, wiring, and configuration, and diluted by everything that is genuinely your own. It is built for output-heavy, multi-file, multi-turn AI work; for a single hand-edited snippet the rules prompt would cost more than it saves. And throughout, the runtime behaviour is identical &mdash; the compiled IL does not change.</p>

  <div class="callout">
    <strong>The bottom line.</strong> For an assistant generating ASP.NET Core and EF Core code across a session, all three benefits are real on real code: faster (the cleanest win), cheaper (after a sub-one-file break-even), and lighter on context (compounding over turns). It is not a trick for one-off edits, and it never changes what your code does.
  </div>
</section>"""


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


def main():
    page("index.html", "Smoower.Minified", "Compact, ordinary C# that cuts the tokens an AI spends on .NET boilerplate.", "overview", OVERVIEW)
    page("getting-started.html", "Smoower.Minified: getting started", "Set up Smoower.Minified: AI-assisted or by hand, plus the prompts to point your assistant at.", "getting started", GETTING_STARTED)
    page("libraries.html", "Smoower.Minified: libraries", "The libraries and packages Smoower.Minified currently supports.", "libraries", render_packages())
    page("economics.html", "Smoower.Minified: does it pay off?", "Is compact AI-generated .NET code faster, cheaper, and lighter on context? Measured on real production code.", "does it pay off?", ECONOMICS)
    page("cheat-sheet.html", "Smoower.Minified: cheat sheet", "Every long-form to compact mapping, grouped by package.", "cheat sheet", render_cheatsheet())


if __name__ == "__main__":
    main()
