#!/usr/bin/env python3
"""Reliability eval: can Claude emit the Smoower Ultra DSL correctly, and where
does it break?

The DSL/generator don't exist yet - this measures whether Opus, given the DSL
rules in its prompt, produces consistent DSL across 15 tasks (including edge
cases the convention does NOT cover) rather than drifting into invented syntax
or falling back to vanilla C#. No compiler in the loop, so HARD checks are
coarse (used the DSL? avoided vanilla base/attrs? records? partial class?) and
per-task EXPECT checks look for the specific construct each edge case needs. Read
the printed outputs for the nuanced verdict.

    export ANTHROPIC_API_KEY=...
    python bench/fluency.py
"""
import os
import sys
from concurrent.futures import ThreadPoolExecutor

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _tokens import CLAUDE_MODEL, _claude_client

SAMPLES = 2

SPEC = """You generate ASP.NET Core APIs using the Smoower Ultra DSL. Output ONLY code, no prose, no comments.

DTOs are records:
  public record FooIn(...);   // request body shape
  public record FooOut(...);  // response/projection shape

A controller is a partial class with the Crud attribute and a primary constructor declaring its dependencies:
  [Crud<TEntity, TIn, TOut>("route")]
  public partial class FoosController(AppDb db, IValidator<TIn> val, ILogger<FoosController> log);

[Crud<TEntity,TIn,TOut>] generates: GET route/{id} (200/404), GET route (200 list), POST route (validate TIn -> 201/400), PUT route/{id} (200/404/400), DELETE route/{id} (204/404). TOut is projected from TEntity by matching property names.

Limit the action set with Only/Except and the Verbs flags (Read, Create, Update, Delete):
  [Crud<...>("route", Only = Verbs.Read)]
  [Crud<...>("route", Except = Verbs.Delete)]
Non-Id key: [Crud<...>("route", Key = nameof(TEntity.Slug))].

Override a single action by writing it by hand in the class body (the generator then skips that one), in smoower style:
  [HG] public Tr All()=>db.Foos.nt().w(x=>x.Active).s(x=>new FooOut(x.Id,x.Name)).okl();
Add extra (non-CRUD) endpoints the same way - just write the action.

Add a side effect with a partial method (the generated action calls it after the DB write):
  partial void OnCreated(TEntity x) => ...;
  partial void OnUpdated(TEntity x) => ...;
  partial void OnDeleted(TEntity x) => ...;

Never change route templates, JSON property names, or status codes."""


def lc(s):
    return s.lower()


TASKS = [
    ("plain_crud",
     "Notes API at api/notes (Id int, Text string). Full CRUD. Response: Id, Text.",
     [("[Crud<", lambda o: "[Crud<" in o)]),
    ("alt_key",
     "Catalog API at api/skus. Sku is keyed by Code (string), not Id. Fields: Code, Title (string), Price (decimal). "
     "Full CRUD addressed by Code. Response: Code, Title, Price.",
     [("Key/{code}", lambda o: "Key" in o or "{code}" in lc(o))]),
    ("sub_route",
     "Users API at api/users (Id, Name, Email), full CRUD. Also add GET api/users/{id}/profile returning a "
     "UserProfileOut (Bio string, AvatarUrl string).",
     [("profile route", lambda o: "profile" in lc(o)), ("[Crud<", lambda o: "[Crud<" in o)]),
    ("extra_action",
     "Users API at api/users (Id, Name, Email, Active bool), full CRUD. Add POST api/users/{id}/activate that sets "
     "Active=true and returns 204.",
     [("activate", lambda o: "activate" in lc(o))]),
    ("nested_dto",
     "Orders API at api/orders. Order has Id, CustomerId, and a list of lines (each line: ProductId int, Qty int). "
     "Read-only (GET endpoints only). Output: Id, CustomerId, and Lines as a list of {ProductId, Qty}.",
     [("nested/list", lambda o: "List<" in o or "[]" in o or "LineOut" in o), ("read-only", lambda o: "Only" in o)]),
    ("read_only",
     "Read-only currencies at api/currencies. Currency: Id, Code, Name. GET endpoints only. Response: Id, Code, Name.",
     [("Only Read", lambda o: "Only" in o and "Read" in o)]),
    ("no_validator",
     "Tags API at api/tags. Tag: Id, Label. Full CRUD. No validation needed.",
     [("no IValidator", lambda o: "IValidator" not in o)]),
    ("pagination",
     "Articles API at api/articles (Id, Title, Body). Full CRUD. The list endpoint must support paging via query "
     "params page and size (skip/take). Response: Id, Title.",
     [("paging override", lambda o: "page" in lc(o) and ("sk(" in o or "Skip" in o or "tk(" in o or "Take" in o))]),
    ("filter",
     "Tickets API at api/tickets (Id, Subject, Status string). Full CRUD. The list endpoint filters by an optional "
     "?status= query param. Response: Id, Subject, Status.",
     [("filter override", lambda o: "status" in lc(o) and ".w(" in o)]),
    ("multi_override",
     "Books API at api/books (Id, Title, Isbn). Full CRUD. Override GET-by-id to also include the author name from a "
     "joined Author (return Id, Title, AuthorName), and override the list to order by Title.",
     [("AuthorName", lambda o: "AuthorName" in o), ("ordered", lambda o: "ob(" in o or "OrderBy" in o)]),
    ("soft_delete",
     "Customers API at api/customers (Id, Name, IsDeleted bool). Full CRUD, but DELETE should soft-delete "
     "(set IsDeleted=true, save) and return 204, and the list must exclude soft-deleted rows. Response: Id, Name.",
     [("soft delete", lambda o: "IsDeleted" in o and ("public Tr Del" in o or "Delete" in o))]),
    ("auth",
     "Admin API at api/admin/users (Id, Name, Role). Full CRUD. The whole controller requires authorization; POST and "
     "DELETE additionally require the Admin role. Response: Id, Name, Role.",
     [("auth", lambda o: "AUTH" in o or "Authorize" in o)]),
    ("patch",
     "Profiles API at api/profiles (Id, DisplayName, Bio). Support GET, GET/{id}, and PATCH /{id} for partial updates "
     "(only non-null fields). No POST/PUT/DELETE. Response: Id, DisplayName, Bio.",
     [("patch", lambda o: "Patch" in o or "[HPA]" in o)]),
    ("enum",
     "Tasks API at api/tasks (Id, Title, Priority as an enum Priority {Low, Medium, High}). Full CRUD. "
     "Response: Id, Title, Priority.",
     [("enum defined", lambda o: "enum Priority" in o)]),
    ("async_hook",
     "Signups API at api/signups (Id, Email). Create (POST) and read (GET, GET/{id}) only. After creating a signup, "
     "send a welcome email via await mailer.SendWelcomeAsync(x.Email), where mailer is an IMailer dependency. "
     "Response: Id, Email.",
     [("await mailer", lambda o: "await mailer" in o or "SendWelcomeAsync" in o)]),
]

HARD = ("dsl", "no_base", "partial", "records")


def hard_checks(o):
    return {
        "dsl": "[Crud<" in o,
        "no_base": "ControllerBase" not in o and "[ApiController]" not in o and "[Route(" not in o,
        "partial": "partial class" in o,
        "records": "record " in o,
    }


LONGFORMS = ("[HttpGet", "[HttpPost", "[HttpPut", "[HttpDelete", "LogInformation")


def style_drift(o):
    return [lf for lf in LONGFORMS if lf in o]


def gen(prompt):
    r = client.messages.create(
        model=CLAUDE_MODEL, max_tokens=2000, system=SPEC,
        messages=[{"role": "user", "content": prompt}],
    )
    return "".join(b.text for b in r.content if b.type == "text")


client = _claude_client()
if client is None:
    print("need ANTHROPIC_API_KEY (or ANTHROPIC_AUTH_TOKEN) set")
    sys.exit(1)

jobs = [(tid, s, prompt) for tid, prompt, _ in TASKS for s in range(SAMPLES)]
with ThreadPoolExecutor(max_workers=6) as ex:
    outs = list(ex.map(lambda j: (j[0], j[1], gen(j[2])), jobs))
results = {(tid, s): o for tid, s, o in outs}

print(f"model: {CLAUDE_MODEL}   tasks: {len(TASKS)}   samples each: {SAMPLES}\n")
print("SUMMARY (hard checks + per-task expects), per sample\n")
print(f"{'task':14}{'sample':>7}  {'hard':14} {'expects':10} style")
hard_fail = expect_fail = 0
for tid, prompt, expects in TASKS:
    for s in range(SAMPLES):
        o = results[(tid, s)]
        hc = hard_checks(o)
        hbad = [k for k in HARD if not hc[k]]
        ebad = [lbl for lbl, fn in expects if not fn(o)]
        hard_fail += len(hbad) > 0
        expect_fail += len(ebad) > 0
        hard_s = "OK" if not hbad else "FAIL:" + ",".join(hbad)
        exp_s = "OK" if not ebad else "MISS:" + ",".join(ebad)
        drift = ",".join(style_drift(o)) or "-"
        print(f"{tid:14}{s:>7}  {hard_s:14} {exp_s:10} {drift}")

print(f"\nrows with a hard failure: {hard_fail}/{len(TASKS) * SAMPLES}   "
      f"rows missing an expect: {expect_fail}/{len(TASKS) * SAMPLES}")

print("\n" + "=" * 70 + "\nFULL OUTPUTS (sample 0 of each task; sample 1 only if it diverged)\n" + "=" * 70)
for tid, prompt, expects in TASKS:
    o0 = results[(tid, 0)]
    o1 = results[(tid, 1)]
    print(f"\n########## {tid} ##########\n{prompt}\n---------- sample 0 ----------\n{o0}")
    if hard_checks(o0) != hard_checks(o1) or [l for l, f in expects if not f(o1)] != [l for l, f in expects if not f(o0)]:
        print(f"---------- sample 1 (diverged) ----------\n{o1}")
