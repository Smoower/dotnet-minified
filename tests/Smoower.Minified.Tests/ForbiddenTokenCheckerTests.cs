using Xunit;

namespace Smoower.Minified.Tests;

// Coarse style guard: scans the sample API source for long-form tokens that the
// compact Smoower.Minified style replaces. It is a text scan, not a parser, so
// false positives are acceptable; GlobalUsings.cs (where the long type names are
// aliased on purpose) is skipped.
public class ForbiddenTokenCheckerTests
{
    private static readonly string[] Forbidden =
    [
        "[HttpGet", "[HttpPost", "[HttpPut", "[HttpPatch", "[HttpDelete",
        "[Route(", "[ApiController", ": ControllerBase", ":ControllerBase",
        "IActionResult", "ActionResult",
        ".Where(", ".Select(", ".ToListAsync(", ".FirstOrDefaultAsync(",
        ".SaveChangesAsync(", "///", "#region", "#endregion",
    ];

    public static IEnumerable<object[]> SampleFiles()
    {
        var dir = FindSamplesDir();
        foreach (var file in Directory.EnumerateFiles(dir, "*.cs", SearchOption.AllDirectories))
        {
            if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") ||
                file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
                continue;
            // Compiler-generated output (EmitCompilerGeneratedFiles), e.g. the ASP.NET
            // PublicTopLevelProgram file and our own [Crud<>] expansion. Build artifacts,
            // not authored samples - same rationale as obj/bin above.
            if (file.Contains($"{Path.DirectorySeparatorChar}Generated{Path.DirectorySeparatorChar}"))
                continue;
            // Packed mirrors (*.min.cs) are derived artifacts of the .cs that already
            // passed this scan; GlobalUsings.min.cs legitimately carries the long type
            // names the alias block defines.
            if (file.EndsWith(".min.cs", StringComparison.OrdinalIgnoreCase))
                continue;
            // Individual-Samples holds real-world input files (and their .min.cs
            // comparisons) for the token benchmark, not canonical compact samples,
            // so they legitimately contain long-form tokens.
            if (file.Contains("Individual-Samples"))
                continue;
            // TodoApi is the deliberately-traditional baseline for the compression
            // benchmark (conventional C# we measure against), not a compact sample.
            if (file.Contains($"{Path.DirectorySeparatorChar}TodoApi{Path.DirectorySeparatorChar}"))
                continue;
            if (Path.GetFileName(file).Equals("GlobalUsings.cs", StringComparison.OrdinalIgnoreCase))
                continue;
            yield return [file];
        }
    }

    [Theory]
    [MemberData(nameof(SampleFiles))]
    public void SampleFile_HasNoForbiddenLongFormTokens(string file)
    {
        var text = File.ReadAllText(file);
        var hits = Forbidden.Where(text.Contains).ToArray();
        Assert.True(hits.Length == 0,
            $"{Path.GetFileName(file)} contains forbidden long-form token(s): {string.Join(", ", hits)}");
    }

    private static string FindSamplesDir()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var samples = Path.Combine(dir.FullName, "samples");
            if (Directory.Exists(samples))
                return samples;
            dir = dir.Parent;
        }
        throw new DirectoryNotFoundException("Could not locate the 'samples' directory above the test output folder.");
    }
}
