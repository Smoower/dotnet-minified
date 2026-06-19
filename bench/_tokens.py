#!/usr/bin/env python3
"""Shared tokenizers for the benchmarks.

Two counters, deliberately side by side:

- ``tiktoken_toks`` - tiktoken's o200k_base (GPT-4o) BPE. Fast, offline, but
  NOT Claude's tokenizer. A *proxy*: read the ratios, not the absolutes.
- ``claude_toks`` - Claude's real, model-specific tokenizer via the
  ``count_tokens`` API. Authoritative for what you'll actually be billed.
  Needs ``pip install anthropic`` and ANTHROPIC_API_KEY (or ANTHROPIC_AUTH_TOKEN).
  The endpoint is free. Model defaults to claude-opus-4-8; override with
  BENCH_CLAUDE_MODEL.

Both counters return ``None`` when their backend is unavailable, so callers can
omit a column rather than crash. Note: ``count_tokens`` counts a full user
message, so its numbers include a small fixed per-message overhead (constant
across variants - it cancels out in the vs-baseline ratios).
"""
import functools
import os
import sys

CLAUDE_MODEL = os.environ.get("BENCH_CLAUDE_MODEL", "claude-opus-4-8")

try:
    import tiktoken

    _enc = tiktoken.get_encoding("o200k_base")

    def tiktoken_toks(s: str):
        return len(_enc.encode(s))
except ImportError:
    def tiktoken_toks(s: str):
        return None


@functools.lru_cache(maxsize=1)
def _claude_client():
    try:
        import anthropic
    except ImportError:
        return None
    if not (os.environ.get("ANTHROPIC_API_KEY") or os.environ.get("ANTHROPIC_AUTH_TOKEN")):
        return None
    try:
        return anthropic.Anthropic()
    except Exception:
        return None


def claude_available() -> bool:
    return _claude_client() is not None


def claude_unavailable_reason() -> str:
    try:
        import anthropic  # noqa: F401
    except ImportError:
        return "anthropic SDK not installed (pip install anthropic)"
    if not (os.environ.get("ANTHROPIC_API_KEY") or os.environ.get("ANTHROPIC_AUTH_TOKEN")):
        return "ANTHROPIC_API_KEY / ANTHROPIC_AUTH_TOKEN not set"
    return "client construction failed"


@functools.lru_cache(maxsize=None)
def claude_toks(s: str):
    """Real Claude token count via count_tokens, or None if unavailable.

    Cached so repeated snippets across the bench don't re-hit the API.
    Individual call failures (rate limit, network) print a warning and return
    None for that row rather than aborting the whole run.
    """
    client = _claude_client()
    if client is None:
        return None
    try:
        r = client.messages.count_tokens(
            model=CLAUDE_MODEL,
            messages=[{"role": "user", "content": s}],
        )
        return r.input_tokens
    except Exception as e:  # noqa: BLE001
        print(f"  ! count_tokens failed: {e}", file=sys.stderr)
        return None


def reduction(base, n):
    """Fractional reduction of n vs. base (e.g. 0.41), or None if unavailable."""
    if not base or n is None:
        return None
    return (base - n) / base


def approx_pct(frac):
    """A fraction rounded to the nearest 5%, as '~X%'. None -> 'n/a'.

    Deliberately approximate - report a defensible ballpark, not a precise
    figure that invites nitpicking the proxy or the exact token count.
    """
    if frac is None:
        return "n/a"
    return f"~{round(frac * 100 / 5) * 5}%"


def approx(base, n):
    """Convenience: approximate reduction of n vs. base as '~X%'."""
    return approx_pct(reduction(base, n))
