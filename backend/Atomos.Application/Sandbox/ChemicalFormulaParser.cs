namespace Atomos.Application.Sandbox;

internal static class ChemicalFormulaParser
{
    internal const int MaxFormulaLength = 50;
    internal const int MaxNestingDepth = 8;

    internal static bool TryParse(string? formula, out Dictionary<string, int> composition)
    {
        composition = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(formula) || formula.Length > MaxFormulaLength)
        {
            return false;
        }

        var index = 0;
        try
        {
            var segmentIndex = 0;
            while (index < formula.Length)
            {
                var multiplier = 1;
                if (segmentIndex > 0 && !TryParsePositiveNumber(formula, ref index, out multiplier))
                {
                    return false;
                }

                var segment = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                if (!TryParseGroup(formula, ref index, 0, false, segment) || segment.Count == 0)
                {
                    return false;
                }

                foreach (var (symbol, count) in segment)
                {
                    composition.TryGetValue(symbol, out var existingCount);
                    composition[symbol] = checked(existingCount + checked(count * multiplier));
                }

                if (index == formula.Length) break;
                if (formula[index] is not ('·' or '.')) return false;
                index++;
                if (index == formula.Length) return false;
                segmentIndex++;
            }

            return composition.Count > 0;
        }
        catch (OverflowException)
        {
            composition.Clear();
            return false;
        }
    }

    private static bool TryParseGroup(
        string formula,
        ref int index,
        int depth,
        bool expectsClosingParenthesis,
        IDictionary<string, int> composition)
    {
        var hasTerm = false;
        while (index < formula.Length)
        {
            var current = formula[index];
            if (current == ')')
            {
                if (!expectsClosingParenthesis || !hasTerm)
                {
                    return false;
                }

                index++;
                return true;
            }

            if (current is '·' or '.')
            {
                return !expectsClosingParenthesis && hasTerm;
            }

            if (current == '(')
            {
                if (depth >= MaxNestingDepth)
                {
                    return false;
                }

                index++;
                var inner = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                if (!TryParseGroup(formula, ref index, depth + 1, true, inner)
                    || !TryParsePositiveNumber(formula, ref index, out var multiplier))
                {
                    return false;
                }

                foreach (var (innerSymbol, count) in inner)
                {
                    composition.TryGetValue(innerSymbol, out var existingCount);
                    composition[innerSymbol] = checked(existingCount + checked(count * multiplier));
                }

                hasTerm = true;
                continue;
            }

            if (current is < 'A' or > 'Z')
            {
                return false;
            }

            var symbolStart = index++;
            if (index < formula.Length && formula[index] is >= 'a' and <= 'z')
            {
                index++;
            }

            var symbol = formula[symbolStart..index];
            if (!TryParsePositiveNumber(formula, ref index, out var atomCount))
            {
                return false;
            }

            composition.TryGetValue(symbol, out var existingAtomCount);
            composition[symbol] = checked(existingAtomCount + atomCount);
            hasTerm = true;
        }

        return !expectsClosingParenthesis && hasTerm;
    }

    private static bool TryParsePositiveNumber(string formula, ref int index, out int value)
    {
        var start = index;
        while (index < formula.Length && formula[index] is >= '0' and <= '9')
        {
            index++;
        }

        if (start == index)
        {
            value = 1;
            return true;
        }

        return int.TryParse(formula.AsSpan(start, index - start), out value) && value > 0;
    }
}
