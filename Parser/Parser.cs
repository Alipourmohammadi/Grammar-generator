using generate_Grammar.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace generate_Grammar.Parser
{
  public class Parser
  {
    private string _input;
    private string _initialInput;
    private int _position;

    public Parser(string input)
    {
      _initialInput = input;
      _input = clearelambdda();
      _position = 0;
    }

    public Expression Parse()
    {
      Expression result = ParseExpression();

      if (_position < _input.Length)
      {
        throw new Exception($"Unexpected characters at position {_position}: {_input.Substring(_position)}");
      }

      return result;
    }
    private string clearelambdda()
    {
      StringBuilder result = new StringBuilder();
      var lamdaIndex = _initialInput.IndexOf('λ');
      if (lamdaIndex == -1)
      {
        return _initialInput;
      }
      else
      {
        for (global::System.Int32 i = 0; i < _initialInput.Length; i++)
        {
          if (!(i == lamdaIndex || i == lamdaIndex - 1 || i == lamdaIndex + 1))
          {
            result.Append(_initialInput[i]);
          }
        }
      }
      return result.ToString();
    }

    private Expression ParseExpression()
    {
      Expression left = ParseTerm();

      while (_position < _input.Length && Peek() == '+')
      {
        // Found a '+', so this is an alternation/sum
        Consume(); // consume '+'
        CompoundExpression alternation;

        if (left is CompoundExpression compoundLeft && compoundLeft.Type == CompoundExpression.CompoundType.Alternation)
          alternation = compoundLeft;
        else
        {
          alternation = new CompoundExpression(CompoundExpression.CompoundType.Alternation);
          alternation.Add(left);
        }

        alternation.Add(ParseTerm());
        left = alternation;
      }

      return left;
    }

    private Expression ParseTerm()
    {
      Expression result = ParseFactor();

      // Check for concatenation (implicit sequencing)
      while (_position < _input.Length &&
            (char.IsLetterOrDigit(Peek()) || Peek() == '(') &&
            Peek() != '+')
      {
        CompoundExpression concat;

        if (result is CompoundExpression compoundResult && compoundResult.Type == CompoundExpression.CompoundType.Concatenation)
          concat = compoundResult;
        else
        {
          concat = new CompoundExpression(CompoundExpression.CompoundType.Concatenation);
          concat.Add(result);
        }
        var x = ParseFactor();
        concat.Add(x);
        result = concat;
      }

      return result;
    }

    private Expression ParseFactor()
    {
      Expression expr;

      if (Peek() == '(')
      {
        Consume(); // consume '('
        expr = ParseExpression();
        Expect(')'); // expect and consume ')'
      }
      else if (char.IsAsciiLetter(Peek()))
      {
        expr = new Symbol(ConsumeChar().ToString());
      }
      else
      {
        throw new Exception($"Unexpected character at position {_position}: {Peek()}");
      }

      // Check for postfix operators
      while (_position < _input.Length)
      {
        if (Peek() == '*')
        {
          Consume(); // consume '*'
          expr = new PostfixExpression(expr, "*");
        }
        else if (Peek() == '^')
        {
          Consume(); // consume '^'

          // Check if there's a '+' after the '^'
          if (_position < _input.Length && Peek() == '+')
          {
            Consume(); // consume '+' as part of '^+' operator
            expr = new PostfixExpression(expr, "^+");
          }
          else if (_position < _input.Length && char.IsNumber(Peek()))
          {
            expr = new PostfixExpression(expr, "^" + ConsumeChar());
          }
          else
          {
            //Error?
            expr = new PostfixExpression(expr, "^");
          }
        }
        else
        {
          break;
        }
      }

      return expr;
    }

    public HashSet<char> GetUniqueLetters()
    {
      if (string.IsNullOrEmpty(_input))
      {
        return new HashSet<char>();
      }

      HashSet<char> uniqueLetters = new HashSet<char>();

      foreach (char c in _input)
      {
        if (char.IsAsciiLetter(c))
        {
          uniqueLetters.Add(c);
        }
      }

      return uniqueLetters;
    }

    private char Peek()
    {
      return _input[_position];
    }

    private char ConsumeChar()
    {
      return _input[_position++];
    }

    private void Consume()
    {
      _position++;
    }

    private void Expect(char expected)
    {
      if (_position >= _input.Length || _input[_position] != expected)
      {
        throw new Exception($"Expected '{expected}' at position {_position}");
      }
      _position++;
    }
  }
}
