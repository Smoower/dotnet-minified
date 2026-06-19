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
            // Individual-Samples holds real-world input files (and their .min.cs
            // comparisons) for the token benchmark, not canonical compact samples,
            // so they legitimately contain long-form tokens.
            if (file.Contains("Individual-Samples"))
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
