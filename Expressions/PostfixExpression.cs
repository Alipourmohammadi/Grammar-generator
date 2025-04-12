using RegexExpressionParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace generate_Grammar.Expressions
{
  /// <summary>
  /// Represents a postfix expression (e.g., E*, E+, E^n)
  /// </summary>
  public class PostfixExpression : Expression
  {
    /// <summary>
    /// Gets the base expression
    /// </summary>
    public Expression Base { get; }

    /// <summary>
    /// Gets or sets the operator used in this postfix expression
    /// </summary>
    public string Operator { get; set; }

    /// <summary>
    /// Creates a new postfix expression with the specified base expression and operator
    /// </summary>
    public PostfixExpression(Expression baseExpr, string op)
    {
      Base = baseExpr ?? throw new ArgumentNullException(nameof(baseExpr));
      Operator = op ?? throw new ArgumentNullException(nameof(op));
    }

    /// <summary>
    /// Returns a string representation of the postfix expression
    /// </summary>
    protected override string ToStringImpl()
    {
      if (Base is Symbol)
        return $"{Base}{Operator}";
      else
        return $"({Base}){Operator}";
    }

    /// <summary>
    /// Checks if this postfix expression equals another expression
    /// </summary>
    public override bool Equals(Expression other)
    {
      if (other is PostfixExpression postfix)
        return Base.Equals(postfix.Base) && Operator == postfix.Operator;
      return false;
    }

    /// <summary>
    /// Gets a hash code for this postfix expression
    /// </summary>
    public override int GetHashCode()
    {
      unchecked
      {
        int hash = 17;
        hash = hash * 23 + Base.GetHashCode();
        hash = hash * 23 + Operator.GetHashCode();
        return hash;
      }
    }
  }
}
