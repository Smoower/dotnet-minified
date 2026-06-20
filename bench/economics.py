#!/usr/bin/env python3
"""Numbers behind the 'faster / cheaper / less context' claims.

o200k_base proxy. Run: pip install tiktoken; python bench/economics.py
"""
import os
import tiktoken

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
enc = tiktoken.get_encoding("o200k_base")
def t(s): return len(enc.encode(s))
def read(p):
    with open(os.path.join(ROOT, p), encoding="utf-8") as f:
        return f.read()

skill = t(read(".claude/skills/dotnet/SKILL.md"))
sysp = t(read("prompts/system-prompt.md"))

# Controller measurement from bench/tokens.py
VANILLA_OUT = 413
SMOOWER_OUT = 207
saved = VANILLA_OUT - SMOOWER_OUT

print(f"instruction overhead (input, one-time / cacheable):")
print(f"  SKILL.md            {skill:5} tokens")
print(f"  system-prompt.md    {sysp:5} tokens")
print()
print(f"per-controller output: vanilla {VANILLA_OUT}, smoower {SMOOWER_OUT}, saved {saved} ({saved/VANILLA_OUT:.0%})")
print()

# Break-even: instruction is INPUT tokens (price 1x), savings are OUTPUT tokens (price Rx).
# Net token-cost saving after N controllers = N*saved*R - instruction*1  (uncached)
# Break-even N = instruction / (saved * R)
instr = sysp
print("break-even (number of generated controllers to recoup the prompt overhead):")
print(f"{'output:input price ratio':28}{'uncached':>10}{'cached@0.1x':>12}")
for R in (3, 5):
    be = instr / (saved * R)
    be_cached = (instr * 0.1) / (saved * R)
    print(f"{f'  {R}x':28}{be:10.2f}{be_cached:12.2f}")
print()
print("interpretation: <1 means you are ahead before finishing the first controller.")
