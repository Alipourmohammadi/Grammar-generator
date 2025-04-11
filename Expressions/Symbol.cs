using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace generate_Grammar.Expressions
{
  public class Symbol : Expression
  {
    public string Name { get; }

    public Symbol(string name)
    {
      Name = name;
    }

    protected override string ToStringImpl()
    {
      return Name;
    }
  }
}
