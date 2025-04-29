using generate_Grammar.Expressions;
using generate_Grammar.Variables;
using System.Collections.Generic;
using System.Linq;
using static generate_Grammar.Expressions.CompoundExpression;

namespace generate_Grammar.GrammarCalculation
{
  /// <summary>
  /// Generates grammar rules from regular expressions.
  /// </summary>
  public class GrammarGenerator
  {
    private readonly Expression _expression;
    private readonly HashSet<char> _alphabet;
    private readonly List<Variable> _variables = new List<Variable>();

    public GrammarGenerator(Expression expression, HashSet<char> alphabet)
    {
      _expression = expression;
      _alphabet = alphabet;
    }

    /// <summary>
    /// Calculates the grammar by examining each variable and processing it.
    /// </summary>
    public void CalculateGrammar()
    {
      // Initialize with start variable
      _variables.Add(new Variable('S', _expression));

      // Process variables until all are marked as processed
      Variable? currentVar;
      while ((currentVar = _variables.FirstOrDefault(x => !x.IsProcessed)) != null)
      {
        if (currentVar.CanGenerateLambda())
        {
          currentVar.AddGrammar(null, 'λ');
        }

        CalculateVariable(currentVar);
        currentVar.IsProcessed = true;
      }
    }

    /// <summary>
    /// Prints all variables and their expressions.
    /// </summary>
    public void PrintVariables()
    {
      foreach (var variable in _variables)
      {
        System.Console.Write($"{variable.Name} : ");
        System.Console.WriteLine(variable.Expression);
      }
    }

    /// <summary>
    /// Prints the final grammar rules.
    /// </summary>
    public void PrintGrammar()
    {
      foreach (var variable in _variables)
      {
        variable.PrintGrammar();
        System.Console.WriteLine();
      }
    }

    /// <summary>
    /// Calculates the production rules for a given variable.
    /// </summary>
    private void CalculateVariable(Variable variable)
    {
      if (variable.Expression is CompoundExpression compoundExpr && compoundExpr.Elements.Count > 0)
      {
        ProcessCompoundExpression(variable, compoundExpr);
      }
      else if (variable.Expression is PostfixExpression postfixExpr && postfixExpr.Base is CompoundExpression)
      {
        ProcessPostfixExpression(variable, postfixExpr);
      }
      else
      {
        // Default behavior for simple expressions
        variable.AddGrammar(variable, _alphabet.ElementAt(0));
      }
    }

    /// <summary>
    /// Processes compound expressions for grammar calculation.
    /// </summary>
    private void ProcessCompoundExpression(Variable variable, CompoundExpression compoundExpr)
    {
      if (compoundExpr.Type == CompoundType.Alternation)
      {
        ProcessAlternationExpression(variable, compoundExpr);
      }
      else if (compoundExpr.Type == CompoundType.Concatenation)
      {
        ProcessConcatenationExpression(variable, compoundExpr);
      }
    }

    /// <summary>
    /// Processes alternation expressions for grammar calculation.
    /// </summary>
    private void ProcessAlternationExpression(Variable variable, CompoundExpression alternationExpr)
    {
      foreach (char symbol in _alphabet)
      {
        var result = CalculateAlternation(alternationExpr, symbol);
        if (result != null)
        {
          if (result is CompoundExpression compExpr && compExpr.Elements.Count > 0)
          {
            variable.AddGrammar(AddUniqueVariable(result), symbol);
          }
          else if (!(result is CompoundExpression))
          {
            variable.AddGrammar(null, symbol); // Lambda case
          }
        }
      }
    }

    /// <summary>
    /// Processes concatenation expressions for grammar calculation.
    /// </summary>
    private void ProcessConcatenationExpression(Variable variable, CompoundExpression concatExpr)
    {
      foreach (char symbol in _alphabet)
      {
        var result = CalculateConcatenation(concatExpr, symbol);
        if (result != null)
        {
          if (result is CompoundExpression compExpr && compExpr.Elements.Count > 0)
          {
            variable.AddGrammar(AddUniqueVariable(result), symbol);
          }
          else if (!(result is CompoundExpression))
          {
            variable.AddGrammar(null, symbol); // Lambda case
          }
        }
      }
    }

    /// <summary>
    /// Processes postfix expression for grammar calculation.
    /// </summary>
    private void ProcessPostfixExpression(Variable variable, PostfixExpression postfixExpr)
    {
      foreach (char symbol in _alphabet)
      {
        var result = CalculateNestedCompoundExpression(postfixExpr, symbol);
        if (result != null)
        {
          if (result is CompoundExpression compExpr && compExpr.Elements.Count > 0)
          {
            variable.AddGrammar(AddUniqueVariable(result), symbol);
          }
          else if (!(result is CompoundExpression))
          {
            variable.AddGrammar(null, symbol); // Lambda case
          }
        }
      }
    }

    /// <summary>
    /// Calculates the derivatives of alternation expressions.
    /// </summary>
    private Expression CalculateAlternation(CompoundExpression expression, char symbol)
    {
      var alternation = new CompoundExpression(CompoundType.Alternation);

      foreach (var element in expression.Elements)
      {
        Expression result = null;

        if (element is CompoundExpression compoundExpr && compoundExpr.Type == CompoundType.Concatenation)
        {
          result = CalculateConcatenation(compoundExpr, symbol);
        }
        else if (element is PostfixExpression postfixExpr && postfixExpr.Base is CompoundExpression)
        {
          result = CalculateNestedCompoundExpression(postfixExpr, symbol);
        }
        else
        {
          // Create a concatenation to handle simple expressions
          var tempConcat = new CompoundExpression(CompoundType.Concatenation);
          tempConcat.Add(element);
          result = CalculateConcatenation(tempConcat, symbol);
        }

        if (result != null)
        {
          alternation.AddUnique(result);
        }
      }

      return alternation.Elements.Count switch
      {
        //0 => null,
        1 => alternation.Elements.First(),
        _ => alternation
      };
    }

    /// <summary>
    /// Calculates the derivatives of concatenation expressions.
    /// </summary>
    private Expression CalculateConcatenation(CompoundExpression expression, char symbol)
    {
      var alternation = new CompoundExpression(CompoundType.Alternation);

      for (int i = 0; i < expression.Elements.Count; i++)
      {
        var element = expression.Elements[i];

        if (element is Symbol symbolExpr)
        {
          if (symbolExpr.Name[0] == symbol)
          {
            // If this is the last element, return lambda or empty concatenation
            if (i + 1 >= expression.Elements.Count)
            {
              if (alternation.Elements.Count > 0)
              {
                alternation.AddUnique(new Symbol("λ"));
                return ReturnAlternationResult(alternation);
              }
              return new Symbol("λ");
            }

            // Otherwise, return the rest of the concatenation
            alternation.AddUnique(CreateSubConcatenation(expression, i + 1));
          }

          // No match for this symbol
          return ReturnAlternationResult(alternation);
        }
        else if (element is PostfixExpression postfixExpr && postfixExpr.Base is Symbol baseSymbol)
        {
          // Handle postfix expressions with symbol base
          if (baseSymbol.Name[0] == symbol)
          {
            if (postfixExpr.Operator == "+")
            {
              // a+ derivative with respect to 'a' is a*
              if (i + 1 >= expression.Elements.Count)
              {
                alternation.AddUnique(new PostfixExpression(postfixExpr.Base, "*"));
                return ReturnAlternationResult(alternation);
              }

              var subExpr = CreateSubConcatenation(expression, i + 1);
              var starExpr = new PostfixExpression(postfixExpr.Base, "*");
              subExpr.Insert(starExpr);
              alternation.AddUnique(subExpr);
              return ReturnAlternationResult(alternation);
            }
            else if (postfixExpr.Operator == "*")
            {
              // a* derivative with respect to 'a' is a* or the rest
              if (i + 1 >= expression.Elements.Count)
              {
                alternation.AddUnique(new PostfixExpression(postfixExpr.Base, "*"));
                return ReturnAlternationResult(alternation);
              }

              // Add a*
              var subExpr = CreateSubConcatenation(expression, i);
              alternation.AddUnique(subExpr);

              // Add derivative of the rest
              var rest = CreateSubConcatenation(expression, i + 1);
              var calculatedRest = CalculateConcatenation(rest, symbol);
              if (calculatedRest != null)
              {
                alternation.AddUnique(calculatedRest);
              }

              return ReturnAlternationResult(alternation);
            }
          }
          else if (postfixExpr.Operator == "*")
          {
            // a* with respect to other symbol is derivative of the rest
            if (i + 1 >= expression.Elements.Count)
              return ReturnAlternationResult(alternation);

            var rest = CreateSubConcatenation(expression, i + 1);
            var result = CalculateConcatenation(rest, symbol);
            if (result != null)
            {
              alternation.AddUnique(result);
            }

            return ReturnAlternationResult(alternation);
          }
          else
          {
            return ReturnAlternationResult(alternation);
          }
        }
        else if (element is CompoundExpression nestedCompound)
        {
          // Convert to postfix with operator "1" for consistent handling
          element = new PostfixExpression(nestedCompound, "1");
        }

        if (element is PostfixExpression postfixCompoundExpr && postfixCompoundExpr.Base is CompoundExpression)
        {
          var nestedResult = CalculateNestedCompoundExpression(postfixCompoundExpr, symbol);

          if (nestedResult != null)
          {
            // Process the result depending on its type
            if (nestedResult is Symbol resultSymbol && resultSymbol.Name == "λ" &&
                CreateSubConcatenation(expression, i + 1).Elements.Count != 0)
            {
              nestedResult = CreateSubConcatenation(expression, i + 1);
            }
            else if (expression.Elements.Count > 1)
            {
              // Add the rest of the expression to the result
              if (nestedResult is CompoundExpression resultCompound)
              {
                AppendRemainingElements(resultCompound, expression, i + 1);
              }
              else
              {
                var temp = new CompoundExpression(CompoundType.Concatenation);
                temp.Add(nestedResult);
                for (int j = i + 1; j < expression.Elements.Count; j++)
                {
                  temp.Add(expression.Elements[j]);
                }
                nestedResult = temp;
              }
            }

            alternation.AddUnique(nestedResult);
            if (postfixCompoundExpr.Operator == "+" || postfixCompoundExpr.Operator == "1")
            {
              // Short-circuit for non-repeating operators
              Variable vari = new Variable('T', postfixCompoundExpr.Base);
              if (!vari.CanGenerateLambda())
              {
                break;
              }
            }
          }
        }
      }

      return ReturnAlternationResult(alternation);
    }

    /// <summary>
    /// Appends remaining elements from source expression to the target compound expression.
    /// </summary>
    private void AppendRemainingElements(CompoundExpression target, CompoundExpression source, int startIndex)
    {
      if (target.Type == CompoundType.Concatenation)
      {
        for (int j = startIndex; j < source.Elements.Count; j++)
        {
          target.Add(source.Elements[j]);
        }
      }
      else if (target.Type == CompoundType.Alternation)
      {
        for (int i = 0; i < target.Elements.Count; i++)
        {
          var element = target.Elements[i];
          if (element is CompoundExpression concat && concat.Type == CompoundType.Concatenation)
          {
            for (int j = startIndex; j < source.Elements.Count; j++)
            {
              concat.Add(source.Elements[j]);
            }
          }
          else
          {
            var temp = new CompoundExpression(CompoundType.Concatenation);
            temp.Add(element);
            for (int j = startIndex; j < source.Elements.Count; j++)
            {
              temp.Add(source.Elements[j]);
            }
            target.Elements[i] = temp;
          }
        }
      }
    }

    /// <summary>
    /// Helper method to handle whether to return alternation or null.
    /// </summary>
    private Expression? ReturnAlternationResult(CompoundExpression expr)
    {
      return expr.Elements.Count switch
      {
        0 => null,
        1 => expr.Elements.First(),
        _ => expr
      };
    }

    /// <summary>
    /// Calculates derivatives for nested compound expressions with postfix operators.
    /// </summary>
    private Expression? CalculateNestedCompoundExpression(PostfixExpression expression, char symbol)
    {
      var baseCompound = expression.Base as CompoundExpression;
      if (baseCompound == null)
      {
        return null;
      }

      Expression result = null;

      if (baseCompound.Type == CompoundType.Concatenation)
      {
        result = CalculateConcatenation(baseCompound, symbol);
      }
      else if (baseCompound.Type == CompoundType.Alternation)
      {
        result = CalculateAlternation(baseCompound, symbol);
      }

      if (result == null)
      {
        return null;
      }

      // Handle case where operator is "1" (just one instance)
      if (expression.Operator == "1")
      {
        return result;
      }

      // Apply postfix operator to the result
      var newExpression = new PostfixExpression(expression.Base, "*");

      if (result is CompoundExpression resultCompound)
      {
        if (resultCompound.Type == CompoundType.Concatenation)
        {
          resultCompound.Add(newExpression);
        }
        else if (resultCompound.Type == CompoundType.Alternation)
        {
          for (int i = 0; i < resultCompound.Elements.Count; i++)
          {
            var element = resultCompound.Elements[i];
            if (element is CompoundExpression concatElement && concatElement.Type == CompoundType.Concatenation)
            {
              concatElement.Add(newExpression);
            }
            else if (element is Symbol symbolElement && symbolElement.Name == "λ")
            {
              resultCompound.Elements[i] = newExpression;
            }
            else
            {
              var temp = new CompoundExpression(CompoundType.Concatenation);
              temp.Add(element);
              temp.Add(newExpression);
              resultCompound.Elements[i] = temp;
            }
          }
        }
        return resultCompound;
      }
      else
      {
        var temp = new CompoundExpression(CompoundType.Concatenation);
        if (!(result is Symbol sym && sym.Name == "λ"))
        {
          temp.Add(result);
        }
        temp.Add(newExpression);
        return temp;
      }
    }

    /// <summary>
    /// Creates a sublist of concatenation from the given expression starting at index i.
    /// </summary>
    private CompoundExpression CreateSubConcatenation(CompoundExpression expression, int startIndex)
    {
      var concat = new CompoundExpression(CompoundType.Concatenation);
      for (int k = startIndex; k < expression.Elements.Count; k++)
      {
        concat.Add(expression.Elements[k]);
      }
      return concat;
    }

    /// <summary>
    /// Adds a variable with the given expression if it doesn't exist already.
    /// </summary>
    private Variable AddUniqueVariable(Expression expr)
    {
      // Check for existing variables with equivalent expressions
      foreach (var variable in _variables)
      {
        if (AreExpressionsEquivalent(variable.Expression, expr))
        {
          return variable;
        }
      }

      // Create a new variable with the next available name
      char name;
      if (_variables.Count == 0 || _variables.Last().Name == 'S')
      {
        name = 'A';
      }
      else
      {
        name = (char)(_variables.Last().Name + 1);
      }

      var newVariable = new Variable(name, expr);
      _variables.Add(newVariable);
      return newVariable;
    }

    /// <summary>
    /// Determines if two expressions are structurally equivalent.
    /// </summary>
    private bool AreExpressionsEquivalent(Expression expr1, Expression expr2)
    {
      // Handle alternation compound expressions comparison (order doesn't matter)
      if (expr1 is CompoundExpression compExpr1 && compExpr1.Type == CompoundType.Alternation &&
          expr2 is CompoundExpression compExpr2 && compExpr2.Type == CompoundType.Alternation)
      {
        // First check if they have the same number of elements
        if (compExpr1.Elements.Count == compExpr2.Elements.Count)
        {
          bool allElementsMatch = true;

          // Check if every element in expr1 exists in expr2
          foreach (var element in compExpr1.Elements)
          {
            if (!compExpr2.Elements.Any(e => e.Equals(element)))
            {
              allElementsMatch = false;
              break;
            }
          }

          // Check if every element in expr2 exists in expr1
          foreach (var element in compExpr2.Elements)
          {
            if (!compExpr1.Elements.Any(e => e.Equals(element)))
            {
              allElementsMatch = false;
              break;
            }
          }

          return allElementsMatch;
        }
      }

      // Simple equality comparison for other expression types
      return expr1.Equals(expr2);
    }
  }
}