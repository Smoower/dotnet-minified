#!/usr/bin/env python3
"""Token benchmark for Smoower.Minified.

Compares a hand-written conventional ASP.NET Core controller against the
Smoower.Minified sample controller (which folds in the former "Ultra" result
terminators). Both do the SAME work: CRUD + a structured log on create.

Reports two token counts side by side:
- tiktoken o200k_base (GPT-4o) - an offline BPE *proxy*, NOT Claude's tokenizer.
- Claude's real tokenizer (count_tokens API), the authoritative billing count.

The Claude column appears only when the anthropic SDK is installed and an API
key is set; otherwise the tiktoken-only bench runs unchanged. Treat absolute
numbers as illustrative; the *ratios* vs. vanilla are the takeaway.

    pip install tiktoken anthropic
    export ANTHROPIC_API_KEY=...            # free count_tokens endpoint
    python bench/tokens.py
"""
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _tokens import CLAUDE_MODEL, approx, claude_available, claude_toks, claude_unavailable_reason, tiktoken_toks

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))


def read(rel: str) -> str:
    with open(os.path.join(ROOT, rel), encoding="utf-8") as f:
        return f.read()


# Conventional baseline: equivalent behavior to the sample controller.
VANILLA = r'''[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly AppDb _db;
    private readonly ILogger<UsersController> _log;
    private readonly Clock _clock;
    public UsersController(AppDb db, ILogger<UsersController> log, Clock clock)
    {
        _db = db;
        _log = log;
        _clock = clock;
    }

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
    public async Task<IActionResult> Post(UserIn r)
    {
        if (string.IsNullOrWhiteSpace(r.Name))
            return BadRequest();
        var x = new User { Name = r.Name, Email = r.Email };
        _db.Users.Add(x);
        await _db.SaveChangesAsync();
        _log.LogInformation("created user {Id} at {At:o}", x.Id, _clock.UtcNow);
        return Ok(new { x.Id, x.Name, x.Email });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Del(int id)
    {
        var x = await _db.Users.FindAsync(id);
        if (x == null)
            return NotFound();
        _db.Users.Remove(x);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
public record UserIn(string Name, string Email);'''

SMOOWER = read("samples/Smoower.Minified.SampleApi/Controllers/UsersController.cs")

use_claude = claude_available()

if use_claude:
    print(f"claude tokenizer: count_tokens / {CLAUDE_MODEL}")
else:
    print(f"claude tokenizer: skipped ({claude_unavailable_reason()})")
print()
print("smoower vs. vanilla (identical behaviour: CRUD + a structured log)")
print("approximate token savings:")
print()

print(f"  {'tiktoken (proxy)':22} {approx(tiktoken_toks(VANILLA), tiktoken_toks(SMOOWER))}")
if use_claude:
    print(f"  {f'claude ({CLAUDE_MODEL})':22} {approx(claude_toks(VANILLA), claude_toks(SMOOWER))}")
print()
print("rounded to the nearest 5% on purpose - report the ballpark, not a precise figure.")
