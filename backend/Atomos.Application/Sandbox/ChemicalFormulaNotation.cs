namespace Atomos.Application.Sandbox;

public static class ChemicalFormulaNotation
{
    public static string ToCanonical(string formula) => string.Concat(formula.Select(character => character switch
    {
        '₀' => '0',
        '₁' => '1',
        '₂' => '2',
        '₃' => '3',
        '₄' => '4',
        '₅' => '5',
        '₆' => '6',
        '₇' => '7',
        '₈' => '8',
        '₉' => '9',
        _ => character
    }));
}
