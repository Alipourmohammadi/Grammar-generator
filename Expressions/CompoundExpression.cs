using System.Text;

namespace generate_Grammar.Expressions
{
  /// <summary>
  /// Represents a compound expression in grammar, combining multiple expressions
  /// through concatenation or alternation.
  /// </summary>
  public class CompoundExpression : Expression
  {
    public enum CompoundType
    {
      Concatenation,  // Implicit sequencing (like 'ab')
      Alternation     // Sum/alternation (like 'a+b')
    }

    public CompoundType Type { get; }
    public List<Expression> Elements { get; } = new List<Expression>();

    public CompoundExpression(CompoundType type, params Expression[] elements)
    {
      Type = type;
      if (elements != null && elements.Length > 0)
      {
        Elements.AddRange(elements);
      }
    }

    /// <summary>
    /// Adds an expression to this compound expression, flattening compound expressions 
    /// of the same type where appropriate.
    /// </summary>
    public void Add(Expression expr)
    {
      // Flatten nested alternations of the same type
      if (expr is CompoundExpression compoundExpr &&
          compoundExpr.Type == Type &&
          Type == CompoundType.Alternation)
      {
        Elements.AddRange(compoundExpr.Elements);
      }
      else
      {
        Elements.Add(expr);
      }
    }

    /// <summary>
    /// Adds an expression if it's not already present in the elements collection.
    /// For alternation expressions, this prevents duplication.
    /// </summary>
    /// <returns>True if the expression was added, false otherwise.</returns>
    public bool AddUnique(Expression expr)
    {
      if (Type == CompoundType.Concatenation)
      {
        // For concatenation, uniqueness doesn't make sense, so just add
        Add(expr);
        return true;
      }

      bool added = false;

      if (expr is CompoundExpression incomingExpr && incomingExpr.Type == CompoundType.Alternation)
      {
        // Handle flattening of nested alternations
        foreach (var element in incomingExpr.Elements)
        {
          // Handle nested alternations recursively to flatten the structure
          if (element is CompoundExpression nestedAlt && nestedAlt.Type == CompoundType.Alternation)
          {
            if (AddUnique(nestedAlt))
              added = true;
          }
          else if (!Elements.Any(e => e.Equals(element)))
          {
            Elements.Add(element);
            added = true;
          }
        }
      }
      else if (!Elements.Any(e => e.Equals(expr)))
      {
        Elements.Add(expr);
        added = true;
      }

      return added;
    }

    /// <summary>
    /// Inserts an expression at the beginning of the elements list.
    /// </summary>
    public void Insert(Expression expr)
    {
      Elements.Insert(0, expr);
    }

    /// <summary>
    /// Returns the first element in the list.
    /// </summary>
    public Expression GetFirst()
    {
      return Elements.FirstOrDefault() ?? new Symbol("λ"); // Return lambda as fallback
    }


    protected override string ToStringImpl()
    {
      if (Elements.Count == 0)
        return "";

      StringBuilder sb = new StringBuilder();

      for (int i = 0; i < Elements.Count; i++)
      {
        if (i > 0 && Type == CompoundType.Alternation)
          sb.Append("U");
        sb.Append(Elements[i].ToString());
      }

      return sb.ToString();
    }


    /// <summary>
    /// Determines whether this compound expression is equal to another expression.
    /// </summary>
    /// <param name="other">The expression to compare with</param>
    /// <returns>True if the expressions are equal, false otherwise</returns>
    public override bool Equals(Expression other)
    {
      return Equals(other as CompoundExpression);
    }

    /// <summary>
    /// Determines whether this compound expression is equal to another compound expression.
    /// </summary>
    /// <param name="other">The compound expression to compare with</param>
    /// <returns>True if the compound expressions are equal, false otherwise</returns>
    public bool Equals(CompoundExpression other)
    {
      if (other is null) return false;
      if (ReferenceEquals(this, other)) return true;

      if (Type != other.Type || Elements.Count != other.Elements.Count)
        return false;

      // For alternation, order doesn't matter
      if (Type == CompoundType.Alternation)
      {
        // Check if both expressions contain the same elements regardless of order
        return !Elements.Except(other.Elements, new ExpressionEqualityComparer()).Any() &&
               !other.Elements.Except(Elements, new ExpressionEqualityComparer()).Any();
      }
      else
      {
        // For concatenation, order matters
        for (int i = 0; i < Elements.Count; i++)
        {
          if (!Elements[i].Equals(other.Elements[i]))
            return false;
        }
        return true;
      }
    }

    /// <summary>
    /// Returns a hash code for this compound expression.
    /// </summary>
    /// <returns>A hash code</returns>
    public override int GetHashCode()
    {
      unchecked
      {
        int hash = 17;
        hash = hash * 23 + Type.GetHashCode();

        if (Type == CompoundType.Alternation)
        {
          // For alternation, order doesn't matter, so use XOR
          foreach (var element in Elements)
          {
            hash ^= element.GetHashCode();
          }
        }
        else
        {
          // For concatenation, order matters
          foreach (var element in Elements)
          {
            hash = hash * 23 + element.GetHashCode();
          }
        }

        return hash;
      }
    }
  }

  /// <summary>
  /// Equality comparer for expressions used in collection operations
  /// </summary>
  internal class ExpressionEqualityComparer : IEqualityComparer<Expression>
  {
    public bool Equals(Expression x, Expression y)
    {
      if (x == null && y == null) return true;
      if (x == null || y == null) return false;
      return x.Equals(y);
    }

    public int GetHashCode(Expression obj)
    {
      return obj?.GetHashCode() ?? 0;
    }
  }

}