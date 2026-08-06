using System.Globalization;

namespace OcctScript.Expressions;

public sealed record ExpressionResult(bool Success, double Value, string Error)
{
    public static ExpressionResult Failed(string error) => new(false, 0, error);
    public static ExpressionResult Succeeded(double value) => new(true, value, string.Empty);
}

public sealed class ExpressionEngine
{
    public ExpressionResult Evaluate(string expression, IReadOnlyDictionary<string, double>? variables = null)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return ExpressionResult.Failed("Expression is empty.");
        }

        try
        {
            var parser = new Parser(expression, variables ?? new Dictionary<string, double>());
            var value = parser.Parse();
            return double.IsFinite(value)
                ? ExpressionResult.Succeeded(value)
                : ExpressionResult.Failed("Expression result is not finite.");
        }
        catch (ExpressionException ex)
        {
            return ExpressionResult.Failed(ex.Message);
        }
    }

    private sealed class Parser(string text, IReadOnlyDictionary<string, double> variables)
    {
        private int position;

        public double Parse()
        {
            var value = ParseExpression();
            SkipWhiteSpace();
            if (position != text.Length)
            {
                throw Error($"Unexpected character '{text[position]}'.");
            }
            return value;
        }

        private double ParseExpression()
        {
            var value = ParseTerm();
            while (true)
            {
                SkipWhiteSpace();
                if (Match('+')) value += ParseTerm();
                else if (Match('-')) value -= ParseTerm();
                else return value;
            }
        }

        private double ParseTerm()
        {
            var value = ParseUnary();
            while (true)
            {
                SkipWhiteSpace();
                if (Match('*')) value *= ParseUnary();
                else if (Match('/'))
                {
                    var divisor = ParseUnary();
                    if (Math.Abs(divisor) < 1e-12) throw Error("Division by zero.");
                    value /= divisor;
                }
                else return value;
            }
        }

        private double ParseUnary()
        {
            SkipWhiteSpace();
            if (Match('+')) return ParseUnary();
            if (Match('-')) return -ParseUnary();
            return ParsePrimary();
        }

        private double ParsePrimary()
        {
            SkipWhiteSpace();
            if (Match('('))
            {
                var value = ParseExpression();
                SkipWhiteSpace();
                if (!Match(')')) throw Error("Missing closing parenthesis.");
                return value;
            }

            if (position < text.Length && (char.IsDigit(text[position]) || text[position] == '.'))
                return ParseNumber();

            if (position < text.Length && (char.IsLetter(text[position]) || text[position] == '_'))
                return ParseIdentifier();

            throw Error("Expected a number, parameter, or parenthesized expression.");
        }

        private double ParseNumber()
        {
            var start = position;
            while (position < text.Length && (char.IsDigit(text[position]) || text[position] is '.' or 'e' or 'E' or '+' or '-'))
            {
                if ((text[position] is '+' or '-') && position > start && text[position - 1] is not ('e' or 'E')) break;
                position++;
            }
            var token = text[start..position];
            if (!double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                throw Error($"Invalid number '{token}'.");
            return value;
        }

        private double ParseIdentifier()
        {
            var start = position++;
            while (position < text.Length && (char.IsLetterOrDigit(text[position]) || text[position] == '_')) position++;
            var name = text[start..position];
            if (name.Equals("PI", StringComparison.OrdinalIgnoreCase)) return Math.PI;
            if (name.Equals("E", StringComparison.OrdinalIgnoreCase)) return Math.E;
            if (!variables.TryGetValue(name, out var value)) throw Error($"Unknown parameter '{name}'.");
            return value;
        }

        private bool Match(char expected)
        {
            if (position >= text.Length || text[position] != expected) return false;
            position++;
            return true;
        }

        private void SkipWhiteSpace()
        {
            while (position < text.Length && char.IsWhiteSpace(text[position])) position++;
        }

        private ExpressionException Error(string message) => new($"{message} Position: {position}.");
    }

    private sealed class ExpressionException(string message) : Exception(message);
}
