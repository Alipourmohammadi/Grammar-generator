using RegexExpressionParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace generate_Grammar.Expressions
{
  public class PostfixExpression : Expression
  {
    public Expression Base { get; }
    public string Operator { get; set; }

    public PostfixExpression(Expression baseExpr, string op)
    {
      Base = baseExpr;
      Operator = op;
    }

    protected override string ToStringImpl()
    {
      if (Base is Symbol)
        return $"{Base}{Operator}";
      else
        return $"({Base}){Operator}";
    }
  }
}
