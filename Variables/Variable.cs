

using generate_Grammar.Expressions;

namespace generate_Grammar.Variables
{
  public class Variable
  {

    public Expression expr { get; set; }
    public char Name { get; set; }
    public bool prosced { get; set; }

    public List<string> GrammarResults { get; set; }
    public Variable(char name, Expression Expr)
    {
      Name = name;
      expr = Expr;
    }
  }
}
