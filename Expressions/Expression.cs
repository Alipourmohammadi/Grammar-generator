
namespace generate_Grammar.Expressions
{
  /// <summary>
  /// Base class for all expression types in the grammar
  /// </summary>
  public abstract class Expression : IEquatable<Expression>
  {
    /// <summary>
    /// Returns a string representation of the expression
    /// </summary>
    public override string ToString() => ToStringImpl();

    /// <summary>
    /// Implementation of ToString for derived classes
    /// </summary>
    protected abstract string ToStringImpl();

    /// <summary>
    /// Checks if this expression equals another expression
    /// </summary>
    public virtual bool Equals(Expression other)
    {
      if (other is null) return false;
      if (ReferenceEquals(this, other)) return true;

      // Default implementation compares string representations
      // Derived classes may provide more efficient implementations
      return ToString() == other.ToString();
    }

    /// <summary>
    /// Checks if this expression equals another object
    /// </summary>
    public override bool Equals(object obj)
    {
      if (obj is null) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj is Expression expr) return Equals(expr);
      return false;
    }

    /// <summary>
    /// Gets a hash code for this expression
    /// </summary>
    public override int GetHashCode() => ToString().GetHashCode();

    // Operator overloads for equality comparison
    public static bool operator ==(Expression left, Expression right)
    {
      if (left is null) return right is null;
      return left.Equals(right);
    }

    public static bool operator !=(Expression left, Expression right) => !(left == right);
  }
}
