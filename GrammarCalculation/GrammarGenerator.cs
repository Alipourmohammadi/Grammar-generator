using generate_Grammar.Expressions;
using generate_Grammar.Variables;
using System.ComponentModel.Design;
using System.Diagnostics.Metrics;
using System.Net.Http.Headers;
using System.Xml.XPath;
using static generate_Grammar.Expressions.CompoundExpression;

namespace generate_Grammar.GrammarCalculation
{
  public class GrammarGenerator
  {
    Expression _expression;
    HashSet<char> _alphabet;
    List<Variable> Variables = new List<Variable>();

    public GrammarGenerator(Expression expression, HashSet<char> alphabet)
    {
      _expression = expression;
      _alphabet = alphabet;
    }
    public enum ExType
    {
      concatinaion,
      alternation,
      postFixEx
    }
    public void calculateGrammar()
    {
      Variables.Add(new Variable('S', _expression));
      var Var = Variables.FirstOrDefault(x => x.prosced == false);
      while (Var != null)
      {
        calculateVariable(Var);
        Var = Variables.FirstOrDefault(y => y.prosced == false);
        Console.WriteLine();
      }
      Console.WriteLine("the final results are :");
      foreach (var item in Variables)
      {
        Console.Write(item.Name + " : ");
        Console.WriteLine(item.expr);
      }
    }

    private void calculateVariable(Variable Var)
    {
      if (Var.expr is CompoundExpression compex && compex.Elements.Count > 0)
      {
        CompoundExpression compoundExpression = (CompoundExpression)Var.expr;
        if (compoundExpression.Type == CompoundExpression.CompoundType.Alternation)
        {
          for (int i = 0; i < _alphabet.Count; i++)
          {
            var result = calcAlternation(compoundExpression, _alphabet.ElementAt(i));
            if (result != null && result is CompoundExpression c1 && c1.Elements.Count > 0)
            {
              Console.Write(Var.Name + "->");
              Console.WriteLine(_alphabet.ElementAt(i).ToString() + AddUnique(result).Name.ToString());
            }
          }
        }
        else if (compoundExpression.Type == CompoundExpression.CompoundType.Concatenation)
        {
          for (int i = 0; i < _alphabet.Count; i++)
          {
            var result = calcConcatenation(compoundExpression, _alphabet.ElementAt(i));
            if (result != null && result is CompoundExpression c1 && c1.Elements.Count > 0)
            {
              Console.Write(Var.Name + "->");
              Console.WriteLine(_alphabet.ElementAt(i).ToString() + AddUnique(result).Name.ToString());
            }

          }
        }
      }
      else if ((Var.expr is PostfixExpression postfixEx) && (postfixEx.Base is CompoundExpression))
      {
        for (int i = 0; i < _alphabet.Count; i++)
        {
          var result = calcNestedCompoundEx(postfixEx, _alphabet.ElementAt(i));
          if (result != null && result is CompoundExpression c1 && c1.Elements.Count > 0)
          {
            Console.Write(Var.Name + "->");
            Console.WriteLine(_alphabet.ElementAt(i).ToString() + AddUnique(result).Name.ToString());
          }
        }
      }
      Var.prosced = true;
    }
    public Expression? calcAlternation(CompoundExpression expression, char alphabet)
    {
      var Alter = new CompoundExpression(CompoundExpression.CompoundType.Alternation);
      foreach (var Ex in expression.Elements)
      {
        if (Ex is CompoundExpression CEx && CEx.Type == CompoundExpression.CompoundType.Concatenation)
        {
          var R = calcConcatenation(CEx, alphabet);
          if (R != null)
            Alter.AddUnique(R);
        }
        else if (Ex is PostfixExpression postEx && postEx.Base is CompoundExpression)
        {
          var R = calcNestedCompoundEx(postEx, alphabet);
          if (R != null)
            Alter.AddUnique(R);
        }
        else
        {
          var CEX = new CompoundExpression(CompoundExpression.CompoundType.Concatenation);
          CEX.Add(Ex);
          var R = calcConcatenation(CEX, alphabet);
          if (R != null)
            Alter.AddUnique(R);
        }
      }

      if (Alter.Elements.Count > 0)
      {
        if (Alter.Elements.Count == 1)
        {
          return Alter.Elements.First();
        }
        return Alter;
      }
      return null;
    }
    public Expression? calcConcatenation(CompoundExpression expression, char alphabet)
    {
      var Alter = new CompoundExpression(CompoundExpression.CompoundType.Alternation);
      for (int i = 0; i < expression.Elements.Count; i++)
      {
        var Ex = expression.Elements.ElementAt(i);
        if (Ex is Symbol symbol)
        {
          if (symbol.Name[0] == alphabet)
          {
            if (i + 1 >= expression.Elements.Count)
              if (Alter.Elements.Count > 0)
              {
                Alter.AddUnique(new Symbol("λ"));
                return Alter;
              }
              else return new Symbol("λ");
            return subConcatEx(expression, i + 1);
          }
          if (Alter.Elements.Count > 0) return Alter; else return null;
        }
        else if (Ex is PostfixExpression PEx && PEx.Base is Symbol symbol1)
        {
          if (symbol1.Name[0] == alphabet)
          {
            if (PEx.Operator == "^+")
            {
              if (i + 1 > expression.Elements.Count)
                return new PostfixExpression(PEx.Base, "*");
              var subEx = subConcatEx(expression, i + 1);
              subEx.Insert(new PostfixExpression(PEx.Base, "*"));
              Alter.AddUnique(subEx);
              return Alter;
            }
            if (PEx.Operator == "*")
            {
              if (i + 1 >= expression.Elements.Count)
                return new PostfixExpression(PEx.Base, "*");
              var subEx = subConcatEx(expression, i);
              Alter.AddUnique(subEx);
              // calculate the rest
              var rest = subConcatEx(expression, i + 1);
              var calculatedRest = calcConcatenation(rest, alphabet);
              if (calculatedRest is not null)
              {
                Alter.AddUnique(calculatedRest);
              }
              if (Alter.Elements.Count == 1)
              {
                return Alter.Elements.First();
              }
              return Alter;
            }
          }
          else if (PEx.Operator == "*")
          {
            // calculate the rest
            if (i + 1 >= expression.Elements.Count)
              if (Alter.Elements.Count > 0) return Alter; else return null;
            var rest = subConcatEx(expression, i + 1);
            var result = calcConcatenation(rest, alphabet);
            if (result != null)
            {
              Alter.AddUnique(result);
            }
            return Alter;
          }
          else
          {
            if (Alter.Elements.Count > 0) return Alter; else return null;
          }
        }
        //check the removal of the second condition
        else if (Ex is CompoundExpression _CEx)
        {
          Ex = new PostfixExpression(_CEx, "1");
        }
        if ((Ex is PostfixExpression CEx && CEx.Base is CompoundExpression))
        {
          var nestedResult = calcNestedCompoundEx(CEx, alphabet);
          // limit if there is none left
          if (nestedResult is not null)
          {
            //add the rest
            if (expression.Elements.Count == 1)
            {

            }
            else if (nestedResult is CompoundExpression compEx)
            {
              if (compEx.Type == CompoundExpression.CompoundType.Concatenation)
              {
                for (int j = i + 1; j < expression.Elements.Count; j++)
                  compEx.Add(expression.Elements.ElementAt(j));
              }
              else
              {
                for (int k = 0; k < compEx.Elements.Count; k++)
                {
                  var ex = compEx.Elements.ElementAt(k);
                  if (ex is CompoundExpression cex && cex.Type == CompoundExpression.CompoundType.Concatenation)
                  {
                    for (int j = i + 1; j < expression.Elements.Count; j++)
                      cex.Add(expression.Elements.ElementAt(j));
                  }
                  else
                  {
                    var temp = new CompoundExpression(CompoundExpression.CompoundType.Concatenation);
                    temp.Add(ex);
                    for (int j = i + 1; j < expression.Elements.Count; j++)
                      temp.Add(expression.Elements.ElementAt(j));
                    compEx.Elements[k] = temp;
                  }
                }
              }
            }
            else
            {
              var temp = new CompoundExpression(CompoundExpression.CompoundType.Concatenation);
              temp.Add(nestedResult);
              for (int j = i + 1; j < expression.Elements.Count; j++)
                temp.Add(expression.Elements.ElementAt(j));
              nestedResult = temp;
            }
            //return the results
            Alter.AddUnique(nestedResult);
            if (CEx.Operator == "^+")
              break;
          }
        }
      }
      if (Alter.Elements.Count > 0)
      {
        if (Alter.Elements.Count == 1)
        {
          return Alter.Elements.First();
        }
        return Alter;
      }
      else
        return null;
    }
    public Expression? calcNestedCompoundEx(PostfixExpression _expression, char alphabet)
    {
      var expression = new PostfixExpression(_expression.Base, _expression.Operator);
      var compEx = (CompoundExpression)expression.Base;
      if (compEx.Type == CompoundExpression.CompoundType.Concatenation)
      {
        var resutl = calcConcatenation(compEx, alphabet);

        if (resutl is not null)
        {
          if (expression.Operator == "1")
          {
            return resutl;
          }
          //add the rest
          expression.Operator = "*";
          if (resutl is CompoundExpression _compEx)
          {
            if (_compEx.Type == CompoundExpression.CompoundType.Concatenation)
            {
              _compEx.Add(expression);
            }
            else
            {
              for (int k = 0; k < _compEx.Elements.Count; k++)
              {
                var ex = _compEx.Elements.ElementAt(k);
                if (ex is CompoundExpression cex && cex.Type == CompoundExpression.CompoundType.Concatenation)
                {
                  cex.Add(expression);
                  _compEx.Elements[k] = cex;
                }
                else
                {
                  Console.WriteLine("this should not be possible bu anyway!!!");
                  var temp = new CompoundExpression(CompoundExpression.CompoundType.Concatenation);
                  temp.Add(ex);
                  temp.Add(expression);
                  ex = temp;
                }
              }
            }
            return _compEx;
          }
          else
          {
            Console.WriteLine("this should not be possible bu anyway!!! 2");
            var temp = new CompoundExpression(CompoundExpression.CompoundType.Concatenation);
            temp.Add(resutl);
            temp.Add(expression);
            resutl = temp;
          }
          //return the results
          return resutl;
        }
      }
      else if (compEx.Type == CompoundExpression.CompoundType.Alternation)
      {
        var resutl = calcAlternation(compEx, alphabet);
        if (resutl is not null)
        {
          if (expression.Operator == "1")
          {
            return resutl;
          }
          //add the rest
          expression.Operator = "*";
          if (resutl is CompoundExpression _compEx)
          {
            if (_compEx.Type == CompoundExpression.CompoundType.Concatenation)
            {
              _compEx.Add(expression);
              return _compEx;
            }
            else
            {
              for (int k = 0; k < _compEx.Elements.Count; k++)
              {
                var ex = _compEx.Elements.ElementAt(k);
                if (ex is CompoundExpression cex && cex.Type == CompoundExpression.CompoundType.Concatenation)
                {
                  cex.Add(expression);
                  _compEx.Elements[k] = cex;
                }
                else
                {
                  if (ex is Symbol sym && sym.Name == "λ")
                  {
                    _compEx.Elements[k] = expression;
                  }
                  else
                  {
                    var temp = new CompoundExpression(CompoundExpression.CompoundType.Concatenation);
                    temp.Add(ex);
                    temp.Add(expression);
                    _compEx.Elements[k] = temp;
                  }
                }
              }
              return _compEx;
            }
          }
          else
          {
            var temp = new CompoundExpression(CompoundExpression.CompoundType.Concatenation);
            if (resutl is Symbol sym && sym.Name != "λ")
            {
              temp.Add(resutl);
            }
            temp.Add(expression);
            resutl = temp;
          }
          //return the results
          return resutl;
        }
      }
      return null;
      // if (symbol) not computed here?
    }

    // sublist of concatenation compoundEx
    private CompoundExpression subConcatEx(CompoundExpression expression, int i)
    {
      var concat = new CompoundExpression(CompoundExpression.CompoundType.Concatenation);
      for (int k = i; k < expression.Elements.Count; k++)
      {
        concat.Add(expression.Elements.ElementAt(k));
      }
      return concat;
    }

    private Variable AddUnique(Expression expr)
    {

      foreach (var item in Variables)
      {
        // Handle alternation compound expressions comparison (order doesn't matter)
        if (item.expr is CompoundExpression CEx1 && CEx1.Type == CompoundType.Alternation &&
            expr is CompoundExpression CEx2 && CEx2.Type == CompoundType.Alternation)
        {
          // First check if they have the same number of elements
          if (CEx1.Elements.Count == CEx2.Elements.Count)
          {
            bool allElementsMatch = true;

            // Check if every element in CEx1 exists in CEx2
            foreach (var element in CEx1.Elements)
            {
              if (!CEx2.Elements.Any(e => e.Equals(element)))
              {
                allElementsMatch = false;
                break;
              }
            }

            // Check if every element in CEx2 exists in CEx1
            foreach (var element in CEx2.Elements)
            {
              if (!CEx1.Elements.Any(e => e.Equals(element)))
              {
                allElementsMatch = false;
                break;
              }
            }

            if (allElementsMatch)
            {
              return item;
            }
          }
        }
        // Simple equality comparison for other expression types
        else if (item.expr.Equals(expr))
        {
          return item;
        }
      }


      if (Variables.Count == 0 || Variables.Last().Name == 'S')
      {
        Variables.Add(new Variable('A', expr));
        return Variables.Last();
      }
      else
      {
        char name = Variables.Last().Name;
        Variables.Add(new Variable((char)(name + 1), expr));
        return Variables.Last();
      }

    }
  }

}
