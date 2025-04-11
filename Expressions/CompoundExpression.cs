
using System.Text;


namespace generate_Grammar.Expressions
{
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
      Elements.AddRange(elements);
    }

    public void Add(Expression expr)
    {
      if (expr is CompoundExpression CEX && CEX.Type == CompoundType.Alternation && Type == CompoundType.Alternation)
      {
        foreach (var item in CEX.Elements)
        {
          Elements.Add(item);
        }
      }
      else
      {
        Elements.Add(expr);
      }
    }
    public bool AddUnique(Expression expr)
    {
      // Check if the expression is already in the list
      if (Type == CompoundType.Concatenation)
        return false;
      if (expr is CompoundExpression CEX && CEX.Type == CompoundType.Alternation)
      {
        foreach (var item in CEX.Elements)
        {
          if (item is CompoundExpression cex && cex.Type == CompoundType.Alternation)
          {
            AddUnique(cex);
          }
          else if (!Elements.Any(e => e.Equals(item)))
          {
            Add(item);
          }
        }
        return true;
      }

      if (!Elements.Any(e => e.Equals(expr)))
      {
        Add(expr);
        return true;
      }
      return false;
    }
    //public bool AddUnique(Expression expr)
    //{
    //  bool added = false;

    //  // For concatenation types, we should still allow adding unique elements
    //  // (removing the automatic return false)

    //  if (expr is CompoundExpression incomingExpr && incomingExpr.Type == CompoundType.Alternation)
    //  {
    //    if (this.Type == CompoundType.Alternation)
    //    {
    //      foreach (var element in incomingExpr.Elements)
    //      {
    //        // Handle nested alternations recursively, flattening the structure
    //        if (element is CompoundExpression nestedAlt && nestedAlt.Type == CompoundType.Alternation)
    //        {
    //          // Recursively add elements from the nested alternation
    //          if (AddUnique(nestedAlt))
    //            added = true;
    //        }
            
    //        else if (!Elements.Any(e => e.Equals(element)))
    //        {
    //          Elements.Add(element);
    //          added = true;
    //        }
    //      }
    //      return added;
    //    }
    //    else if (!Elements.Any(e => e.Equals(expr)))
    //    {
    //      Elements.Add(expr);
    //      return true;
    //    }
    //  }
    //  else if (!Elements.Any(e => e.Equals(expr)))
    //  {
    //    Elements.Add(expr);
    //    return true;
    //  }

    //  return added;
    //}
    // insert Expression to begging of the elements
    public void Insert(Expression expr)
    {
      Elements.Insert(0, expr);
    }
    public Expression getFirst()
    {
      return Elements.First();
    }

    protected override string ToStringImpl()
    {
      if (Elements.Count == 0)
        return "";

      StringBuilder sb = new StringBuilder();
      //sb.Append("(");
      for (int i = 0; i < Elements.Count; i++)
      {
        if (i > 0 && Type == CompoundType.Alternation)
          sb.Append("+");
        sb.Append(Elements[i].ToString());
      }
      //sb.Append(")");
      return sb.ToString();
    }
  }
}
