using generate_Grammar.Expressions;
using System.ComponentModel.Design;
using System.Diagnostics.Metrics;
using System.Net.Http.Headers;

namespace generate_Grammar.GrammarCalculation
{
  public class GrammarGenerator
  {
    Expression _expression;
    HashSet<char> _alphabet;
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
      if (_expression is CompoundExpression)
      {
        CompoundExpression compoundExpression = (CompoundExpression)_expression;
        if (compoundExpression.Type == CompoundExpression.CompoundType.Alternation)
        {
          for (int i = 0; i < _alphabet.Count; i++)
          {
            var result = calcAlternation(compoundExpression, _alphabet.ElementAt(i));
            Console.WriteLine(result);
          }
        }
        else if (compoundExpression.Type == CompoundExpression.CompoundType.Concatenation)
        {
          for (int i = 0; i < _alphabet.Count; i++)
          {
            var result = calcConcatenation(compoundExpression, _alphabet.ElementAt(i));
            Console.WriteLine(result);

          }
        }
      }
      else if ((_expression is PostfixExpression postfixEx) && (postfixEx.Base is CompoundExpression))
      {
        for (int i = 0; i < _alphabet.Count; i++)
        {
          var result = calcNestedCompoundEx(postfixEx, _alphabet.ElementAt(i));
          Console.WriteLine(result);

        }
      }
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
              return new Symbol("λ");
            return subConcatEx(expression, i + 1);
          }
          return null;
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
              return subEx;
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
              return null;
            var rest = subConcatEx(expression, i + 1);
            return calcConcatenation(rest, alphabet);
          }
          else { return null; }
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
                    ex = temp;
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


  }

}
