using System.Globalization;

namespace Lab6.ViewModels;

public static class FunctionParser
{
    public static Func<double, double, double> Parse(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("Функция не задана.");

        var parser = new Parser(Tokenize(expression));
        var function = parser.ParseExpression();

        if (parser.Current is not null)
            throw new FormatException($"Некорректный символ: {parser.Current}.");

        return function;
    }

    private static List<string> Tokenize(string expression)
    {
        var tokens = new List<string>();
        int i = 0;

        while (i < expression.Length)
        {
            char c = expression[i];

            if (char.IsWhiteSpace(c))
            {
                i++;
                continue;
            }

            if (char.IsLetter(c))
            {
                int start = i;

                while (i < expression.Length && char.IsLetterOrDigit(expression[i]))
                    i++;

                tokens.Add(expression[start..i].ToLowerInvariant());
                continue;
            }

            if (char.IsDigit(c) || c == '.')
            {
                int start = i;

                while (i < expression.Length &&
                       (char.IsDigit(expression[i]) || expression[i] == '.'))
                {
                    i++;
                }

                tokens.Add(expression[start..i]);
                continue;
            }

            if ("+-*/^(),".Contains(c))
            {
                tokens.Add(c.ToString());
                i++;
                continue;
            }

            throw new FormatException($"Недопустимый символ: {c}.");
        }

        return tokens;
    }

    private sealed class Parser
    {
        private readonly List<string> _tokens;
        private int _position;

        public Parser(List<string> tokens) => _tokens = tokens;

        public string? Current =>
            _position < _tokens.Count ? _tokens[_position] : null;

        public Func<double, double, double> ParseExpression()
        {
            var left = ParseTerm();

            while (Current is "+" or "-")
            {
                string operation = Current!;
                Next();

                var right = ParseTerm();

                var currentLeft = left;
                var currentRight = right;

                left = operation == "+"
                    ? (x, y) => currentLeft(x, y) + currentRight(x, y)
                    : (x, y) => currentLeft(x, y) - currentRight(x, y);
            }

            return left;
        }

        private Func<double, double, double> ParseTerm()
        {
            var left = ParseFactor();

            while (Current is "*" or "/")
            {
                string operation = Current!;
                Next();

                var right = ParseFactor();

                var currentLeft = left;
                var currentRight = right;

                left = operation == "*"
                    ? (x, y) => currentLeft(x, y) * currentRight(x, y)
                    : (x, y) => currentLeft(x, y) / currentRight(x, y);
            }

            return left;
        }

        private Func<double, double, double> ParseFactor()
        {
            var value = ParseUnary();

            if (Current == "^")
            {
                Next();

                var exponent = ParseFactor();

                var currentBase = value;
                var currentExponent = exponent;

                return (x, y) =>
                    Math.Pow(currentBase(x, y), currentExponent(x, y));
            }

            return value;
        }

        private Func<double, double, double> ParseUnary()
        {
            if (Current == "-")
            {
                Next();

                var value = ParseUnary();
                var currentValue = value;

                return (x, y) => -currentValue(x, y);
            }

            if (Current == "+")
            {
                Next();
                return ParseUnary();
            }

            return ParsePrimary();
        }

        private Func<double, double, double> ParsePrimary()
        {
            if (Current is null)
                throw new FormatException("Неожиданный конец выражения.");

            string token = Current!;

            if (token == "(")
            {
                Next();

                var inner = ParseExpression();

                if (Current != ")")
                    throw new FormatException("Ожидалась закрывающая скобка.");

                Next();
                return inner;
            }

            if (double.TryParse(
                    token,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out double number))
            {
                Next();
                return (_, _) => number;
            }

            if (token == "x")
            {
                Next();
                return (x, _) => x;
            }

            if (token == "y")
            {
                Next();
                return (_, y) => y;
            }

            if (token == "pi")
            {
                Next();
                return (_, _) => Math.PI;
            }

            if (token == "e")
            {
                Next();
                return (_, _) => Math.E;
            }

            if (IsFunction(token))
            {
                Next();

                if (Current != "(")
                    throw new FormatException(
                        $"После функции {token} ожидается скобка.");

                Next();

                var argument = ParseExpression();

                if (Current != ")")
                    throw new FormatException("Ожидалась закрывающая скобка.");

                Next();

                var currentArgument = argument;

                return token switch
                {
                    "sin" => (x, y) => Math.Sin(currentArgument(x, y)),
                    "cos" => (x, y) => Math.Cos(currentArgument(x, y)),
                    "tan" => (x, y) => Math.Tan(currentArgument(x, y)),
                    "asin" => (x, y) => Math.Asin(currentArgument(x, y)),
                    "acos" => (x, y) => Math.Acos(currentArgument(x, y)),
                    "atan" => (x, y) => Math.Atan(currentArgument(x, y)),
                    "sqrt" => (x, y) => Math.Sqrt(currentArgument(x, y)),
                    "abs" => (x, y) => Math.Abs(currentArgument(x, y)),
                    "log" => (x, y) => Math.Log(currentArgument(x, y)),
                    "exp" => (x, y) => Math.Exp(currentArgument(x, y)),
                    _ => throw new FormatException(
                        $"Неизвестная функция: {token}.")
                };
            }

            throw new FormatException(
                $"Неизвестный элемент выражения: {token}.");
        }

        private void Next()
        {
            if (_position < _tokens.Count)
                _position++;
        }

        private static bool IsFunction(string token) => token is
            "sin" or "cos" or "tan" or "asin" or "acos" or "atan" or
            "sqrt" or "abs" or "log" or "exp";
    }
}