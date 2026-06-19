# Smoower.Minified

[![CI](https://github.com/smoower/dotnet-minified/actions/workflows/ci.yml/badge.svg)](https://github.com/smoower/dotnet-minified/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Smoower.Minified.AspNetCore.svg?logo=nuget&label=NuGet)](https://www.nuget.org/packages/Smoower.Minified.AspNetCore)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0%20%7C%2010.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com)
[![License: Source-Available](https://img.shields.io/badge/License-Source--Available-red.svg)](LICENSE)

**Your AI pays by the token. ASP.NET Core makes it pay a lot.**

Smoower.Minified is a set of tiny C# libraries that shrink the boilerplate-heavy
parts of a .NET API (controllers, EF Core queries, DI, logging) into short,
stable forms that are still 100% ordinary C#. No source generator, no transpiler,
no magic. Same IL, fewer tokens.

📖 **Full per-library before/after reference (with per-mapping token deltas):
<https://smoower.github.io/dotnet-minified/>**

---

## The bill nobody talks about

Here's a perfectly normal controller action. Count the ceremony:

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> Get(int id)
{
    var x = await _db.Users
        .AsNoTracking()
        .Where(u => u.Id == id)
        .Select(u => new { u.Id, u.Name, u.Email })
        .FirstOrDefaultAsync();
    return x == null ? NotFound() : Ok(x);
}
```

`HttpGet`, `Task<IActionResult>`, `AsNoTracking`, `FirstOrDefaultAsync`,
`NotFound`, `Ok`. None of that is *your* logic. It's the framework tax. And
every time your AI assistant writes, rewrites, or refactors a controller, it pays
that tax again in **output tokens**.

The same action with Smoower.Minified:

```csharp
[HG("{id}")]public Tr Get(int id)=>db.Users.nt().w(x=>x.Id==id).s(x=>new{x.Id,x.Name,x.Email}).ok1();
```

One line. Same behavior. `ok1()` runs the query and returns `200` with the row,
or `404` if it's missing. That's the exact `x == null ? NotFound() : Ok(x)` you
wrote above, folded into the call.

## What that actually saves you

A full CRUD controller (with logging) was measured against the hand-written
equivalent — both with `tiktoken`'s `o200k_base` as an offline proxy and with
Claude's own tokenizer via the free `count_tokens` API. Treat it as a ballpark,
not a hard count: the bench rounds to the nearest 5%, and the figure swings with
how big the controller is and which tokenizer you use.

**The compact version comes out roughly 35-40% smaller in output tokens**
(Claude's real tokenizer measures ~35%, the tiktoken proxy ~40% — reproduce with
`python bench/tokens.py`; set `ANTHROPIC_API_KEY` to add the Claude column).
Three things follow from that, and it's worth being precise about which ones are
real.

The cleanest win is speed. LLMs emit output one token at a time and decode is
sequential, so wall-clock generation time tracks output-token count almost
linearly. Half the tokens means roughly half the time to produce that file.

It's also cheaper, once you clear a small break-even. Output tokens are billed,
and they cost several times more than input (5x across the current Claude
models). The catch: you have to add the rules prompt, about 1,050 tokens of
*input* for the system prompt (the Claude Code skill is a bit larger), paid once.
You earn it back within the first controller or two, and faster once that prompt
is cached.

The third effect is real but second-order. Shorter code burns less context. It
takes up less of the window and gets re-processed as input on every later turn,
so a long session stays cheaper and summarization comes later.

The full arithmetic (the billing model, the measured break-even table, and the
honest caveats like out-of-distribution names, reasoning tokens, and the
tokenizer proxy) is on the docs site: **[Does it pay off?](https://smoower.github.io/dotnet-minified/economics.html)**
(reproduce with `python bench/economics.py`).

## Where it does *not* help (so you can trust the numbers)

No snake oil here. A shortcut only saves cost or time if it
saves **tokens**, and tokens aren't characters.

- `.Where(` to `.w(` saves **zero tokens**. `Where` is already one token, and so
  is `.Select(` to `.s(`.
- `db.SaveChanges()` to `db.saveS()` saves **zero tokens**. The savings on EF
  come entirely from dropping the long `...Async` names (`SaveChangesAsync`
  becomes `save`), which is why async is the unmarked default and sync gets the
  `S` suffix.

The real wins are two things. First, collapsing long PascalCase identifiers that
tokenize into 3 to 5 sub-tokens (`FirstOrDefaultAsync`, `Task<IActionResult>`,
`AddScoped`). Second, the result-fusing terminators that delete whole
`async`/`await`/`return`/`Ok` phrases. The single-letter swaps are mostly there
to keep the style consistent and the code short to read. They're a nice-to-have,
not the headline.

Across a whole project, expect roughly **10 to 25%**. Your business logic,
models, and config don't shrink, only the framework ceremony does.

## Getting started

Two ways in. If you have an AI assistant with tool access (Claude Code, Copilot,
Cursor), let it do the wiring. Otherwise add the packages by hand — it's three
small steps.

### Option A — let your AI set it up

Paste **[prompts/setup-prompt.md](prompts/setup-prompt.md)** to your assistant in
a new or existing repo. It detects the project (or scaffolds a Web API in an
empty repo), installs the packages the project actually needs — or the default
ASP.NET backend set when unsure — writes `GlobalUsings.cs`, adds the compact-style
rules to your `CLAUDE.md` / `.github/copilot-instructions.md`, and builds to
verify. It's safe to re-run; it only adds what's missing.

> **Claude Code** users can skip the paste: the repo ships a skill at
> `.claude/skills/smoower-minified/`, so just ask it to *"set up Smoower.Minified
> in this project."*

### Option B — install by hand

1. Add the packages you need (all `0.1.0`, multi-targeting net8.0 / net9.0 /
   net10.0). The default set for an ASP.NET Core backend:

   ```bash
   dotnet add package Smoower.Minified.Core
   dotnet add package Smoower.Minified.AspNetCore
   dotnet add package Smoower.Minified.EFCore
   dotnet add package Smoower.Minified.Hosting
   dotnet add package Smoower.Minified.Logging
   dotnet add package Smoower.Minified.Validation
   ```

   Or, as `PackageReference` entries — take only what you use (see
   [The packages](#the-packages)):

   ```xml
   <PackageReference Include="Smoower.Minified.AspNetCore" Version="0.1.0" />
   <PackageReference Include="Smoower.Minified.EFCore" Version="0.1.0" />
   ```

2. Drop the imports and aliases into a `GlobalUsings.cs`. Aliases aren't
   transitive across assemblies, so this lives in *your* project (copy from the
   [sample](samples/Smoower.Minified.SampleApi/GlobalUsings.cs)):

   ```csharp
   global using Smoower.Minified.Core;
   global using Smoower.Minified.AspNetCore;
   global using Smoower.Minified.EFCore;
   global using Ctl = Microsoft.AspNetCore.Mvc.ControllerBase;
   global using Res = Microsoft.AspNetCore.Mvc.IActionResult;
   global using Tr  = System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.IActionResult>;
   global using CT  = System.Threading.CancellationToken;
   global using Cfg = Microsoft.Extensions.Configuration.IConfiguration;
   ```

3. Point your AI at the style. **Claude Code:** the repo ships a skill at
   `.claude/skills/smoower-minified/`, so just ask it to use Smoower.Minified.
   **GPT / Copilot / Cursor:** paste [prompts/system-prompt.md](prompts/system-prompt.md)
   as a system prompt or rules file. Short version to drop into any chat:

   > Generate ASP.NET Core / EF Core code using the Smoower.Minified compact
   > helpers (`[API]`/`[HG]`/`[HPO]`, `:Ctl`, `Tr`, `.w`/`.s`/`.nt`/`.lst`/`.one`,
   > `db.save`/`db.add`, `ok1`/`okl`/`okId`/`delById`, `nil()`). Code only, no
   > comments, file-scoped namespaces, primary constructors, records for DTOs.
   > Never change route templates, status codes, or DTO/JSON names.

## The cheat sheet

| Long | Short |
| --- | --- |
| `[ApiController]` `[Route("api/x")]` | `[API]` `[RT("api/x")]` |
| `[HttpGet]` `[HttpPost]` `[HttpDelete]` ... | `[HG]` `[HPO]` `[HD]` ... |
| `: ControllerBase` | `:Ctl` |
| `async Task<IActionResult>` | `Tr` |
| `.Where` `.Select` `.OrderBy` `.AsNoTracking` `.Include` | `.w` `.s` `.ob` `.nt` `.inc` |
| `.ToListAsync` `.FirstOrDefaultAsync` `.CountAsync` | `.lst` `.one` `.cnt` |
| `SaveChangesAsync` `FindAsync` add/update/remove+save | `db.save()` `set.id(k)` `db.add/upd/del` |
| `x==null ? NotFound() : Ok(x)` over a query | `q.ok1()` (`okl`/`okId`/`okAdd`/`delById`) |
| `string.IsNullOrWhiteSpace(s)` | `s.nil()` |
| `LogInformation(...)` | `log.inf(...)` |
| `services.AddScoped<I,T>()` | `svc.scoped<I,T>()` |
| `client.GetFromJsonAsync<T>(url)` | `c.getJson<T>(url)` |

Full rules for humans and agents live in [CLAUDE.md](CLAUDE.md).

## The packages

Pick only what you use. The data and web layers are split so a console worker can
take `EFCore` without dragging in ASP.NET Core.

| Package | What |
| --- | --- |
| `Smoower.Minified.Core` | guards (`nil`/`emp`/`none`) plus base aliases, zero framework deps |
| `Smoower.Minified.AspNetCore` | attributes, MVC aliases, result-fusing terminators |
| `Smoower.Minified.EFCore` | query + write helpers (async default, `S`-suffixed sync) |
| `Smoower.Minified.Http` | `HttpClient` JSON helpers |
| `Smoower.Minified.Redis` | StackExchange.Redis helpers |
| `Smoower.Minified.Logging` | `ILogger` helpers |
| `Smoower.Minified.Hosting` | DI registration helpers |

## The one rule: don't compact the contract

This changes how the code is *written*, never what it *does* at runtime. Keep
route templates, HTTP verbs, status codes, and DTO property/JSON names exactly as
your API requires. Shortening those silently breaks clients. Shorten the code,
not the contract.

## What's next

The same hotspot analysis (`python bench/hotspots.py`) points at the next big
token sinks worth packaging:

- **201/Created results.** `CreatedAtAction(nameof(Get), new { id = x.Id }, x)`
  is heavy; a fused `created(x)` cuts it by roughly 70%.
- **`[ProducesResponseType(...)]`.** Short attribute forms (`[P200]`, `[P404]`)
  save around 70% each, and they stack up on documented APIs.
- **Config binding.** `Configuration.GetSection("X").Get<T>()` becomes `cfg.bind<T>("X")`.
- **JSON.** `JsonSerializer.Serialize/Deserialize` shorteners.

What's *not* worth it: minimal-hosting boilerplate (`WebApplication.CreateBuilder`,
`AddControllers`, `MapControllers`) measures at roughly 0 token savings, because
those are already cheap, common tokens. Shorteners that don't earn their keep
won't ship.

## Docs site

The browsable reference at <https://smoower.github.io/dotnet-minified/> is a
static site under [`docs/`](docs/), generated by [`docs/build.py`](docs/build.py),
which is the single source of truth for every mapping (re-run it after changing a
helper). It deploys automatically via [`.github/workflows/pages.yml`](.github/workflows/pages.yml).
Enable it once under **Settings → Pages → Source: GitHub Actions**.
