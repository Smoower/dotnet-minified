#!/usr/bin/env python3
"""Real-world before/after on the Individual-Samples files (a production .NET app).

For each original .cs we compare three forms on Claude's real tokenizer:
  - orig          the file as written
  - strip         comments + blank lines removed (the "free" lever smoower style
                  mandates anyway - no aliasing)
  - min           the hand-written .min.cs (full smoower) if one exists

This decomposes the saving into "just drop the XML docs" vs. "smoower aliasing on
top". Files with no .min.cs (the DbContext, the factory) show only orig->strip,
because smoower has no shipped helpers for EF fluent config / DbSet declarations.

    export ANTHROPIC_API_KEY=...
    python bench/realworld.py
"""
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _tokens import CLAUDE_MODEL, approx, claude_toks, claude_available

DIR = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))),
                   "samples", "Individual-Samples")


def read(p):
    with open(p, encoding="utf-8") as f:
        return f.read()


def strip_comments(text):
    out = []
    for line in text.splitlines():
        t = line.strip()
        if not t or t.startswith("//"):
            continue
        out.append(line)
    return "\n".join(out)


if not claude_available():
    print("need ANTHROPIC_API_KEY set")
    sys.exit(1)

files = sorted(f for f in os.listdir(DIR) if f.endswith(".cs") and not f.endswith(".min.cs"))

print(f"real-world token comparison - claude tokenizer ({CLAUDE_MODEL})\n")
print(f"{'file':32}{'orig':>7}{'strip':>8}{'min':>8}   {'strip%':>7} {'min%':>7}")

tot_orig = tot_min = tot_min_orig = 0
for f in files:
    orig = read(os.path.join(DIR, f))
    o = claude_toks(orig)
    s = claude_toks(strip_comments(orig))
    min_path = os.path.join(DIR, f[:-3] + ".min.cs")
    has_min = os.path.exists(min_path)
    m = claude_toks(read(min_path)) if has_min else None

    strip_pct = approx(o, s)
    min_pct = approx(o, m) if has_min else "  -"
    m_disp = str(m) if has_min else "  -"
    print(f"{f:32}{o:>7}{s:>8}{m_disp:>8}   {strip_pct:>7} {min_pct:>7}")

    if has_min:
        tot_orig += o
        tot_min += m
        tot_min_orig += o

print()
if tot_min_orig:
    print(f"files with a .min.cs: orig {tot_min_orig} -> min {tot_min} "
          f"({approx(tot_min_orig, tot_min)} fewer claude tokens)")
print("strip% = comments+blank lines removed only; min% = full smoower.")
