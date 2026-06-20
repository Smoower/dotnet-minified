# Contributing to Smoower.Minified

Thanks for wanting to help. Smoower.Minified is a small set of C# libraries that
shrink the boilerplate an AI re-emits when it writes .NET code. Everything here
serves one goal: **fewer output tokens for the same behavior.** This guide is how
to add to it without breaking that promise.

## The two rules everything else follows

1. **Never compact the contract.** Route templates, HTTP verbs, status codes, and
   DTO property / JSON / DB names stay exactly as the API requires. A helper
   changes how code is *written*, never what it *does* at runtime — same IL.
2. **A shortener ships only when it earns its place.** It has to measurably save
   Claude tokens **and** be something the model actually reaches for. We measure
   before we add (see [Measuring tokens](#measuring-tokens)). "It looks shorter"
   isn't enough; a surprising or rarely-used helper costs more than it saves.

If a change can't satisfy both, it doesn't go in — and that's usually the right
call.

## Project layout

- `src/Smoower.Minified.*` — the libraries, one package per surface. Plain C#
  extension methods, attributes, and type aliases; the opt-in `[Crud<>]` source
  generator lives in `Smoower.Minified.Generators`.
- `tests/Smoower.Minified.Tests` — the xUnit suite. It dogfoods
  `Smoower.Minified.Testing`, so tests are written with `[F]`/`[Th]` and the
  fluent assertions (`x.eq(y)`, `x.notNul()`, …).
- `samples/` — authored compact `.cs` files and their packed `.min.cs` mirrors.
- `docs/build.py` — **the single source of truth for the docs and the cheat
  sheet.** Edit the Python, not the generated HTML.
- `bench/` — the token benchmarks and the tokenizers.
- `tools/pack.py` — packs an authored `.cs` into its `.min.cs` whitespace-packed
  mirror.
- `prompts/`, `.claude/skills/dotnet/`, `CLAUDE.md` — the generation
  rules that teach an assistant to emit the compact style. Keep them in sync.
- `.claude-plugin/plugin.json` + `skills/dotnet/SKILL.md` — the Claude
  Code plugin published to the marketplace. The `skills/` copy must stay
  byte-identical to `.claude/skills/dotnet/SKILL.md` (the in-repo dev
  copy); after editing the skill, re-sync it:
  `cp .claude/skills/dotnet/SKILL.md skills/dotnet/SKILL.md`,
  then `claude plugin validate .`.

## Build & test

```bash
dotnet build Smoower.Minified.slnx
dotnet test  Smoower.Minified.slnx
```

The libraries multi-target `net8.0;net9.0;net10.0`, and CI builds and tests all
three in Release. Make sure the suite is green before opening a PR.

## Adding a helper

1. **Put it in the right package**, next to its peers, and follow the naming:
   short, lowercase, mnemonic (`w`, `s`, `ok1`, `notNul`). Async is the unmarked
   default; synchronous variants take an `S` suffix (`saveS`, `oneS`).
2. **Keep it plain C#** — an extension method, attribute, or alias that compiles
   to the same IL as the long form. No behavior change.
3. **Add tests** in `tests/`, written in the compact test style.
4. **Update the docs**: add the mapping to `docs/build.py` (the `LIBS` cheat-sheet
   list, plus `PACKAGES` for a whole new package), then regenerate:
   ```bash
   python docs/build.py
   ```
5. **Update the generation rules** if you added a new surface: `prompts/system-prompt.md`,
   the skill under `.claude/skills/dotnet/`, and `CLAUDE.md`.
6. **Measure it** (next section) and put the numbers in your PR.

The forbidden-token checker (`tests/Smoower.Minified.Tests/ForbiddenTokenCheckerTests.cs`)
scans the samples for long-form tokens the compact style replaces — keep the
samples clean.

## Measuring tokens

The benchmark tokenizers live in `bench/_tokens.py`:

- **tiktoken** (`o200k_base`) — offline, fast, a *proxy*. Read the ratios, not the
  absolutes.
- **Claude's tokenizer** — the authoritative `count_tokens` API (free endpoint).
  Needs `pip install anthropic` and `ANTHROPIC_API_KEY`; the model defaults to
  `claude-opus-4-8` (override with `BENCH_CLAUDE_MODEL`).

Show a before/after on a realistic snippet — the long form vs. the compact form,
and the saving. A helper that doesn't move the number doesn't ship.

## Style

- Authored `.cs` is compact but readable: file-scoped namespaces, primary
  constructors, records for DTOs, no XML docs, no `#region`, minimal blank lines.
- The `.min.cs` mirrors are generated, never hand-edited:
  `python tools/pack.py <file-or-dir>`.

## Issues & PRs

- File issues with the templates: a **bug**, a **library suggestion**, or
  **anything else**.
- PRs use the checklist in the pull-request template. Small, focused PRs land
  fastest.

## Licensing of contributions

Smoower.Minified is **source-available under a non-compete license** (see
[LICENSE](LICENSE)). By submitting a contribution you confirm you have the right
to contribute it and agree it is provided under, and becomes part of the Software
subject to, that same license. If that isn't something you can agree to, please
open an issue to discuss before sending code.

## Be decent

Be respectful and assume good faith. That's the whole code of conduct.
