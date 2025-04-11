
namespace generate_Grammar.Expressions
{
  // Base class
  public abstract class Expression
  {
    public override string ToString()
    {
      return ToStringImpl();
    }

    protected abstract string ToStringImpl();

    // Add equality comparison for expressions
    public override bool Equals(object obj)
    {
      if (obj is Expression other)
      {
        return ToString() == other.ToString();
      }
      return false;
    }

    public override int GetHashCode()
    {
      return ToString().GetHashCode();
    }
  }
}
