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

## 4. The full ladder on a real app (`samples/TodoApi`)

A task-management API in **conventional C#** with real business logic (status
state-machine, WIP limits, completion gating, recurrence, dashboard aggregation —
*not* pure CRUD), measured against its smoower + packed `.min` mirrors on Claude:

| form | Claude tok | vs vanilla |
|---|--:|--:|
| traditional C# | 5049 | — |
| smoower (aliases) | 4121 | ~18% |
| packed `.min` | 3785 | ~25% |

- The alias lever is a **ceremony detector**: ~30% on the controller, ~15% on the
  logic-heavy service, **0% on entities/DTOs** (pure contract — nothing to alias).
- Whitespace packing (§5) adds the rest and is the **only** lever that touches the
  contract/data files (16–19% there).
- **~25% end-to-end on a realistic app** — confirms the "10–25% project-wide" band
  with a measurement, not an extrapolation. The ~80% of a pure-CRUD controller is
  the exception; business logic and the contract are the floor.

## 5. Whitespace packing — the `.min` lever

Stripping every newline + indentation (lexically safe; a formatter reverses it):

- **~5%** on already-dense smoower controllers (near the whitespace floor already).
- **15–37%** on conventional, multi-line, commented code (data models, bootstrap,
  the `global using` block). Whole authored SampleApi: **16.8%**.
- ~0.75 Claude tokens per whitespace char — code newlines/indentation are
  token-dense (leading whitespace at line starts tokenizes as its own token).

An **input/storage** lever, not an output lever: the model reads packed code fine
(free), but *generating* packed single-line code likely raises the error rate
(untested) — keep it to packing existing code / cached rules, recover readability
with the VS Code virtual view. Unlike §2's cryptic *characters* (which gave nothing
on Claude), whitespace removal *does* pay; its readability cost is recovered by
deterministic tooling, not by the model reading it.

## 6. Short-naming the domain vocabulary (attribute-decoupled)

The contract floor (§2, §4) is frozen because it's a **cross-boundary promise**
(JSON / DB columns / routes, read by systems that don't have your unpack tool) —
not because it's read. But you can move the boundary name into an attribute (paid
once) and let the C# identifier be a short handle used everywhere: `[JPN("wire")]`
(JSON), `[Col("Column")]` (DB), `global using TT = TodoTask` (type).

Per-use deltas on Claude (`bench`):

- Hot, multi-token wins: `RecurrenceDays` 8→2, `OrganizationId`/`CreatedAt` 6→2,
  `CustomerId` 5→2, `TodoTask` 5→2.
- **No headroom / traps:** `Id`/`Title`/`Status` are already ~2 tok; `Description`→
  `Desc` is **negative**. Measure the delta — "long word" ≠ "many tokens".
- The `C_`/`P_` prefix scheme is the worst choice: the underscore fragments
  (`TaskService`→`C_TSvc` is **+2**). Use bare names; `global using` gives
  collision-safety without a prefix.

Economics = **frequency × delta − fixed one-time cost** (the attribute/alias decl).
On a toy (each id used ~2–5×) it is ~breakeven (net 32 tok). Holding the same
identifiers but scaling uses to codebase size: **~3,000 net at 10×, ~9,700 at 30×**
— the one-time cost amortizes to noise. The only lever that keeps paying on the
business-logic / contract floor as a codebase grows.

**Rules (the boundaries that held):**
- Target **hot + positive-delta** identifiers only.
- Boundary names live in compilable carriers (`[Col]`/`[JPN]`/`global using`) —
  those *are* the map; a `.map` sidecar is only for internal names with no carrier.
- **Never `global using` an enum type** (obscures switches/`nameof` — bad code).
  Enum *values* can be shortened via `[JsonStringEnumMemberName]`/`[EnumMember]`,
  but the un-aliasable type name dominates the reference, so value-only shortening
  barely pays — low priority.
- Consistency needs the name-map in context → **cache it** (prompt-caching lever).
