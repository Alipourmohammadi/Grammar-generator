using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace generate_Grammar.Expressions
{
  /// <summary>
  /// Represents a symbol (terminal or non-terminal) in the grammar
  /// </summary>
  public class Symbol : Expression
  {
    /// <summary>
    /// Gets the name of the symbol
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Creates a new symbol with the specified name
    /// </summary>
    public Symbol(string name)
    {
      Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <summary>
    /// Returns a string representation of the symbol
    /// </summary>
    protected override string ToStringImpl() => Name;

    /// <summary>
    /// Checks if this symbol equals another expression
    /// </summary>
    public override bool Equals(Expression other)
    {
      if (other is Symbol symbol)
        return Name == symbol.Name;
      return false;
    }

    /// <summary>
    /// Gets a hash code for this symbol
    /// </summary>
    public override int GetHashCode() => Name.GetHashCode();
  }
}