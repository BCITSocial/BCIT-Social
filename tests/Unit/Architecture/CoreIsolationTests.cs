using BCIT_Social.Tests.Support;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BCIT_Social.Tests.Unit.Architecture;

/// <summary>
/// Enforces clean architecture rules using the Roslyn Compiler API (Microsoft.CodeAnalysis.CSharp)
/// so we do not need to split the codebase into multiple class-library .csproj files.
/// </summary>
public class CoreIsolationTests
{
    private static readonly string[] ForbiddenCorePrefixes =
    [
        "Microsoft.EntityFrameworkCore",
        "Microsoft.AspNetCore",
        "Azure.Storage",
        "BCIT_Social.Data",
        "BCIT_Social.Controllers",
    ];

    private static readonly string[] ForbiddenCoreTypeNames =
    [
        "ApplicationDbContext",
        "DbContext",
        "UserManager",
        "SignInManager",
        "Controller",
        "HttpContext",
    ];

    // T25
    [Fact]
    public void CoreAndViewModels_RespectPureArchitectureBoundaries_ViaRoslynSyntaxAnalysis()
    {
        var coreFolder = TestHelpers.WebProjectPath("tests", "Core");
        var viewModelsFolder = TestHelpers.WebProjectPath("ViewModels");

        var coreFiles = Directory.GetFiles(coreFolder, "*.cs", SearchOption.AllDirectories);
        var viewModelFiles = Directory.GetFiles(viewModelsFolder, "*.cs", SearchOption.AllDirectories);

        coreFiles.ShouldNotBeEmpty();
        viewModelFiles.ShouldNotBeEmpty();

        var violations = new List<string>();

        // Rule 1: Pure domain files in tests/Core/ must not use EF Core, ASP.NET, Identity, or Data/Controllers.
        foreach (var file in coreFiles)
        {
            var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(file), path: file);
            var root = tree.GetCompilationUnitRoot();
            var shortName = Path.GetFileName(file);

            foreach (var usingDirective in root.DescendantNodes().OfType<UsingDirectiveSyntax>())
            {
                var ns = usingDirective.Name?.ToString() ?? "";
                if (ForbiddenCorePrefixes.Any(prefix => ns.StartsWith(prefix, StringComparison.Ordinal)))
                {
                    violations.Add($"{shortName}: forbidden using '{ns}' in pure Core layer.");
                }
            }

            foreach (var identifier in root.DescendantNodes().OfType<IdentifierNameSyntax>())
            {
                var name = identifier.Identifier.ValueText;
                if (ForbiddenCoreTypeNames.Contains(name, StringComparer.Ordinal))
                {
                    violations.Add($"{shortName}: forbidden framework type '{name}' referenced in pure Core layer.");
                }
            }
        }

        // Rule 2: ViewModels/ must never depend on EF Core, ApplicationDbContext, or Controllers.
        foreach (var file in viewModelFiles)
        {
            var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(file), path: file);
            var root = tree.GetCompilationUnitRoot();
            var shortName = Path.GetFileName(file);

            foreach (var usingDirective in root.DescendantNodes().OfType<UsingDirectiveSyntax>())
            {
                var ns = usingDirective.Name?.ToString() ?? "";
                if (ns.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal)
                    || ns.StartsWith("BCIT_Social.Data", StringComparison.Ordinal)
                    || ns.StartsWith("BCIT_Social.Controllers", StringComparison.Ordinal))
                {
                    violations.Add($"{shortName}: ViewModel must not depend on data/controller namespace '{ns}'.");
                }
            }
        }

        violations.ShouldBeEmpty(string.Join(Environment.NewLine, violations));
    }
}
