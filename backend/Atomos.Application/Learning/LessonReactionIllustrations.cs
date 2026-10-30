using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Atomos.Application.Sandbox;

namespace Atomos.Application.Learning;

public sealed record LessonReactionIllustration(string Equation, bool IsHeated, string? Note);

public static partial class LessonReactionIllustrations
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static IReadOnlyList<LessonReactionIllustration> Deserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Array) return [];

            var illustrations = new List<LessonReactionIllustration>();
            foreach (var element in document.RootElement.EnumerateArray())
            {
                try
                {
                    var illustration = element.Deserialize<LessonReactionIllustration>(JsonOptions);
                    if (illustration is not null && TryNormalize(illustration, out var normalized, out _))
                    {
                        illustrations.Add(normalized);
                    }
                }
                catch (JsonException)
                {
                    // A damaged entry must not hide valid sibling illustrations.
                }
            }

            return illustrations;
        }
        catch (JsonException)
        {
            return [];
        }
    }

    public static bool TrySerialize(IEnumerable<LessonReactionIllustration>? illustrations, out string? json, out string? error)
    {
        var normalized = Normalize(illustrations, out error);
        if (error is not null)
        {
            json = null;
            return false;
        }

        json = normalized.Count == 0 ? null : JsonSerializer.Serialize(normalized, JsonOptions);
        return true;
    }

    public static bool TryCreateSimulationRequest(
        LessonReactionIllustration illustration,
        out SandboxStoichiometryRequest request)
    {
        request = new SandboxStoichiometryRequest();
        var equation = NormalizeArrow(illustration.Equation);
        var arrowIndex = equation.IndexOf("->", StringComparison.Ordinal);
        if (arrowIndex <= 0 || arrowIndex >= equation.Length - 2) return false;

        var reactants = equation[..arrowIndex]
            .Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(ParseReactant)
            .ToList();
        if (reactants.Count == 0 || reactants.Any(item => item is null)) return false;

        request = new SandboxStoichiometryRequest
        {
            IsHeated = illustration.IsHeated,
            MaxSteps = 1,
            // Equation coefficients are stoichiometric mole ratios, never pressure values.
            Substances = reactants.Select(item => new SandboxSubstanceInput
            {
                Formula = item!.Value.Formula,
                Amount = item.Value.Coefficient,
                Unit = "mol"
            }).ToList()
        };
        return true;
    }

    private static IReadOnlyList<LessonReactionIllustration> Normalize(
        IEnumerable<LessonReactionIllustration>? illustrations,
        out string? error)
    {
        error = null;
        var normalized = new List<LessonReactionIllustration>();
        foreach (var illustration in illustrations ?? [])
        {
            if (!TryNormalize(illustration, out var item, out error)) return [];
            normalized.Add(item);
        }

        return normalized;
    }

    private static bool TryNormalize(
        LessonReactionIllustration illustration,
        out LessonReactionIllustration normalized,
        out string? error)
    {
        var equation = illustration.Equation?.Trim() ?? string.Empty;
        if (equation.Length == 0)
        {
            normalized = illustration;
            error = "Phương trình minh họa không được để trống.";
            return false;
        }

        if (equation.Length > 500)
        {
            normalized = illustration;
            error = "Phương trình minh họa không được vượt quá 500 ký tự.";
            return false;
        }

        var note = string.IsNullOrWhiteSpace(illustration.Note) ? null : illustration.Note.Trim();
        if (note?.Length > 1000)
        {
            normalized = illustration;
            error = "Ghi chú minh họa không được vượt quá 1000 ký tự.";
            return false;
        }

        normalized = new LessonReactionIllustration(equation, illustration.IsHeated, note);
        if (!TryCreateSimulationRequest(normalized, out _))
        {
            error = "Phương trình minh họa không đúng định dạng.";
            return false;
        }

        error = null;
        return true;
    }

    private static string NormalizeArrow(string equation) => equation.Trim()
        .Replace("⟶", "->", StringComparison.Ordinal)
        .Replace("⟹", "->", StringComparison.Ordinal)
        .Replace("⇒", "->", StringComparison.Ordinal)
        .Replace("→", "->", StringComparison.Ordinal)
        .Replace("⇌", "->", StringComparison.Ordinal)
        .Replace("=", "->", StringComparison.Ordinal);

    private static (string Formula, decimal Coefficient)? ParseReactant(string value)
    {
        var match = ReactantPattern().Match(ChemicalFormulaNotation.ToCanonical(value));
        if (!match.Success) return null;

        var coefficient = match.Groups[1].Success
            && decimal.TryParse(match.Groups[1].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : 1m;
        var formula = match.Groups[2].Value;
        return coefficient > 0 && ChemicalFormulaParser.TryParse(formula, out _)
            ? (formula, coefficient)
            : null;
    }

    [GeneratedRegex(@"^\s*(?:(\d+(?:\.\d+)?)\s*)?([A-Za-z][A-Za-z0-9()\[\]·.]*)\s*$", RegexOptions.CultureInvariant)]
    private static partial Regex ReactantPattern();
}
