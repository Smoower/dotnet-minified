#!/usr/bin/env python3
"""How far does compaction actually pay? The encoding ladder.

Same UsersController behaviour (GET {id} 200/404, GET all, POST create with
validation + structured log 201/400, DELETE {id} 204/404, UserIn record),
encoded five ways from conventional C# down to a maximally cryptic numeric form.

The product question this answers: do the unreadable numeric/packed encodings
actually beat the readable-compact "smoower" form on Claude's *real* tokenizer,
or do their rare byte sequences fragment and give the savings back? The contract
identifiers (api/users, User, UserIn, Id, Name, Email, UserInValidator, the log
template, the status codes) are irreducible - they must appear in every encoding -
so the only thing cryptic forms can delete is structure smoower already deleted.

Variants 3-5 are HYPOTHETICAL (they don't compile - a source generator would
expand them). They exist to price the encoding paradigm, not to ship.

    pip install tiktoken anthropic
    export ANTHROPIC_API_KEY=...        # free count_tokens endpoint
    python bench/encodings.py
"""
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _tokens import (
    CLAUDE_MODEL,
    approx_pct,
    claude_available,
    claude_toks,
    claude_unavailable_reason,
    reduction,
    tiktoken_toks,
)

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))

# 1. Conventional C#, faithful to the sample's behaviour (validation, Produces,
#    structured log, CreatedAtAction). No usings/namespace, to match the others.
VANILLA = r'''[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly AppDb _db;
    private readonly IValidator<UserIn> _val;
    private readonly ILogger<UsersController> _log;
    private readonly Clock _clock;
    public UsersController(AppDb db, IValidator<UserIn> val, ILogger<UsersController> log, Clock clock)
    {
        _db = db; _val = val; _log = log; _clock = clock;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int id)
    {
        var x = await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new { u.Id, u.Name, u.Email })
            .FirstOrDefaultAsync();
        return x == null ? NotFound() : Ok(x);
    }

    [HttpGet]
    public async Task<IActionResult> All()
    {
        var x = await _db.Users
            .AsNoTracking()
            .Select(u => new { u.Id, u.Name, u.Email })
            .ToListAsync();
        return Ok(x);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post(UserIn r)
    {
        var v = await _val.ValidateAsync(r);
        if (!v.IsValid) return BadRequest(v.Errors);
        var x = new User { Name = r.Name, Email = r.Email };
        _db.Users.Add(x);
        await _db.SaveChangesAsync();
        _log.LogInformation("created user {Id} at {At:o}", x.Id, _clock.UtcNow);
        return CreatedAtAction(nameof(Get), new { id = x.Id }, new { x.Id, x.Name, x.Email });
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Del(int id)
    {
        var x = await _db.Users.FindAsync(id);
        if (x == null) return NotFound();
        _db.Users.Remove(x);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
public record UserIn(string Name, string Email);'''


def _read(rel):
    with open(os.path.join(ROOT, rel), encoding="utf-8") as f:
        return f.read()


# 2. The shipping product: readable-compact, valid C#. Strip usings/namespace so
#    it matches the others (just the controller + record).
_sample = _read("samples/Smoower.Minified.SampleApi/Controllers/UsersController.cs")
SMOOWER = _sample[_sample.index("[API"):].strip()

# 3. Source-generator declarative form (screenshot's "attribute" version). A
#    [Crud<>] convention implies the four actions; a generator would expand it.
CRYPTIC_DECL = '''[Api,Route("api/users")]
[Crud<User,UserIn>(Key:Id,Proj:[Id,Name,Email],Validate:UserInValidator,Log:"created user {Id} at {At:o}")]
partial class UsersController;'''

# 4. Packed-string DSL (screenshot's "aggressive AI-only" version). One attribute
#    string carries the whole contract.
CRYPTIC_PACKED = '''[A("r=api/users;e=User;i=UserIn;k=Id;p=Id,Name,Email;v=UserInValidator;l=created user {Id} at {At:o};x=200/404,200,201/400,204/404")]
partial class U;'''

# 5. Maximally numeric: digit keys for every field. Tests the thesis that rare
#    digit/symbol sequences fragment on a BPE tokenizer.
NUMERIC = '''[A("0=api/users;1=User;2=UserIn;3=Id;4=Id,Name,Email;5=UserInValidator;6=created user {Id} at {At:o};7=200/404;8=200;9=201/400;a=204/404")]
partial class U;'''

LADDER = [
    ("vanilla", VANILLA, "conventional C#"),
    ("smoower", SMOOWER, "valid C#, current product"),
    ("cryptic_decl", CRYPTIC_DECL, "hypothetical source-gen [Crud<>] attr"),
    ("cryptic_packed", CRYPTIC_PACKED, "hypothetical packed DSL string"),
    ("numeric", NUMERIC, "hypothetical numeric keys"),
]

use_claude = claude_available()
if use_claude:
    print(f"claude tokenizer: count_tokens / {CLAUDE_MODEL}\n")
else:
    print(f"claude tokenizer: skipped ({claude_unavailable_reason()})\n")

print("encoding ladder - identical UsersController behaviour, escalating compaction")
print("savings vs. conventional C#:\n")

# Measure once, reuse for both baselines.
counts = {}
for name, code, _note in LADDER:
    counts[name] = (tiktoken_toks(code), claude_toks(code) if use_claude else None)

tk_van, cl_van = counts["vanilla"]
if use_claude:
    print(f"{'variant':16}{'claude':>8}{'tiktoken':>10}  note")
else:
    print(f"{'variant':16}{'tiktoken':>10}  note")
for name, _code, note in LADDER:
    tk, cl = counts[name]
    tk_s = "  -  " if name == "vanilla" else approx_pct(reduction(tk_van, tk))
    if use_claude:
        cl_s = "  -  " if name == "vanilla" else approx_pct(reduction(cl_van, cl))
        print(f"{name:16}{cl_s:>8}{tk_s:>10}  {note}")
    else:
        print(f"{name:16}{tk_s:>10}  {note}")

# The product question: does any cryptic form beat plain smoower?
print("\ndoes the cryptic form beat plain smoower? (reduction vs. smoower; + beats it, - is worse)\n")
tk_smo, cl_smo = counts["smoower"]
if use_claude:
    print(f"{'variant':16}{'claude':>8}{'tiktoken':>10}")
else:
    print(f"{'variant':16}{'tiktoken':>10}")
for name, _code, _note in LADDER:
    if name in ("vanilla", "smoower"):
        continue
    tk, cl = counts[name]
    tk_s = approx_pct(reduction(tk_smo, tk))
    if use_claude:
        cl_s = approx_pct(reduction(cl_smo, cl))
        print(f"{name:16}{cl_s:>8}{tk_s:>10}")
    else:
        print(f"{name:16}{tk_s:>10}")

print("\nrounded to the nearest 5%. variants 3-5 are hypothetical (do not compile).")
