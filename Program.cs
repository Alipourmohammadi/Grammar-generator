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
      //string equation1 = "a*c+a*a";
      string equation1 = "a*(c+b)+a";
      //string equation1 = "b*a*(a^+b*c*)*";
      //string equation1 = "(ab^++c*b*a^+)*a*c";
      //string equation1 = "(a^+b^++ac*)^++(b*a^++c^+b*)*b";
      //string equation1 = "abcd";
      //string equation1 = "(aa+b)(ab+bb)*";
      //string equation1 = "(a+ab^+)*(bc*+c)^+";



      string equation2 = "a*+(b^+a+c)^+";

      try
      {
        Parser parser1 = new Parser(equation1);
        Expression expr1 = parser1.Parse();
        GrammarGenerator generator = new GrammarGenerator(expr1,parser1.GetUniqueLetters());
        Console.WriteLine($"Parsed equation : {expr1}");
        Console.WriteLine();
        generator.calculateGrammar();
        Console.WriteLine("Generated Grammar:");
        generator.printGrammar();
        Console.WriteLine("Variables are:");
        generator.printVariables();

        //PrintStructure(expr1, "Equation Structure");
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