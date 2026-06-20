# Token-lever backlog — code-magic before more DSL/codegen

Explored and measured 2026-06-20, then deferred ("fine for now, we can always
improve"). These are the next batch of token savings to reach for **before** more
DSL or source generators — all valid C#, no codegen.

## Candidates

1. **`useSmoowerErrors()`** — a centralized exception→status mapper for
   `Smoower.Minified.AspNetCore`. Ship a **narrow** default allowlist only:
   `KeyNotFoundException` → 404, `OperationCanceledException` → 499; domain types
   opt-in; everything unmapped → logged 500.
2. **`need()`** — a non-null guard that throws `UnauthorizedAccessException`
   (→ 401 via the mapper). Collapses the `GetCurrentUserId` / `if == null` /
   `Unauthorized` guard. Caveat: this is control-flow-by-exception, so keep it
   optional.
3. **Curated exception-name aliases** (`KNF` / `IOE` / `UAE` / `AE`) as
   documented `global using`s — for the catches we **keep**.
4. **Adopt the already-shipped `nil()`** over `string.IsNullOrWhiteSpace`
   (free, ~13 Claude tokens per use).

## The measured insight (the non-obvious part)

Exception type names are unusually expensive on Claude's tokenizer:
`KeyNotFoundException` is +8 tokens, `UnauthorizedAccessException` +10 — most
CamelCase identifiers flatten, but these do not.

But you **cannot blanket-delete `try`/`catch`**. Distinguish:

- **Status-mapping catches** — narrow type → relocatable to middleware, behavior
  identical. These are the ones the mapper can absorb.
- **Behavioral catches** — cleanup / fallback / log / swallow / compensate →
  must stay.

And broad framework types (`InvalidOperationException`, `ArgumentException`) must
**not** be auto-mapped: a global map turns real bugs into misleading 4xx
responses. So this is a **rules/prompt-level** lever (a judgment encoded in the
skill plus a compiler-in-the-loop), not a codegen transform.

Honest measured saving on a real endpoint: **~18% off the `.min` version** (vs an
unsafe blanket 31%). The 13-point gap is the price of keeping the broad catch —
worth it.

## Related

Builds on the `[Crud<>]` generator and the `whereIf` / `paged` terminators
shipped earlier. Primary references: [`planning/design.md`](design.md),
[`planning/ultra-plan.csv`](ultra-plan.csv), [`bench/FINDINGS.md`](../bench/FINDINGS.md).
