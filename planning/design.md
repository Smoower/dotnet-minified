# Smoower Ultra — DSL & generator design

The reference the Roslyn source generator is built against. Scope: the **DSL
surface** (what a human/AI authors) and the **generated output** (what compiles).
Grounded in the benchmark results — see [bench/FINDINGS.md](../bench/FINDINGS.md)
for the evidence and [planning/ultra-plan.csv](ultra-plan.csv) for status.

Design rule, proven by the encoding ladder: **stay readable.** The token floor is
the contract; the readable declarative form already sits on it, and cryptic /
numeric encodings tokenize the same or worse on Claude. The DSL is typed C#
(attributes + generics + records) because that's what the model generates
reliably (0/30 vanilla fallback in the fluency eval).

---

## 1. Core model

Three records carry the contract; one attribute + primary constructor carries the
convention:

```csharp
public record UserIn(string Name, string Email);          // request body
public record UserOut(int Id, string Name, string Email); // response / projection

[Crud<User, UserIn, UserOut>("api/users")]
public partial class UsersController(AppDb db, IValidator<UserIn> val, ILogger<UsersController> log);
```

- `TEntity` = the EF entity. `TIn` = request DTO. `TOut` = response/projection DTO.
- The controller is a **`partial class`** with a **primary constructor** declaring
  its dependencies. The generator emits the other `partial` with the actions.
- Dependencies are bound **by type** from the constructor: the `DbContext` is the
  store, `IValidator<TIn>` is validation, `ILogger<>` is logging, anything else
  (e.g. `IEventBus`, `IMailer`) is available to hooks/overrides by name.

## 2. What `[Crud<TEntity, TIn, TOut>]` generates

| Verb | Route | Statuses | Behaviour |
|---|---|---|---|
| GET | `route/{id}` | 200 / 404 | project `TEntity`→`TOut`, `ok1()` |
| GET | `route` | 200 | list of `TOut`, `okl()` |
| POST | `route` | 201 / 400 | validate `TIn` (if a validator is present) → create → `created()` |
| PUT | `route/{id}` | 200 / 404 / 400 | validate → load-or-404 → map `TIn` → save |
| DELETE | `route/{id}` | 204 / 404 | `delById<TEntity>(id)` |

`TOut` is projected from `TEntity` by **matching property names**. Validation is
**auto-discovered**: if `IValidator<TIn>` is in the constructor, POST/PUT validate;
if not, they skip it.

## 3. Knobs (attribute arguments)

```csharp
[Crud<Currency, CurrencyIn, CurrencyOut>("api/currencies", Only = Verbs.Read)]   // GETs only
[Crud<Order, OrderIn, OrderOut>("api/orders", Except = Verbs.Delete)]            // all but DELETE
[Crud<Sku, SkuIn, SkuOut>("api/skus", Key = nameof(Sku.Code))]                   // non-Id key
```

`Verbs` is a `[Flags]` enum: `Read` (both GETs), `Create`, `Update`, `Delete`,
`All`. `Only` and `Except` are mutually exclusive.

## 4. Overriding — three tiers + extra endpoints

The escape hatch, smallest change first. The generator decides what to emit by
**scanning the partial class for what you already declared** (§6 collision rule).

**Tier 0 — pure convention.** Write nothing; the generator makes all the actions.

**Tier 1 — attributes only, body generated.** Declare the action as a partial
method with your attributes; the generator supplies the conventional body. For
decorating a generated verb (auth, caching, extra `[P*]`) without rewriting logic:

```csharp
[Authorize(Roles = "Admin")] public partial Tr Post(UserIn r);   // attrs yours, body generated
```

**Tier 2 — full body.** Write the whole action in smoower style; the generator
skips that verb:

```csharp
[HG] public Tr All() => db.Users.nt().w(x=>x.Active).s(x=>new UserOut(x.Id,x.Name,x.Email)).okl();
```

**Extra (non-CRUD) endpoints.** Just write them in the class body — the generator
only fills CRUD gaps:

```csharp
[HPO("{id}/activate")] public async Tr Activate(int id){ ... }
```

## 5. Hooks — side effects without rewriting the action

Convention **by method presence** (not `partial void`, which can't be async).
The generated action calls the hook after its DB write if the class declares it:

```csharp
void  OnCreated(User x)        => log.inf("created {Id}", x.Id);   // sync
Task  OnUpdatedAsync(User x)   => bus.PublishAsync(x);             // async — generator AWAITS it
```

Recognised hooks: `OnCreated`/`OnUpdated`/`OnDeleted` (sync) and the `…Async`
variants (awaited). The async form is first-class so side effects that need
`await` have a correct home — this closes the silent `await`-drop found in the
eval. The generator warns if a sync hook returns an unawaited `Task`.

## 6. The collision rule (how the generator decides)

For each action in the resolved set (`Only`/`Except` applied):

1. If the class declares a **full method** matching the verb+route → **skip** (Tier 2).
2. Else if it declares a **partial method** matching the verb → emit only the
   **body** for that declaration (Tier 1).
3. Else → emit the full action (Tier 0).

"Matching" = same HTTP verb and route template. Extra endpoints with non-CRUD
routes never collide.

## 7. `[CrudAuth]` — sugar for the dominant override (auth)

Auth is the most common reason to decorate generated actions, so special-case it:

```csharp
[Crud<User, UserIn, UserOut>("api/admin/users")]
[CrudAuth(Verbs.All)]                                    // [Authorize] on every action
[CrudAuth(Verbs.Create | Verbs.Delete, Roles = "Admin")] // + roles on those
public partial class UsersController(AppDb db, IValidator<UserIn> val);
```

Emits `[Authorize]` / `[Authorize(Roles="…")]` on the matching generated verbs.
Anything `[CrudAuth]` can't express falls back to Tier 1.

## 8. Projection rules

`TOut` is generated as `new TOut(x.A, x.B, …)` by matching `TOut`'s constructor
parameter names to `TEntity` properties. **Flat projections only.** Nested shapes
(a list of child DTOs, a joined field) don't match by name → use a Tier 2
override (the eval showed the model does this correctly when the shape is nested,
e.g. `x.Lines.Select(l => new LineOut(...)).ToList()`).

## 9. What it emits (generated vocabulary)

The generated body is ordinary **Smoower.Minified** — the shipped, documented
vocabulary (`nt`, `w`, `s`, `ob`, `lst`, `one`, `db.add`, `db.save`,
`delById<T>`, `ok1`, `okl`, `created`, `[HG]`/`[HPO]`/`[HPU]`/`[HD]`,
`[P200]`…). The skill/system prompt MUST ship this exact vocabulary so
hand-written Tier-2 overrides don't invent names (the eval's main drift:
`.f`→`.id`, `.sv`→`save`, `.o`→`ob`, `.okf`→`ok1`, `[HP]`→`[HPO]`/`[HPA]`). A
**compiler in the loop** (`dotnet build` after generation) catches any leftover
inventions and structural slips in one turn.

## 10. Non-goals

- **No cryptic / numeric encoding.** Rejected — no token gain over this readable
  form, worse fluency (FINDINGS §2).
- **No compression of the contract or business logic.** Routes, JSON names,
  status codes, domain types, and custom logic are irreducible. Ultra only
  deletes framework ceremony.
- **No external DSL file / packed-string attribute.** Both lose tooling and model
  fluency for ~zero token benefit.

## 11. Open decisions (lock before the generator spike)

| # | Decision | Options | Lean |
|---|---|---|---|
| 1 | Default action set | full CRUD / read+create / opt-in | full CRUD, opt out via `Except` |
| 2 | PATCH | not in default; via extra endpoint / a `Verbs.Patch` | extra endpoint for now |
| 3 | Update semantics | PUT replace from `TIn` / PATCH merge | PUT replace; PATCH later |
| 4 | Validator discovery | auto by ctor type / explicit | auto, skip if absent |
| 5 | Error mapping | `#line` to the attribute / generated files | emit real files + `#line` |

## 12. Build order (first spike)

1. Generator: `[Crud<>]` → the four/five actions, flat projection, validation
   auto-wire, **DI happy path end-to-end, compiling + running.**
2. Collision rule (Tier 2 skip) — the escape hatch.
3. Hooks (incl. `…Async`).
4. `Only`/`Except`/`Key`, then Tier 1 partial-method declarations, then
   `[CrudAuth]`.
5. CLI `expand` (eject to full C#) — cheap once the transform exists.
6. VSCode dual-view on top.
