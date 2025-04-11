//using generate_Grammar.Ecuation;
//using System.Text;

//namespace generate_Grammar
//{
//  //internal class Program
//  //  {
//  //      static void Main(string[] args)
//  //      {
//  //          var parser = new Equation("a*(b^+a+c)^+");
//  //          showEquation(parser.equations, 0);
//  //      }
//  //      static void showEquation(List<Element> equations, int indent)
//  //      {
//  //          for (int i = 0; i < equations.Count(); i++)
//  //          {
//  //              System.Console.Write((indent == 1 ? "    " : "") + "Element " + i + ": ");
//  //              if (equations[i].subElements == null)
//  //              {
//  //                  System.Console.WriteLine(equations[i].alphabet + " to power " + equations[i].power);
//  //              }
//  //              else
//  //              {
//  //                  System.Console.WriteLine("in " + i + ":");
//  //                  showEquation(equations[i].subElements, 1);
//  //                  System.Console.WriteLine(i + " to power " + equations[i].power);

//  //              }
//  //          }
//  //      }
//  //  }

//  // Base class for all expression elements
//  public abstract class Expression
//  {
//    public override string ToString()
//    {
//      return ToStringImpl();
//    }

//    protected abstract string ToStringImpl();
//  }

//  // Represents a simple symbol (like 'a', 'b', 'c')
//  public class Symbol : Expression
//  {
//    public string Name { get; }

//    public Symbol(string name)
//    {
//      Name = name;
//    }

//    protected override string ToStringImpl()
//    {
//      return Name;
//    }
//  }

//  // Represents a postfix operator (like '^+')
//  public class PostfixExpression : Expression
//  {
//    public Expression Base { get; }
//    public string Operator { get; }

//    public PostfixExpression(Expression baseExpr, string op)
//    {
//      Base = baseExpr;
//      Operator = op;
//    }

//    protected override string ToStringImpl()
//    {
//      if (Base is Symbol)
//        return $"{Base}{Operator}";
//      else
//        return $"({Base}){Operator}";
//    }
//  }

//  // Represents a concatenation (like 'b^+a')
//  public class ConcatenationExpression : Expression
//  {
//    public List<Expression> Elements { get; } = new List<Expression>();

//    public ConcatenationExpression(params Expression[] elements)
//    {
//      Elements.AddRange(elements);
//    }

//    public void Add(Expression expr)
//    {
//      Elements.Add(expr);
//    }

//    protected override string ToStringImpl()
//    {
//      StringBuilder sb = new StringBuilder();
//      foreach (var element in Elements)
//      {
//        sb.Append(element.ToString());
//      }
//      return sb.ToString();
//    }
//  }

//  // Represents a sum of expressions (like 'b^+a+c')
//  public class SumExpression : Expression
//  {
//    public List<Expression> Terms { get; } = new List<Expression>();

//    public SumExpression(params Expression[] terms)
//    {
//      Terms.AddRange(terms);
//    }

//    public void Add(Expression term)
//    {
//      Terms.Add(term);
//    }

//    protected override string ToStringImpl()
//    {
//      StringBuilder sb = new StringBuilder();
//      for (int i = 0; i < Terms.Count; i++)
//      {
//        if (i > 0)
//          sb.Append("+");
//        sb.Append(Terms[i].ToString());
//      }
//      return sb.ToString();
//    }
//  }

//  // Main parser class
//  public class Parser
//  {
//    private string _input;
//    private int _position;

//    public Parser(string input)
//    {
//      _input = input;
//      _position = 0;
//    }

//    public Expression Parse()
//    {
//      Expression result = ParseExpression();

//      if (_position < _input.Length)
//      {
//        throw new Exception($"Unexpected characters at position {_position}: {_input.Substring(_position)}");
//      }

//      return result;
//    }

//    private Expression ParseExpression()
//    {
//      Expression left = ParseTerm();

//      while (_position < _input.Length && Peek() == '+')
//      {
//        // If we find a '+' not inside parentheses, it's a sum
//        Consume(); // consume '+'
//        SumExpression sum;

//        if (left is SumExpression)
//          sum = (SumExpression)left;
//        else
//        {
//          sum = new SumExpression();
//          sum.Add(left);
//        }

//        sum.Add(ParseTerm());
//        left = sum;
//      }

//      return left;
//    }

//    private Expression ParseTerm()
//    {
//      Expression result = ParseFactor();

//      // Check for concatenation (implicit multiplication)
//      while (_position < _input.Length &&
//            (char.IsLetterOrDigit(Peek()) || Peek() == '('))
//      {
//        ConcatenationExpression concat;

//        if (result is ConcatenationExpression)
//          concat = (ConcatenationExpression)result;
//        else
//        {
//          concat = new ConcatenationExpression();
//          concat.Add(result);
//        }

//        concat.Add(ParseFactor());
//        result = concat;
//      }

//      return result;
//    }

//    private Expression ParseFactor()
//    {
//      Expression expr;

//      if (Peek() == '(')
//      {
//        Consume(); // consume '('
//        expr = ParseExpression();
//        Expect(')'); // expect and consume ')'
//      }
//      else if (char.IsLetter(Peek()))
//      {
//        expr = new Symbol(ConsumeChar().ToString());
//      }
//      else
//      {
//        throw new Exception($"Unexpected character at position {_position}: {Peek()}");
//      }

//      // Check for postfix operators
//      while (_position < _input.Length)
//      {
//        if (Peek() == '*')
//        {
//          Consume(); // consume '*'

//          // Check if there's a '+' after the '*'
//          if (_position < _input.Length && Peek() == '+')
//          {
//            Consume(); // consume '+'
//            expr = new PostfixExpression(expr, "*+");
//          }
//          else
//          {
//            expr = new PostfixExpression(expr, "*");
//          }
//        }
//        else if (Peek() == '^')
//        {
//          Consume(); // consume '^'

//          // Check if there's a '+' after the '^'
//          if (_position < _input.Length && Peek() == '+')
//          {
//            Consume(); // consume '+'
//            expr = new PostfixExpression(expr, "^+");
//          }
//          else
//          {
//            expr = new PostfixExpression(expr, "^");
//          }
//        }
//        else
//        {
//          break;
//        }
//      }

//      return expr;
//    }

//    private char Peek()
//    {
//      return _input[_position];
//    }

//    private char ConsumeChar()
//    {
//      return _input[_position++];
//    }

//    private void Consume()
//    {
//      _position++;
//    }

//    private void Expect(char expected)
//    {
//      if (_position >= _input.Length || _input[_position] != expected)
//      {
//        throw new Exception($"Expected '{expected}' at position {_position}");
//      }
//      _position++;
//    }
//  }

//  class Program
//  {
//    static void Main(string[] args)
//    {
//      // Example usage
//      string equation1 = "a*(b^+a+c)^+";
//      string equation2 = "a*+(b^+a+c)^+";

//      try
//      {
//        Parser parser1 = new Parser(equation1);
//        Expression expr1 = parser1.Parse();
//        Console.WriteLine($"Parsed equation 1: {expr1}");

//        Parser parser2 = new Parser(equation2);
//        Expression expr2 = parser2.Parse();
//        Console.WriteLine($"Parsed equation 2: {expr2}");

//        // Print the structure for debugging
//        PrintStructure(expr1, "Equation 1 Structure");
//        PrintStructure(expr2, "Equation 2 Structure");
//      }
//      catch (Exception ex)
//      {
//        Console.WriteLine($"Error: {ex.Message}");
//      }
//    }

//    static void PrintStructure(Expression expr, string title)
//    {
//      Console.WriteLine($"\n{title}:");
//      PrintStructureRecursive(expr, 0);
//    }

//    static void PrintStructureRecursive(Expression expr, int indent)
//    {
//      string padding = new string(' ', indent * 2);

//      if (expr is Symbol symbol)
//      {
//        Console.WriteLine($"{padding}Symbol: {symbol.Name}");
//      }
//      else if (expr is PostfixExpression postfix)
//      {
//        Console.WriteLine($"{padding}PostfixExpression: Operator={postfix.Operator}");
//        Console.WriteLine($"{padding}  Base:");
//        PrintStructureRecursive(postfix.Base, indent + 1);
//      }
//      else if (expr is ConcatenationExpression concat)
//      {
//        Console.WriteLine($"{padding}ConcatenationExpression:");
//        foreach (var element in concat.Elements)
//        {
//          PrintStructureRecursive(element, indent + 1);
//        }
//      }
//      else if (expr is SumExpression sum)
//      {
//        Console.WriteLine($"{padding}SumExpression:");
//        foreach (var term in sum.Terms)
//        {
//          PrintStructureRecursive(term, indent + 1);
//        }
//      }
//    }
//  }
//}
using generate_Grammar.Expressions;
using generate_Grammar.GrammarCalculation;
using generate_Grammar.Parser;


namespace RegexExpressionParser
{
  class Program
  {
    static void Main(string[] args)
    {
      // Example usage
      //string equation1 = "a*(c+a)";
      string equation1 = "c*a*(ab^++ca)(cb*)^+";
      //string equation1 = "a*b^+c*(a*c*+b^+)*+c*a";
      //string equation1 = "b*a*(a^+b*c*)*";
      string equation2 = "a*+(b^+a+c)^+";

      try
      {
        Parser parser1 = new Parser(equation1);
        Expression expr1 = parser1.Parse();
        GrammarGenerator generator = new GrammarGenerator(expr1,parser1.GetUniqueLetters());
        generator.calculateGrammar();
        Console.WriteLine($"Parsed equation 1: {expr1}");

        Parser parser2 = new Parser(equation2);
        Expression expr2 = parser2.Parse();
        Console.WriteLine($"Parsed equation 2: {expr2}");

        // Print the structure for debugging
        PrintStructure(expr1, "Equation 1 Structure");
        PrintStructure(expr2, "Equation 2 Structure");
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error: {ex.Message}");
      }
    }

    static void PrintStructure(Expression expr, string title)
    {
      Console.WriteLine($"\n{title}:");
      PrintStructureRecursive(expr, 0);
    }

    static void PrintStructureRecursive(Expression expr, int indent)
    {
      string padding = new string(' ', indent * 2);

      if (expr is Symbol symbol)
      {
        Console.WriteLine($"{padding}Symbol: {symbol.Name}");
      }
      else if (expr is PostfixExpression postfix)
      {
        Console.WriteLine($"{padding}PostfixExpression: Operator={postfix.Operator}");
        Console.WriteLine($"{padding}  Base:");
        PrintStructureRecursive(postfix.Base, indent + 1);
      }
      else if (expr is CompoundExpression compound)
      {
        Console.WriteLine($"{padding}{compound.Type}: ({compound.Elements.Count} elements)");
        foreach (var element in compound.Elements)
        {
          PrintStructureRecursive(element, indent + 1);
        }
      }
    }
  }
}