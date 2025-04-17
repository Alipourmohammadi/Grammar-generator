# Regular Expression Grammar Generator

A C# application that converts regular expressions into corresponding regular grammar. This tool parses regular expressions and generates equivalent grammar rules, which can be useful for language processing, compiler design, and theoretical computer science applications.

## Overview

This project implements an algorithm to transform regular expressions into regular grammar by:
1. Parsing regular expressions into an abstract syntax tree
2. Calculating grammar productions for each expression component
3. Generating a complete set of grammar rules that recognize the same language as the input regular expression

## Features

- Supports standard regular expression operators:
  - Concatenation (implicit sequencing like `ab`)
  - Alternation (sum/choice like `aUb`)
  - Kleene star (`*` for zero or more repetitions)
  - One or more repetitions (`+`)
- Handles complex nested expressions with parentheses
- Generates non-redundant grammar variables 
- Proper handling of lambda (ε) transitions

## Structure

The project is organized into several components:

### Expression Classes
- `Expression`: Base abstract class for all expression types
- `Symbol`: Represents a terminal symbol or atomic element
- `CompoundExpression`: Handles concatenation and alternation operations
- `PostfixExpression`: Manages postfix operators like `*` and `+`

### Parsing and Grammar Generation
- `Parser`: Converts regular expression strings into expression objects
- `GrammarGenerator`: Transforms parsed expressions into grammar rules
- `Variable`: Represents a non-terminal symbol in the generated grammar

## Usage

```csharp
// Create a parser for your regular expression
Parser parser = new Parser("a*(cUb)Ua");

// Parse the expression
Expression expr = parser.Parse();

// Generate grammar from the expression
GrammarGenerator generator = new GrammarGenerator(expr, parser.GetUniqueLetters());
generator.calculateGrammar();

// Display the resulting grammar
generator.printGrammar();
```

## Example

For the regular expression `a*(cUb)Ua`, the program will:
1. Parse it into an abstract syntax tree
2. Calculate grammar productions for each sub-expression
3. Output a context-free grammar that recognizes the same language

Output:
```
Parsed equation : a*(cUb)Ua

Generated Grammar:
S -> a|aA|aC

A -> a|aA|aC|c|b

C -> a|aA|aC|c|b
```

## Getting Started

### Prerequisites
- .NET SDK 6.0 or later

### Building the Project
1. Clone the repository
2. Navigate to the project directory
3. Run `dotnet build` to build the project

### Running Examples
```
dotnet run
```

## Algorithm Overview

The grammar generation algorithm works by:
1. Creating an initial variable `S` for the entire expression
2. Breaking down complex expressions into simpler components
3. Calculating which characters each component can start with
4. Creating grammar rules based on these first characters and remaining expressions
5. Handling special cases like Kleene star and alternation

## Contributing

Contributions are welcome! Feel free to submit issues or pull requests.

## License

This project is open source and available under the [MIT License](LICENSE).