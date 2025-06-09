using System.Text.RegularExpressions;

class Program
{
    public static double Evaluate(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression) || !IsValidExpression(expression))
        {
            throw new ArgumentException("Invalid expression.");
        }

        List<string> postfix = ConvertToPostfix(expression);
        return EvaluatePostfix(postfix);
    }

    private static bool IsValidExpression(string expression) => Regex.IsMatch(expression, @"^[\d+\-*/^().\s]+$") && HasBalancedParentheses(expression);

    private static bool IsOperator(char c) => "+-*/^".Contains(c);

    private static int Precedence(char op) =>
        op switch
        {
            '^' => 3,  // Highest precedence for exponentiation
            '*' or '/' => 2,
            '+' or '-' => 1,
            _ => 0
        };

    private static double ApplyOperator(double a, double b, char op) => op switch
    {
        '+' => a + b,
        '-' => a - b,
        '*' => a * b,
        '/' => b == 0 ? throw new DivideByZeroException("Cannot divide by zero.") : a / b,
        '^' => Math.Pow(a, b),
        _ => throw new InvalidOperationException($"Unknown operator: {op}")
    };

    private static double EvaluatePostfix(List<string> postfix)
    {

        Stack<double> stack = new Stack<double>();

        foreach (string token in postfix)
        {
            if (double.TryParse(token, out double number))
            {
                stack.Push(number);
            }
            else
            {
                double b = stack.Pop();
                double a = stack.Pop();
                stack.Push(ApplyOperator(a, b, token[0]));
            }
        }

        return stack.Pop();
    }

    private static List<string> ConvertToPostfix(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression) || !IsValidExpression(expression))
        {
            throw new ArgumentException("Invalid expression.");
        }

        Stack<char> operators = new Stack<char>();
        List<string> output = new List<string>();
        string number = "";
        bool expectUnary = true; // True at start or after '('

        foreach (char c in expression)
        {
            if (char.IsDigit(c) || c == '.')
            {
                number += c;
                expectUnary = false;
            }
            else
            {
                if (!string.IsNullOrEmpty(number))
                {
                    output.Add(number);
                    number = "";
                }

                if (c == '(')
                {
                    operators.Push(c);
                    expectUnary = true;
                }
                else if (c == ')')
                {
                    while (operators.Count > 0 && operators.Peek() != '(')
                    {
                        output.Add(operators.Pop().ToString());
                    }
                    operators.Pop();
                    expectUnary = false;
                }
                else if (c == '-' && expectUnary)
                {
                    output.Add("0"); // Convert unary minus to "0 - X"
                    operators.Push(c);
                }
                else if (IsOperator(c))
                {
                    while (operators.Count > 0 && Precedence(operators.Peek()) >= Precedence(c))
                    {
                        output.Add(operators.Pop().ToString());
                    }
                    operators.Push(c);
                    expectUnary = true;
                }
            }
        }

        if (!string.IsNullOrEmpty(number))
        {
            output.Add(number);
        }

        while (operators.Count > 0)
        {
            output.Add(operators.Pop().ToString());
        }

        return output;
    }

    private static bool HasBalancedParentheses(string expression)
    {
        var count = 0;
        foreach (var c in expression)
        {
            if (count < 0)
            {
                return false;
            }
            count += c == '(' ? 1 : c == ')' ? -1 : 0;
        }
        return count == 0;
    }

    public static void Main(string[] args)
    {
        while (true)
        {
            Console.Write("Enter an arithmetic expression (or type 'exit' to quit): ");
            string? expression = Console.ReadLine()?.Trim();
            if (expression?.ToLower() == "exit")
            {
                break;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(expression))
                {
                    throw new ArgumentException("Expression cannot be null or empty.");
                }

                List<string> postfix = ConvertToPostfix(expression);
                Console.WriteLine($"Postfix notation: {string.Join(" ", postfix)}");

                double result = Evaluate(expression);
                Console.WriteLine($"Result: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
