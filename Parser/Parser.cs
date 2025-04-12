using generate_Grammar.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace generate_Grammar.Parser
{
  /// <summary>
  /// Parser for grammar expressions
  /// </summary>
  public class Parser
  {
    private readonly string _input;
    private readonly string _originalInput;
    private int _position;

    /// <summary>
    /// Creates a new parser for the specified input
    /// </summary>
    public Parser(string input)
    {
      _originalInput = input ?? throw new ArgumentNullException(nameof(input));
      _input = RemoveLambdaSymbol();
      _position = 0;
    }

    /// <summary>
    /// Parses the input into an expression
    /// </summary>
    public Expression Parse()
    {
      if (string.IsNullOrEmpty(_input))
        throw new InvalidOperationException("Input is empty");

      Expression result = ParseExpression();

      if (_position < _input.Length)
      {
        throw new ParseException($"Unexpected characters at position {_position}: {_input.Substring(_position)}", _position);
      }

      return result;
    }

    /// <summary>
    /// Removes lambda symbols from the input
    /// </summary>
    private string RemoveLambdaSymbol()
    {
      // This method appears to be removing lambda symbol and adjacent characters
      // which seems suspicious. Consider revising the logic based on requirements.
      int lambdaIndex = _originalInput.IndexOf('λ');
      if (lambdaIndex == -1)
      {
        return _originalInput;
      }

      StringBuilder result = new StringBuilder(_originalInput.Length);
      for (int i = 0; i < _originalInput.Length; i++)
      {
        // Skip lambda and adjacent characters
        if (i == lambdaIndex || i == lambdaIndex - 1 || i == lambdaIndex + 1)
          continue;

        result.Append(_originalInput[i]);
      }

      return result.ToString();
    }

    /// <summary>
    /// Parses an expression (alternation)
    /// </summary>
    private Expression ParseExpression()
    {
      Expression left = ParseTerm();

      while (_position < _input.Length && Peek() == '+')
      {
        // Found a '+', so this is an alternation/sum
        Consume(); // consume '+'

        CompoundExpression alternation;
        if (left is CompoundExpression compoundLeft && compoundLeft.Type == CompoundExpression.CompoundType.Alternation)
        {
          // Continue building the existing alternation
          alternation = compoundLeft;
        }
        else
        {
          // Create a new alternation
          alternation = new CompoundExpression(CompoundExpression.CompoundType.Alternation);
          alternation.Add(left);
        }

        alternation.Add(ParseTerm());
        left = alternation;
      }

      return left;
    }

    /// <summary>
    /// Parses a term (concatenation)
    /// </summary>
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
        {
          // Continue building the existing concatenation
          concat = compoundResult;
        }
        else
        {
          // Create a new concatenation
          concat = new CompoundExpression(CompoundExpression.CompoundType.Concatenation);
          concat.Add(result);
        }

        concat.Add(ParseFactor());
        result = concat;
      }

      return result;
    }

    /// <summary>
    /// Parses a factor (symbol, parenthesized expression, or postfix expression)
    /// </summary>
    private Expression ParseFactor()
    {
      Expression expr;

      if (_position >= _input.Length)
        throw new ParseException("Unexpected end of input", _position);

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
        throw new ParseException($"Unexpected character at position {_position}: {Peek()}", _position);
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
            // Get all digits of the number
            StringBuilder numberBuilder = new StringBuilder();
            while (_position < _input.Length && char.IsNumber(Peek()))
            {
              numberBuilder.Append(ConsumeChar());
            }
            expr = new PostfixExpression(expr, "^" + numberBuilder.ToString());
          }
          else
          {
            // Just '^' with no number or '+' after it is probably an error
            throw new ParseException("Expected a number or '+' after '^'", _position);
          }
        }
        else
        {
          break;
        }
      }

      return expr;
    }

    /// <summary>
    /// Gets the set of unique letters in the input
    /// </summary>
    public HashSet<char> GetUniqueLetters()
    {
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

    /// <summary>
    /// Peeks at the current character without consuming it
    /// </summary>
    private char Peek()
    {
      if (_position >= _input.Length)
        throw new ParseException("Unexpected end of input", _position);

      return _input[_position];
    }

    /// <summary>
    /// Consumes and returns the current character
    /// </summary>
    private char ConsumeChar()
    {
      if (_position >= _input.Length)
        throw new ParseException("Unexpected end of input", _position);

      return _input[_position++];
    }

    /// <summary>
    /// Consumes the current character without returning it
    /// </summary>
    private void Consume()
    {
      if (_position >= _input.Length)
        throw new ParseException("Unexpected end of input", _position);

      _position++;
    }

    /// <summary>
    /// Expects and consumes a specific character
    /// </summary>
    private void Expect(char expected)
    {
      if (_position >= _input.Length)
        throw new ParseException($"Expected '{expected}' but found end of input", _position);

      if (_input[_position] != expected)
        throw new ParseException($"Expected '{expected}' at position {_position} but found '{_input[_position]}'", _position);

      _position++;
    }
  }

  /// <summary>
  /// Exception thrown when parsing fails
  /// </summary>
  public class ParseException : Exception
  {
    /// <summary>
    /// Gets the position in the input where the error occurred
    /// </summary>
    public int Position { get; }

    /// <summary>
    /// Creates a new parse exception with the specified message and position
    /// </summary>
    public ParseException(string message, int position)
        : base(message)
    {
      Position = position;
    }
  }
}