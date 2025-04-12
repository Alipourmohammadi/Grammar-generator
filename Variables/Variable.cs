

using generate_Grammar.Expressions;
using System.Text;

namespace generate_Grammar.Variables
{
  public class Variable
  {

    public Expression expr { get; set; }
    public char Name { get; set; }
    public bool prosced { get; set; }

    public List<string> GrammarResults { get; set; } = new List<string>();
    public Variable(char name, Expression Expr)
    {
      Name = name;
      expr = Expr;
    }
    public bool CanGenerateLambda()
    {
      return checkLambdaCreation(this.expr);
    }
    private bool checkLambdaCreation(Expression expr)
    {
      if (expr is Symbol symbol)
      {
        if (symbol.Name == "λ")
        {
          return true;
        }
        return false;
      }
      else if (expr is PostfixExpression postfix)
      {
        if (postfix.Operator == "*")
          return true;
        else if (postfix.Operator == "^+" || postfix.Operator == "")
          return false;


        //return CanGenerateLambda(postfix.Base);
      }
      else if (expr is CompoundExpression compound)
      {
        if (compound.Type == CompoundExpression.CompoundType.Alternation)
        {
          foreach (var element in compound.Elements)
          {
            if (checkLambdaCreation(element))
            {
              return true;
            }
          }
          return false;
        }
        else if (compound.Type == CompoundExpression.CompoundType.Concatenation)
        {
          foreach (var element in compound.Elements)
          {
            if (!checkLambdaCreation(element))
            {
              return false;
            }
          }
          return true;
        }
      }
      return false;
    }
    public void addGrammar(Variable var, char alph)
    {
      if (var != null)
      {
        if (var.expr is CompoundExpression c1)
        {
          if (c1.Elements.Count > 0)
          {
            GrammarResults.Add(alph + var.Name.ToString());
            //Console.Write(this.Name + "->");
            //Console.WriteLine(alph + var.Name.ToString());
          }
        }
        else
        {
          GrammarResults.Add(alph + var.Name.ToString());
          //Console.Write(this.Name + "->");
          //Console.WriteLine(alph + var.Name.ToString());
        }
      }
      else GrammarResults.Add(alph.ToString());
    }

    public void PrintGrammar()
    {
      Console.Write(this.Name + " -> ");
      for (int i = 0; i < GrammarResults.Count; i++)
      {
        Console.Write(GrammarResults.ElementAt(i));
        if (i + 1 < GrammarResults.Count)
        {
          Console.Write("|");
        }
      }
    }
  }
}
