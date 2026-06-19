#!/usr/bin/env python3
"""Generate docs/index.html from the per-library mapping tables.

Single source of truth for the documentation site. Each mapping row carries a
realistic short call-site and its long-form equivalent; token deltas are computed
with tiktoken's o200k_base (a proxy for Claude's tokenizer - read the deltas, not
absolutes). Re-run after changing any helper:

    pip install tiktoken
    python docs/build.py
"""
import html
import os

import tiktoken

enc = tiktoken.get_encoding("o200k_base")
HERE = os.path.dirname(os.path.abspath(__file__))


def d(short, long):
    return len(enc.encode(long)) - len(enc.encode(short))


# (short, long, note) — note is optional
LIBS = [
    {
        "id": "core",
        "name": "Smoower.Minified.Core",
        "blurb": "Framework-agnostic guards and the base type aliases. Zero web/EF dependency, so every other package can sit on top of it.",
        "deps": "Microsoft.Extensions.Configuration.Abstractions",
        "groups": [
            ("Guards", [
                ("s.nil()", "string.IsNullOrWhiteSpace(s)", ""),
                ("s.emp()", "string.IsNullOrEmpty(s)", ""),
                ("items.none()", "!items.Any()", ""),
            ]),
            ("Aliases", [
                ("CT ct", "CancellationToken ct", "alias"),
                ("Cfg cfg", "IConfiguration cfg", "alias"),
            ]),
        ],
    },
    {
        "id": "aspnetcore",
        "name": "Smoower.Minified.AspNetCore",
        "blurb": "Compact MVC attributes, type aliases, and the result-fusing terminators that turn a controller action into a single expression. Depends on EFCore so the terminators can run queries, and keeps EFCore itself web-free.",
        "deps": "ASP.NET Core (FrameworkReference) · Smoower.Minified.EFCore · Smoower.Minified.Core",
        "groups": [
            ("Attributes", [
                ("[API]", "[ApiController]", ""),
                ('[RT("api/users")]', '[Route("api/users")]', ""),
                ('[HG("{id}")]', '[HttpGet("{id}")]', ""),
                ("[HPO]", "[HttpPost]", ""),
                ("[HPU]", "[HttpPut]", ""),
                ("[HPA]", "[HttpPatch]", ""),
                ('[HD("{id}")]', '[HttpDelete("{id}")]', ""),
                ("[AUTH]", "[Authorize]", ""),
                ("[ANON]", "[AllowAnonymous]", ""),
                ("[FB]", "[FromBody]", ""),
                ("[FR]", "[FromRoute]", ""),
                ("[FQ]", "[FromQuery]", ""),
                ("[FH]", "[FromHeader]", ""),
            ]),
            ("Aliases", [
                (":Ctl", ": ControllerBase", "alias"),
                ("Res", "IActionResult", "alias"),
                ("AR", "ActionResult", "alias (non-generic)"),
                ("Tr", "Task<IActionResult>", "closed-generic alias"),
            ]),
            ("Result-fusing terminators", [
                ("q.ok1()", "var x=await q.FirstOrDefaultAsync();return x==null?NotFound():Ok(x);", "200 row / 404"),
                ("q.okl()", "Ok(await q.ToListAsync())", "200 list"),
                ("q.okc()", "Ok(await q.CountAsync())", "200 count"),
                ("set.okId(id)", "var x=await set.FindAsync(id);return x==null?NotFound():Ok(x);", "200 row / 404"),
                ("db.okAdd(e)", "db.Add(e);await db.SaveChangesAsync();return Ok(e);", "200 entity"),
                ("db.okNew(e)", "db.Add(e);await db.SaveChangesAsync();return CreatedAtAction(nameof(Get), new{id=e.Id}, e);", "add+save, 201"),
                ("value.created()", "CreatedAtAction(nameof(Get), new { id = value.Id }, value)", "201 with body"),
                ("db.delById<User>(id)", "var x=await db.Set<User>().FindAsync(id);if(x==null)return NotFound();db.Remove(x);await db.SaveChangesAsync();return NoContent();", "204 / 404"),
            ]),
            ("Response-type attributes (Swagger)", [
                ("[P200]", "[ProducesResponseType(StatusCodes.Status200OK)]", ""),
                ("[P200<UserDto>]", "[ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]", "generic"),
                ("[P201]", "[ProducesResponseType(StatusCodes.Status201Created)]", ""),
                ("[P400]", "[ProducesResponseType(StatusCodes.Status400BadRequest)]", ""),
                ("[P404]", "[ProducesResponseType(StatusCodes.Status404NotFound)]", ""),
            ]),
        ],
    },
    {
        "id": "efcore",
        "name": "Smoower.Minified.EFCore",
        "blurb": "EF Core query and write shorteners. Predicates and projections take Expression<Func<...>>, so EF Core still translates to SQL. Async is the unmarked default, and sync gets an S suffix.",
        "deps": "Microsoft.EntityFrameworkCore (8.x / 9.x / 10.x per TFM) · Smoower.Minified.Core",
        "groups": [
            ("Query (composition)", [
                ("q.w(x=>x.Id==id)", "q.Where(x=>x.Id==id)", ""),
                ("q.s(x=>x.Name)", "q.Select(x=>x.Name)", ""),
                ("q.ob(x=>x.Name)", "q.OrderBy(x=>x.Name)", ""),
                ("q.obd(x=>x.Name)", "q.OrderByDescending(x=>x.Name)", ""),
                ("q.tb(x=>x.Name)", "q.ThenBy(x=>x.Name)", ""),
                ("q.tbd(x=>x.Name)", "q.ThenByDescending(x=>x.Name)", ""),
                ("q.sk(10)", "q.Skip(10)", ""),
                ("q.tk(10)", "q.Take(10)", ""),
                ("q.nt()", "q.AsNoTracking()", ""),
                ("q.inc(x=>x.Orders)", "q.Include(x=>x.Orders)", ""),
            ]),
            ("Query (async terminators)", [
                ("q.lst()", "q.ToListAsync()", ""),
                ("q.one()", "q.FirstOrDefaultAsync()", ""),
                ("q.single()", "q.SingleOrDefaultAsync()", ""),
                ("q.any()", "q.AnyAsync()", ""),
                ("q.cnt()", "q.CountAsync()", ""),
            ]),
            ("Write (async)", [
                ("set.id(key)", "set.FindAsync(key)", ""),
                ("await db.save()", "await db.SaveChangesAsync()", ""),
                ("await db.add(e)", "db.Add(e);await db.SaveChangesAsync();", "returns e"),
                ("await db.upd(e)", "db.Update(e);await db.SaveChangesAsync();", "returns e"),
                ("await db.del(e)", "db.Remove(e);await db.SaveChangesAsync();", ""),
            ]),
            ("Sync variants (S suffix)", [
                ("q.lstS()", "q.ToList()", "no token win"),
                ("q.oneS()", "q.FirstOrDefault()", "no token win"),
                ("db.saveS()", "db.SaveChanges()", "no token win"),
                ("set.idS(key)", "set.Find(key)", "no token win"),
            ]),
        ],
    },
    {
        "id": "http",
        "name": "Smoower.Minified.Http",
        "blurb": "HttpClient JSON helpers over System.Net.Http.Json.",
        "deps": "(framework only)",
        "groups": [
            ("HttpClient", [
                ("c.getJson<T>(url)", "c.GetFromJsonAsync<T>(url)", ""),
                ("c.postJson(url, body)", "c.PostAsJsonAsync(url, body)", ""),
                ("c.putJson(url, body)", "c.PutAsJsonAsync(url, body)", ""),
                ("c.patchJson(url, body)", "c.PatchAsJsonAsync(url, body)", ""),
                ("c.del(url)", "c.DeleteAsync(url)", ""),
            ]),
        ],
    },
    {
        "id": "redis",
        "name": "Smoower.Minified.Redis",
        "blurb": "StackExchange.Redis helpers on IDatabase, plus JSON value helpers with optional TTL.",
        "deps": "StackExchange.Redis",
        "groups": [
            ("IDatabase", [
                ("db.get(k)", "db.StringGetAsync(k)", ""),
                ("db.set(k, v)", "db.StringSetAsync(k, v)", ""),
                ("db.set(k, v, ttl)", "db.StringSetAsync(k, v, ttl)", ""),
                ("db.del(k)", "db.KeyDeleteAsync(k)", ""),
                ("db.incr(k)", "db.StringIncrementAsync(k)", ""),
                ("db.getJson<T>(k)", "JsonSerializer.Deserialize<T>(await db.StringGetAsync(k))", "get + deserialize"),
                ("db.setJson(k, v)", "db.StringSetAsync(k, JsonSerializer.Serialize(v))", "serialize + set"),
            ]),
        ],
    },
    {
        "id": "logging",
        "name": "Smoower.Minified.Logging",
        "blurb": "ILogger helpers. Declared on the non-generic ILogger base, so they apply to ILogger<T> too (which cannot be aliased).",
        "deps": "Microsoft.Extensions.Logging.Abstractions",
        "groups": [
            ("ILogger", [
                ('log.inf("created {Id}", id)', 'log.LogInformation("created {Id}", id)', ""),
                ('log.wrn("slow {Ms}", ms)', 'log.LogWarning("slow {Ms}", ms)', ""),
                ('log.err("failed {Id}", id)', 'log.LogError("failed {Id}", id)', ""),
                ('log.err(ex, "failed")', 'log.LogError(ex, "failed")', ""),
                ('log.dbg("state {S}", s)', 'log.LogDebug("state {S}", s)', ""),
            ]),
        ],
    },
    {
        "id": "hosting",
        "name": "Smoower.Minified.Hosting",
        "blurb": "Chainable DI registration helpers on IServiceCollection.",
        "deps": "Microsoft.Extensions.DependencyInjection.Abstractions",
        "groups": [
            ("IServiceCollection", [
                ("svc.scoped<Foo>()", "svc.AddScoped<Foo>()", ""),
                ("svc.scoped<IFoo, Foo>()", "svc.AddScoped<IFoo, Foo>()", ""),
                ("svc.single<Bar>()", "svc.AddSingleton<Bar>()", ""),
                ("svc.trans<Baz>()", "svc.AddTransient<Baz>()", ""),
            ]),
        ],
    },
    {
        "id": "validation",
        "name": "Smoower.Minified.Validation",
        "blurb": "Compact FluentValidation rule shorteners. Inherit MiniValidator<T> to get the rule/req entry points, then chain the rule helpers.",
        "deps": "FluentValidation",
        "groups": [
            ("Entry points (on MiniValidator<T>)", [
                ("req(x=>x.Name)", "RuleFor(x=>x.Name).NotEmpty()", "NotEmpty rule"),
                ("rule(x=>x.Age)", "RuleFor(x=>x.Age)", "plain rule"),
            ]),
            ("Rule shorteners", [
                ("r.max(100)", "r.MaximumLength(100)", ""),
                ("r.min(2)", "r.MinimumLength(2)", ""),
                ("r.len(2, 100)", "r.Length(2, 100)", ""),
                ("r.email()", "r.EmailAddress()", ""),
                ("r.matches(pattern)", "r.Matches(pattern)", ""),
                ("r.gt(0)", "r.GreaterThan(0)", ""),
                ("r.lt(10)", "r.LessThan(10)", ""),
                ("r.gte(1)", "r.GreaterThanOrEqualTo(1)", ""),
                ("r.lte(120)", "r.LessThanOrEqualTo(120)", ""),
                ("r.rng(1, 5)", "r.InclusiveBetween(1, 5)", ""),
            ]),
        ],
    },
    {
        "id": "json",
        "name": "Smoower.Minified.Json",
        "blurb": "System.Text.Json round-trip helpers. No extra dependency. For Newtonsoft, the Smoower.Minified.Json.Newtonsoft package has the identical surface in a different namespace - import one.",
        "deps": "(framework only) · Newtonsoft variant: Newtonsoft.Json",
        "groups": [
            ("System.Text.Json", [
                ("x.toJson()", "JsonSerializer.Serialize(x)", ""),
                ("x.toJson(pretty: true)", "JsonSerializer.Serialize(x, new JsonSerializerOptions { WriteIndented = true })", ""),
                ("s.fromJson<T>()", "JsonSerializer.Deserialize<T>(s)", ""),
            ]),
            ("Newtonsoft.Json (same names)", [
                ("x.toJson()", "JsonConvert.SerializeObject(x)", ""),
                ("s.fromJson<T>()", "JsonConvert.DeserializeObject<T>(s)", ""),
            ]),
        ],
    },
    {
        "id": "dapper",
        "name": "Smoower.Minified.Dapper",
        "blurb": "Compact Dapper helpers on IDbConnection, for projects not using EF Core. Thin async wrappers, and you pass Dapper's usual anonymous-type parameter bag.",
        "deps": "Dapper",
        "groups": [
            ("IDbConnection", [
                ("c.q<T>(sql, p)", "c.QueryAsync<T>(sql, p)", "many rows"),
                ("c.q1<T>(sql, p)", "c.QueryFirstOrDefaultAsync<T>(sql, p)", "first or default"),
                ("c.qs<T>(sql, p)", "c.QuerySingleOrDefaultAsync<T>(sql, p)", "single or default"),
                ("c.ex(sql, p)", "c.ExecuteAsync(sql, p)", "rows affected"),
                ("c.scalar<T>(sql, p)", "c.ExecuteScalarAsync<T>(sql, p)", "scalar value"),
            ]),
        ],
    },
]


def esc(s):
    return html.escape(s)


def delta_cell(short, long):
    n = d(short, long)
    cls = "pos" if n > 0 else "zero"
    sign = f"+{n}" if n > 0 else "0"
    return f'<span class="delta {cls}">{sign}</span>'


def render_table(rows):
    out = ['<table><thead><tr><th>Compact</th><th>Long form</th><th>tok&nbsp;&Delta;</th><th>Notes</th></tr></thead><tbody>']
    for short, long, note in rows:
        out.append(
            f'<tr><td class="short"><code>{esc(short)}</code></td>'
            f'<td class="long"><code>{esc(long)}</code></td>'
            f'<td>{delta_cell(short, long)}</td>'
            f'<td>{esc(note)}</td></tr>'
        )
    out.append("</tbody></table>")
    return "\n".join(out)


def render_nav(current):
    # Same-page anchors on the current page; cross-page links otherwise.
    def ix(anchor):
        return f"#{anchor}" if current == "index" else f"index.html#{anchor}"

    out = [f'<a class="group" href="{ix("overview")}">Overview</a>']
    for lib in LIBS:
        short_name = lib["name"].replace("Smoower.Minified.", "")
        out.append(f'<a href="{ix(lib["id"])}"><code>{esc(short_name)}</code></a>')
    econ_href = "#top" if current == "economics" else "economics.html"
    out.append(f'<a class="group" href="{econ_href}">Does it pay off?</a>')
    return "\n".join(out)


def render_libs():
    out = []
    for lib in LIBS:
        out.append(f'<section id="{lib["id"]}">')
        out.append(f'<h2>{esc(lib["name"])}</h2>')
        out.append(f'<p class="lead">{esc(lib["blurb"])}</p>')
        out.append(f'<p class="deps"><strong>Depends on:</strong> {esc(lib["deps"])}</p>')
        out.append(
            f'<pre>&lt;PackageReference Include="{esc(lib["name"])}" Version="0.1.0" /&gt;</pre>'
        )
        for title, rows in lib["groups"]:
            out.append(f"<h3>{esc(title)}</h3>")
            out.append(render_table(rows))
        out.append("</section>")
    return "\n".join(out)


# --- Economics page: measured inputs -------------------------------------
ROOT = os.path.dirname(HERE)


def _read_root(rel):
    with open(os.path.join(ROOT, rel), encoding="utf-8") as f:
        return f.read()


def _toks(s):
    return len(enc.encode(s))


SKILL_T = _toks(_read_root(".claude/skills/smoower-minified/SKILL.md"))
SYSP_T = _toks(_read_root("prompts/system-prompt.md"))
VAN, SMO = 413, 207  # from bench/tokens.py
SAVED = VAN - SMO


def render_economics():
    # Break-even (controllers) to recoup the system prompt: input cost vs output savings.
    rows = []
    for r in (3, 5):
        uncached = SYSP_T / (SAVED * r)
        cached = (SYSP_T * 0.1) / (SAVED * r)
        rows.append(
            f"<tr><td><code>{r}&times;</code></td>"
            f"<td>{uncached:.2f} controllers</td>"
            f"<td>{cached:.2f} controllers</td></tr>"
        )
    breakeven_rows = "\n".join(rows)

    return f"""<section id="economics-top">
  <h1>Does it actually pay off?</h1>
  <p class="tagline">Faster, cheaper, lighter on context. Which of those is real, and by how much?</p>
  <p class="lead">Short answer: yes to all three, but only after you account for the prompt you have to add. Here's the arithmetic, with the honest caveats, so you can decide for your own workload.</p>

  <div class="callout info">
    <strong>Short version.</strong>
    <strong>Faster</strong> is the strong one: generation is decode-bound and sequential, so wall-clock time tracks output-token count almost linearly. ~50% fewer output tokens means roughly ~50% less generation time per file.
    <strong>Cheaper</strong> holds after a tiny break-even: output tokens cost more than input, and the one-time prompt is recouped in under one controller (see below).
    <strong>Less context</strong> is real but second-order: it's "cheaper" applied to every later turn, since shorter code is re-processed as input each turn.
  </div>

  <h2>How an LLM spends your time and money</h2>
  <p class="lead">Two facts drive everything here:</p>
  <ul>
    <li><strong>Pricing is per token, and output costs more than input</strong>, commonly 3 to 5&times; the input rate. So a token you <em>don't</em> have to emit is worth several input tokens.</li>
    <li><strong>Latency is dominated by decode, not prefill.</strong> The model reads your prompt in parallel (prefill, fast), then emits the answer one token at a time (decode, sequential). Generation time ≈ output tokens &times; per-token latency. Input length barely moves it, and with prompt caching, re-sent input is nearly free.</li>
  </ul>
  <p class="lead">Both point the same way: <strong>cutting output tokens is what pays.</strong> Cutting input characters that don't change token count does nothing.</p>

  <h2>The measurement</h2>
  <p class="lead">A full CRUD controller (with a structured log), hand-written vs. Smoower.Minified, same behavior:</p>
  <div class="stat">
    <div class="box"><div class="n hl">~{SAVED / VAN:.0%}</div><div class="l">fewer output tokens on this controller</div></div>
    <div class="box"><div class="n">10-25%</div><div class="l">typical across a whole project</div></div>
  </div>

  <h2>The catch: the prompt isn't free</h2>
  <p class="lead">The model can't emit <code>ok1()</code> or <code>:Ctl</code> unless it knows what they mean. These names aren't in its training data the way <code>FirstOrDefaultAsync</code> is, so you have to add the rules:</p>
  <div class="stat">
    <div class="box"><div class="n">{SKILL_T}</div><div class="l">tokens, Claude skill (<code>SKILL.md</code>)</div></div>
    <div class="box"><div class="n">{SYSP_T}</div><div class="l">tokens, system prompt (<code>system-prompt.md</code>)</div></div>
  </div>
  <p class="lead">Crucially this is <strong>input</strong>, paid <strong>once</strong> per session and <strong>cacheable</strong>, not re-emitted per file like output is. That asymmetry (cheap, one-time, cached input vs. expensive, recurring output) is the whole reason the trade works.</p>

  <h2>Break-even</h2>
  <p class="lead">How many controllers must you generate before the {SYSP_T}-token system prompt pays for itself? Break-even = prompt&nbsp;input&nbsp;tokens ÷ (saved&nbsp;output&nbsp;tokens × output:input&nbsp;price&nbsp;ratio):</p>
  <table>
    <thead><tr><th>output : input price</th><th>uncached</th><th>cached @ 0.1&times;</th></tr></thead>
    <tbody>
{breakeven_rows}
    </tbody>
  </table>
  <p class="lead">In every case you're ahead before finishing the <strong>first</strong> controller (values &lt; 1). Every controller after that is pure savings: the output tokens you didn't emit, times the price ratio. The more code you generate per session, the better it gets.</p>

  <h2>Faster, the strongest claim</h2>
  <p class="lead">Because decode is sequential, the file that is half the tokens takes roughly half the time to stream out. The added prompt lands in prefill (parallel, and cached after the first call), so it barely touches latency. Net: shorter time-to-finished-file, and a snappier feel when iterating.</p>

  <h2>Less context, real but secondary</h2>
  <p class="lead">In a multi-turn session, everything already written stays in the window and is re-processed as input on every subsequent turn. A controller that comes out ~50% smaller leaves more room before you hit the limit (or trigger summarization), and makes each later turn's input cheaper. It's the same mechanism as "cheaper," applied forward in time.</p>

  <h2>Where the claim breaks down</h2>
  <ul>
    <li><strong>The tokenizer here is a proxy.</strong> <code>o200k_base</code> is GPT-4o's, and Claude's differs. The ratios hold qualitatively, the exact numbers won't.</li>
    <li><strong>Unusual names are out-of-distribution.</strong> The model has seen <code>SaveChangesAsync</code> billions of times and <code>save()</code> almost never. With the prompt in context it's reliable, but expect the occasional slip, and a correction turn can cost more than the file saved. Adherence matters, so measure it on your stack.</li>
    <li><strong>Reasoning tokens are unaffected.</strong> On a thinking model, the compact style shrinks the <em>answer</em>, not the hidden reasoning it spends working out the logic.</li>
    <li><strong>Tiny one-off edits don't amortize.</strong> If you generate one three-line snippet and stop, the prompt overhead can exceed the savings. The trade favors output-heavy, multi-file, multi-turn work.</li>
    <li><strong>Character-only shortcuts give none of this.</strong> <code>.Where(</code> → <code>.w(</code> is the same token count, so zero cost or speed benefit. We keep a few for consistency and label them <span class="delta zero">0</span> in the <a href="index.html#overview">reference</a>.</li>
  </ul>

  <div class="callout">
    <strong>Verdict.</strong> For an AI assistant generating ASP.NET Core / EF Core code across a session, all three benefits are real: <strong>faster</strong> (decode-bound, the cleanest win), <strong>cheaper</strong> (after a sub-one-controller break-even), and <strong>lighter on context</strong> (compounding over turns). It is <em>not</em> a win for one-shot tiny edits, and it never changes runtime behavior, only how many tokens the code costs to write.
  </div>

  <p class="lead">Reproduce these numbers: <code>python bench/economics.py</code> and <code>python bench/tokens.py</code>.</p>
</section>"""


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
  Generated by <code>docs/build.py</code>. Token figures via tiktoken <code>o200k_base</code> (proxy for Claude's tokenizer). Smoower.Minified is MIT-licensed.
</footer>
</main>
</div>
</body>
</html>
"""


def page(filename, title, desc, subtitle, current, content):
    out = SHELL.format(
        title=title, desc=desc, subtitle=subtitle,
        nav=render_nav(current), content=content,
    )
    out_path = os.path.join(HERE, filename)
    with open(out_path, "w", encoding="utf-8") as f:
        f.write(out)
    print(f"wrote {out_path} ({len(out)} bytes)")


INDEX_CONTENT = """<section id="overview">
  <h1>Smoower.Minified</h1>
  <p class="tagline">Compact, valid C# that cuts the tokens your AI spends on .NET boilerplate.</p>
  <p class="lead">Every symbol below is an ordinary C# type or extension method, with no source generator and no transpiler. The compiled IL is identical to the long form. The point is fewer <em>output tokens</em> when an LLM writes or rewrites your code: lower cost, faster generation, and less context burned over a session. Whether that actually pays off is worked out on <a href="economics.html">Does it pay off?</a></p>
  <div class="stat">
    <div class="box"><div class="n hl">~50%</div><div class="l">fewer output tokens on a CRUD controller</div></div>
    <div class="box"><div class="n">10-25%</div><div class="l">fewer across a whole project</div></div>
  </div>
  <div class="callout info">
    <strong>How to read the <code>tok&nbsp;&Delta;</code> column.</strong> Tokens saved per use, measured with tiktoken's <code>o200k_base</code> (GPT-4o) as a proxy for Claude's tokenizer, a relative signal rather than gospel. Rows showing <span class="delta zero">0</span> save no tokens (the long name was already a single token), and they're kept for a shorter, consistent style, not for cost. The real wins are the long PascalCase names and the result-fusing terminators.
  </div>
  <div class="callout">
    <strong>Never compact the contract.</strong> Route templates, HTTP verbs, status codes, and DTO property / JSON names must stay exactly as your API requires. This changes how code is written, never what it does at runtime.
  </div>
  <p class="lead">Point your AI at the style: Claude Code uses the skill in <code>.claude/skills/smoower-minified/</code>, and GPT / Copilot / Cursor use <code>prompts/system-prompt.md</code>. Repo: <a href="https://github.com/smoower/dotnet-minified">github.com/smoower/dotnet-minified</a>.</p>
</section>
{libs}"""


def main():
    page(
        "index.html",
        "Smoower.Minified: library reference",
        "Per-library before/after reference for Smoower.Minified: compact ASP.NET Core / EF Core helpers that cut AI output tokens.",
        "library reference",
        "index",
        INDEX_CONTENT.format(libs=render_libs()),
    )
    page(
        "economics.html",
        "Smoower.Minified: does it pay off?",
        "Is compact AI-generated .NET code actually cheaper, faster, and lighter on context? The measured break-even and the honest caveats.",
        "does it pay off?",
        "economics",
        render_economics(),
    )


if __name__ == "__main__":
    main()
