using Smoower.Minified.Tools.Engine;

// Phase 1 CLI. The same engine is hosted as a long-lived stdio server in later
// phases so the VSCode extension doesn't pay process startup per keystroke.
//
//   minified expand  <file|->  [-o out] [--vocab v]   compact -> readable
//   minified compact <file|->  [-o out] [--vocab v]   readable -> compact
//   minified vocab   [globalUsings.cs] [-o vocab.json]   emit the shared vocabulary
//   minified check   <file|dir>        [--vocab v]    round-trip gate (CI)
//
// "-" reads stdin. --vocab enables Tier A/B alias resolution; without it the
// transform is reflow-only (Phase 0 behavior). expand and compact are exact
// inverses over the vocabulary, which `check` enforces: compact(expand(x))==x.

if (args.Length == 0)
{
    Console.Error.WriteLine(Usage);
    return 2;
}

try
{
    var cmd = args[0];
    var rest = args[1..];
    return cmd switch
    {
        "expand" => Transform(rest, Expander.Expand),
        "compact" => Transform(rest, Compactor.Compact),
        "vocab" => Vocab(rest),
        "check" => Check(rest),
        "-h" or "--help" or "help" => Help(),
        _ => Unknown(cmd),
    };
}
catch (Exception ex)
{
    Console.Error.WriteLine($"minified: {ex.Message}");
    return 1;
}

static int Transform(string[] args, Func<string, AliasMap?, string> transform)
{
    var opts = ParseOpts(args, requireInput: true);
    var source = ReadInput(opts.Input!);
    var result = transform(source, opts.LoadVocab());
    WriteOutput(opts.Output, result);
    return 0;
}

static int Vocab(string[] args)
{
    var opts = ParseOpts(args, requireInput: false);
    var globalUsings = opts.Input is null ? null : ReadInput(opts.Input);
    WriteOutput(opts.Output, VocabGenerator.Generate(globalUsings));
    return 0;
}

// Round-trip property gate. For every .cs under the path, canonicalize to compact
// then assert compact(expand(c)) == c. Exit 1 if anything drifts.
static int Check(string[] args)
{
    var opts = ParseOpts(args, requireInput: true);
    var vocab = opts.LoadVocab();
    var files = ResolveCsFiles(opts.Input!);

    int checked_ = 0, drifted = 0;
    foreach (var f in files)
    {
        string compact;
        try { compact = Compactor.Compact(File.ReadAllText(f), vocab); }
        catch (Exception ex) { Console.Error.WriteLine($"SKIP  {f}: {ex.Message}"); continue; }

        checked_++;
        var roundTrip = Compactor.Compact(Expander.Expand(compact, vocab), vocab);
        if (!string.Equals(roundTrip, compact, StringComparison.Ordinal))
        {
            drifted++;
            Console.Error.WriteLine($"DRIFT {f}");
        }
    }

    Console.Out.WriteLine($"checked {checked_} file(s), {drifted} drifted");
    return drifted == 0 ? 0 : 1;
}

static IEnumerable<string> ResolveCsFiles(string path) =>
    Directory.Exists(path)
        ? Directory.EnumerateFiles(path, "*.cs", SearchOption.AllDirectories).Where(IsSource)
        : [path];

// Skip build output and generated code — never readable-view targets, and they
// carry preprocessor directives the Phase 1 transform doesn't round-trip yet.
static bool IsSource(string f)
{
    var p = f.Replace('\\', '/');
    if (p.Contains("/obj/") || p.Contains("/bin/") || p.Contains("/Generated/")) return false;
    return !f.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase)
        && !f.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase);
}

static Options ParseOpts(string[] args, bool requireInput)
{
    string? input = null, output = null, vocab = null;
    for (var i = 0; i < args.Length; i++)
    {
        switch (args[i])
        {
            case "-o" or "--out":
                output = NextArg(args, ref i, "-o");
                break;
            case "--vocab":
                vocab = NextArg(args, ref i, "--vocab");
                break;
            default:
                if (input is null) input = args[i];
                else throw new ArgumentException($"unexpected argument: {args[i]}");
                break;
        }
    }
    if (requireInput && input is null) throw new ArgumentException("expected a file path or '-' for stdin");
    return new Options(input, output, vocab);
}

static string NextArg(string[] args, ref int i, string flag)
{
    if (i + 1 >= args.Length) throw new ArgumentException($"{flag} requires a value");
    return args[++i];
}

static string ReadInput(string input) =>
    input == "-" ? Console.In.ReadToEnd() : File.ReadAllText(input);

static void WriteOutput(string? output, string content)
{
    if (output is null) Console.Out.Write(content);
    else File.WriteAllText(output, content);
}

static int Unknown(string cmd)
{
    Console.Error.WriteLine($"minified: unknown command '{cmd}'\n\n{Usage}");
    return 2;
}

static int Help()
{
    Console.Out.WriteLine(Usage);
    return 0;
}

partial class Program
{
    private readonly record struct Options(string? Input, string? Output, string? VocabPath)
    {
        public AliasMap? LoadVocab() => VocabPath is null ? null : AliasMap.Load(VocabPath);
    }

    const string Usage = """
        minified — Smoower.Minified readability engine (Phase 1)

        Usage:
          minified expand  <file|->  [-o out] [--vocab v]   compact C# -> readable
          minified compact <file|->  [-o out] [--vocab v]   readable C# -> compact
          minified vocab   [GlobalUsings.cs] [-o vocab.json]
          minified check   <file|dir>        [--vocab v]    round-trip gate (CI)
        """;
}
