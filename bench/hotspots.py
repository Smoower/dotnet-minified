#!/usr/bin/env python3
"""Where do the tokens still hide? Cost of common .NET constructs vs a compact form.

Reports approximate savings, rounded to the nearest 5%, for the tiktoken proxy
and (when a key is set) Claude's real tokenizer. Run:

    pip install tiktoken anthropic
    export ANTHROPIC_API_KEY=...        # free count_tokens endpoint
    python bench/hotspots.py
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

pairs = [
    # (label, long form, compact candidate)
    ("SaveChanges (sync)", "db.SaveChanges();", "db.saveS();"),
    ("Find (sync)", "db.Users.Find(id)", "db.Users.idS(id)"),
    ("ToList (sync)", ".ToList()", ".lstS()"),
    ("builder boilerplate", "var builder = WebApplication.CreateBuilder(args);", "var b = WebApplication.CreateBuilder(args);"),
    ("AddControllers", "builder.Services.AddControllers();", "b.Services.AddControllers();"),
    ("MapControllers", "app.MapControllers();", "app.MapControllers();"),
    ("MapGet minimal", 'app.MapGet("/u/{id}", (int id) => ...)', 'app.mg("/u/{id}", (int id) => ...)'),
    ("GetSection bind", 'builder.Configuration.GetSection("Db").Get<DbOpts>()', 'cfg.bind<DbOpts>("Db")'),
    ("ProducesResponseType", "[ProducesResponseType(StatusCodes.Status200OK)]", "[P200]"),
    ("JsonSerializer", "JsonSerializer.Serialize(x)", "x.toJson()"),
    ("JsonDeserialize", "JsonSerializer.Deserialize<T>(s)", "s.fromJson<T>()"),
    ("Created result", 'return CreatedAtAction(nameof(Get), new { id = x.Id }, x);', "return ok201(x);"),
    ("typeof check", "if (x is null) return NotFound();", "// folded into ok1()"),
    ("DataAnnotations", "[Required, StringLength(100)] public string Name", "[Req, Len(100)] public string Name"),
]

use_claude = claude_available()
if use_claude:
    print(f"claude tokenizer: count_tokens / {CLAUDE_MODEL}\n")
    print(f"{'construct':24}{'tiktoken':>9}{'claude':>8}")
else:
    print(f"claude tokenizer: skipped ({claude_unavailable_reason()})\n")
    print(f"{'construct':24}{'tiktoken':>9}")

tk_long = tk_short = 0
cl_long = cl_short = 0
cl_corpus_ok = use_claude
for label, a, b in pairs:
    la, lb = tiktoken_toks(a), tiktoken_toks(b)
    tk_long += la
    tk_short += lb
    tk = reduction(la, lb)
    if use_claude:
        ca, cb = claude_toks(a), claude_toks(b)
        if ca is None or cb is None:
            cl_corpus_ok = False
            cl = None
        else:
            cl_long += ca
            cl_short += cb
            cl = reduction(ca, cb)
        print(f"{label:24}{approx_pct(tk):>9}{approx_pct(cl):>8}")
    else:
        print(f"{label:24}{approx_pct(tk):>9}")

tk_total = approx_pct(reduction(tk_long, tk_short))
if use_claude and cl_corpus_ok:
    cl_total = approx_pct(reduction(cl_long, cl_short))
    print(f"{'TOTAL (corpus)':24}{tk_total:>9}{cl_total:>8}")
else:
    print(f"{'TOTAL (corpus)':24}{tk_total:>9}")

print("\nrounded to the nearest 5% on purpose - report the ballpark, not a precise figure.")
