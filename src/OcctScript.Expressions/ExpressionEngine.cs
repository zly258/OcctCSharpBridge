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
        if (string.IsNullOrWhiteSpace(expression)) return ExpressionResult.Failed("Expression is empty.");
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
            if (position != text.Length) throw Error($"Unexpected character '{text[position]}'.");
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
            var value = ParsePower();
            while (true)
            {
                SkipWhiteSpace();
                if (Match('*')) value *= ParsePower();
                else if (Match('/'))
                {
                    var divisor = ParsePower();
                    if (Math.Abs(divisor) < 1e-12) throw Error("Division by zero.");
                    value /= divisor;
                }
                else return value;
            }
        }

        private double ParsePower()
        {
            var value = ParseUnary();
            SkipWhiteSpace();
            if (Match('^')) value = Math.Pow(value, ParsePower());
            return value;
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
            if (position < text.Length && (char.IsDigit(text[position]) || text[position] == '.')) return ParseNumber();
            if (position < text.Length && (char.IsLetter(text[position]) || text[position] == '_')) return ParseIdentifierOrFunction();
            throw Error("Expected a number, parameter, function, or parenthesized expression.");
        }

        private double ParseNumber()
        {
            var start = position;
            var seenExponent = false;
            while (position < text.Length)
            {
                var current = text[position];
                if (char.IsDigit(current) || current == '.') { position++; continue; }
                if ((current == 'e' || current == 'E') && !seenExponent)
                {
                    seenExponent = true;
                    position++;
                    if (position < text.Length && text[position] is '+' or '-') position++;
                    continue;
                }
                break;
            }
            var token = text[start..position];
            if (!double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                throw Error($"Invalid number '{token}'.");
            return value;
        }

        private double ParseIdentifierOrFunction()
        {
            var start = position++;
            while (position < text.Length && (char.IsLetterOrDigit(text[position]) || text[position] == '_')) position++;
            var name = text[start..position];
            SkipWhiteSpace();
            if (Match('('))
            {
                var arguments = new List<double>();
                SkipWhiteSpace();
                if (!Match(')'))
                {
                    while (true)
                    {
                        arguments.Add(ParseExpression());
                        SkipWhiteSpace();
                        if (Match(')')) break;
                        if (!Match(',')) throw Error("Expected ',' or ')' in function argument list.");
                    }
                }
                return EvaluateFunction(name, arguments);
            }

            if (name.Equals("PI", StringComparison.OrdinalIgnoreCase)) return Math.PI;
            if (name.Equals("E", StringComparison.OrdinalIgnoreCase)) return Math.E;
            if (variables.TryGetValue(name, out var value)) return value;
            var match = variables.FirstOrDefault(x => string.Equals(x.Key, name, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(match.Key)) return match.Value;
            throw Error($"Unknown parameter '{name}'.");
        }

        private double EvaluateFunction(string name, IReadOnlyList<double> args)
        {
            var key = name.ToLowerInvariant();
            return key switch
            {
                "abs" => One(name, args, Math.Abs),
                "sqrt" => One(name, args, Math.Sqrt),
                "sin" => One(name, args, Math.Sin),
                "cos" => One(name, args, Math.Cos),
                "tan" => One(name, args, Math.Tan),
                "sind" => One(name, args, value => Math.Sin(value * Math.PI / 180.0)),
                "cosd" => One(name, args, value => Math.Cos(value * Math.PI / 180.0)),
                "tand" => One(name, args, value => Math.Tan(value * Math.PI / 180.0)),
                "asin" => One(name, args, Math.Asin),
                "acos" => One(name, args, Math.Acos),
                "atan" => One(name, args, Math.Atan),
                "floor" => One(name, args, Math.Floor),
                "ceil" or "ceiling" => One(name, args, Math.Ceiling),
                "round" => One(name, args, Math.Round),
                "min" => Many(name, args, values => values.Min()),
                "max" => Many(name, args, values => values.Max()),
                "pow" => Two(name, args, Math.Pow),
                "atan2" => Two(name, args, Math.Atan2),
                "clamp" => Three(name, args, Math.Clamp),
                "rad" => One(name, args, value => value * Math.PI / 180.0),
                "deg" => One(name, args, value => value * 180.0 / Math.PI),
                _ => throw Error($"Unknown function '{name}'.")
            };
        }

        private double One(string name, IReadOnlyList<double> args, Func<double, double> function)
        {
            RequireCount(name, args, 1);
            return function(args[0]);
        }

        private double Two(string name, IReadOnlyList<double> args, Func<double, double, double> function)
        {
            RequireCount(name, args, 2);
            return function(args[0], args[1]);
        }

        private double Three(string name, IReadOnlyList<double> args, Func<double, double, double, double> function)
        {
            RequireCount(name, args, 3);
            return function(args[0], args[1], args[2]);
        }

        private double Many(string name, IReadOnlyList<double> args, Func<IEnumerable<double>, double> function)
        {
            if (args.Count == 0) throw Error($"Function '{name}' requires at least one argument.");
            return function(args);
        }

        private void RequireCount(string name, IReadOnlyList<double> args, int count)
        {
            if (args.Count != count) throw Error($"Function '{name}' requires {count} argument(s).");
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
