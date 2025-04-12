using generate_Grammar.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace generate_Grammar.Variables
{
  /// <summary>
  /// Represents a variable in grammar with its associated expression and production rules.
  /// </summary>
  public class Variable
  {
    public Expression Expression { get; set; }
    public char Name { get; set; }
    public bool IsProcessed { get; set; }
    public List<string> GrammarResults { get; private set; } = new List<string>();

    public Variable(char name, Expression expression)
    {
      Name = name;
      Expression = expression;
      IsProcessed = false;
    }

    /// <summary>
    /// Determines if this variable's expression can generate the empty string (lambda).
    /// </summary>
    public bool CanGenerateLambda()
    {
      return CheckLambdaGeneration(this.Expression);
    }

    /// <summary>
    /// Recursively checks if an expression can generate lambda.
    /// </summary>
    private bool CheckLambdaGeneration(Expression expr)
    {
      switch (expr)
      {
        case Symbol symbol:
          return symbol.Name == "λ";

        case PostfixExpression postfix:
          // Only * operator can generate lambda
          if (postfix.Operator == "*")
            return true;

          if (postfix.Operator == "^+" || postfix.Operator == "")
            return false;

          // For numbered operators (^n), check if n >= 0
          if (postfix.Operator.StartsWith("^") && postfix.Operator.Length > 1)
          {
            if (int.TryParse(postfix.Operator.Substring(1), out int count))
              return count == 0 || (count > 0 && CheckLambdaGeneration(postfix.Base));
          }

          return false;

        case CompoundExpression compound:
          if (compound.Type == CompoundExpression.CompoundType.Alternation)
          {
            // Only need one alternative to generate lambda
            return compound.Elements.Any(CheckLambdaGeneration);
          }
          else if (compound.Type == CompoundExpression.CompoundType.Concatenation)
          {
            // All parts must be able to generate lambda
            return compound.Elements.All(CheckLambdaGeneration);
          }
          return false;

        default:
          return false;
      }
    }

    /// <summary>
    /// Adds a grammar production rule to this variable.
    /// </summary>
    public void AddGrammar(Variable variable, char symbol)
    {
      string production;

      if (variable != null)
      {
        // Check if the target variable has a non-empty expression
        if (variable.Expression is CompoundExpression compoundExpr && compoundExpr.Elements.Count > 0 ||
            !(variable.Expression is CompoundExpression))
        {
          production = $"{symbol}{variable.Name}";
        }
        else
        {
          // Just the symbol if the variable has an empty expression
          production = symbol.ToString();
        }
      }
      else
      {
        // Just the symbol for lambda or direct terminals
        production = symbol.ToString();
      }

      // Add the production if it doesn't already exist
      if (!GrammarResults.Contains(production))
      {
        GrammarResults.Add(production);
      }
    }

    /// <summary>
    /// Prints the grammar production rules for this variable.
    /// </summary>
    public void PrintGrammar()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append($"{Name} -> ");

      for (int i = 0; i < GrammarResults.Count; i++)
      {
        sb.Append(GrammarResults[i]);
        if (i + 1 < GrammarResults.Count)
        {
          sb.Append(" | ");
        }
      }

      System.Console.Write(sb.ToString());
    }

    /// <summary>
    /// Returns a string representation of this variable.
    /// </summary>
    public override string ToString()
    {
      return $"{Name} -> {Expression}";
    }
  }
}