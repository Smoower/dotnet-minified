# Smoower Ultra — benchmark findings

Consolidated learnings from the `bench/` suite. All token figures are **Claude's
real tokenizer** (`count_tokens`, `claude-opus-4-8`) unless noted; tiktoken
`o200k_base` is kept only as an offline proxy. Reproduce any of this with the
scripts named below (set `ANTHROPIC_API_KEY` for the Claude columns — the
`count_tokens` endpoint is free).

## 1. The tokenizer reality (`bench/tokens.py`)

- On a CRUD controller, smoower vs. conventional C# saves **~35% on Claude**
  (~40% on the tiktoken proxy). The proxy slightly *oversells* it.
- Claude tokenizes C# at roughly 1.7–1.8× the tiktoken count, but the *savings
  ratios* track closely. Use Claude numbers for any published claim; the proxy
  is directional only.
- Published band is **~35–40%** on a controller, **10–25% project-wide** (most of
  a real app is business logic, which is incompressible — see §4).

## 2. The encoding ladder (`bench/encodings.py`)

Same controller, escalating compaction, raw Claude tokens:

| encoding | Claude tok | vs vanilla | vs smoower |
|---|--:|--:|--:|
| vanilla C# | 923 | — | — |
| smoower (valid C#) | 404 | ~55% | baseline |
| `[Crud<>]` declarative | 90 | ~90% | ~80% |
| packed-string DSL | 88 | ~90% | ~80% |
| numeric/cryptic | 94 | ~90% | ~75% |

**Key results:**
- The big win past smoower comes from the **source-generator convention**
  (deleting implied structure), *not* from cryptic characters.
- Going numeric/cryptic does **not** lower Claude tokens — `numeric` (94) is the
  **worst** of the three short forms; the readable declarative form (90) ties the
  packed string (88) and beats numeric.
- The token floor **is the contract** (route, JSON names, status codes, the
  records). The declarative form already sits on it.
- tiktoken exaggerates the spread between cryptic variants (50→71, ~42%) where
  Claude flattens it (88→94, ~7%). Measuring on Claude is what killed the
  "numeric is cheaper" hypothesis.

**Conclusion:** the unreadable/numeric encoding is **rejected** — same-or-worse
tokens, zero readability, worse model fluency. "Ultra" = smoower (aliases) +
`[Crud<>]` (generator), both readable.

## 3. DSL generation reliability (`bench/fluency.py`)

15 tasks (including edge cases the convention does *not* cover), 2 samples each,
30 generations. No compiler in the loop, so automated checks are coarse —
manual read is authoritative.

**Rock-solid (the part the generator reads):**
- 0/30 fell back to vanilla (`[ApiController]`/`ControllerBase`/`[Route]`).
- Every output used `[Crud<>]`, records, partial class.
- Correct DSL decisions throughout: `Key = nameof(...)`, `Only/Except`,
  dropping the validator when read-only, defining + using enums (even renamed the
  entity `TaskEntity` to dodge the `System.Threading.Tasks.Task` clash),
  override-by-writing-the-action, `On*` hooks, dependency-by-type.

**Where it breaks — all in the hand-written escape-hatch body, not the DSL:**

1. **Invented helper names (cosmetic, fixable).** With only a few helpers shown
   in the test prompt, the model extrapolated wrong names: `.f` (real `.id`),
   `.sv`/`db.Save()` (real `db.save`), `.o` (real `.ob`), `.okf`/`.ok` (real
   `.ok1`), `nf()`/`nc()`/`this.bad()`, `[HP]` (real `[HPO]`/`[HPA]`). Artifact
   of a thin prompt — the shipping skill carries the real vocabulary and a
   compiler catches the rest.
2. **Structural slip (`soft_delete`).** Terminated the class with `;` then wrote
   override methods *outside* the class — invalid C#. Substring checks missed it;
   a compiler would not.
3. **Two genuine design gaps — failed silently:**
   - **Async side effect (`async_hook`).** `partial void` can't `await`, so the
     model wrote `partial void OnCreated(x) => mailer.SendWelcomeAsync(...)` and
     **dropped the `await`** — a fire-and-forget runtime bug that looks fine.
   - **Decorating a generated action (`auth`).** No DSL way to put
     `[Authorize(Roles="Admin")]` on the generated POST/DELETE, so it fully
     re-implemented both actions by hand.

**Verdict:** DSL declaration layer is **green**. Risk has moved to the
escape-hatch body, addressed by: (a) ship the real vocabulary in the skill,
(b) put a compiler in the loop, (c) two convention features —
`partial Task OnCreatedAsync(...)` and per-verb attributes (`[CrudAuth]`).

**Harness limitation:** substring checks can't see structure or semantics (missed
the `soft_delete` structural break and the dropped `await`). The real eval needs
the actual generator so outputs compile-check.
