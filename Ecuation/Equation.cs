//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace generate_Grammar.Ecuation
//{
//  public class Equation
//  {
//    public List<Element> equations;
//    public Equation(string equationString)
//    {
//      equations = ParseEquation(equationString);
//    }

//    private List<Element> ParseEquation(string equationString)
//    {
//      var elements = new List<Element>();
//      var currentSign = 1;
//      for (int i = 0; i < equationString.Length; i++)
//      {
//        char currentChar = equationString[i];
//        if (currentChar == '(')
//        {
//          int closingIndex = FindClosingParenthesis(equationString, i);
//          if (closingIndex == -1) throw new ArgumentException("Mismatched parentheses in equation string.");

//          string subEquation = equationString.Substring(i + 1, closingIndex - i - 1);
//          i = closingIndex;

//          ToPower power = ToPower.one;
//          if (i + 1 < equationString.Length && (equationString[i + 1] == '*' || equationString[i + 1] == '^'))
//          {
//            power = equationString[i + 1] == '*' ? ToPower.Star : ToPower.Plus;
//            i++;
//          }

//          elements.Add(new Element { subElements = ParseEquation(subEquation), power = power });
//        }
//        else if (char.IsLetter(currentChar))
//        {
//          ToPower power = ToPower.one;
//          if (i + 1 < equationString.Length && (equationString[i + 1] == '*' || equationString[i + 1] == '^'))
//          {
//            power = equationString[i + 1] == '*' ? ToPower.Star : ToPower.Plus;
//            i++;
//          }

//          elements.Add(new Element { alphabet = currentChar, power = power });
//        }
//        else if (char.IsAscii('+'))
//        {

//        }
//      }
//      return elements;
//    }

//    private int FindClosingParenthesis(string equationString, int openIndex)
//    {
//      int depth = 1;
//      for (int i = openIndex + 1; i < equationString.Length; i++)
//      {
//        if (equationString[i] == '(') depth++;
//        if (equationString[i] == ')') depth--;
//        if (depth == 0) return i;
//      }
//      return -1;
//    }
//  }
//}