// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;

// namespace generate_Grammar
// {
//   public class Grammar
//   {
//     String Equation = "";
//     public Grammar(String input_equation)
//     {
//       Equation = input_equation;
//     }
//     public List<String> getSmallest()
//     {
//       var starIndex = Equation.IndexOf('*');
//       var tempEquation = new StringBuilder(Equation);
//       while (starIndex > 0)
//       {
//         tempEquation[starIndex] = '0';
//         starIndex = Equation.IndexOf('*', starIndex);
//       }

//       var plusIndex = Equation.IndexOf("^+");
//       while (plusIndex > 0)
//       {
//         tempEquation[plusIndex+1] = '1';
//         plusIndex = Equation.IndexOf("^+");
//       }

//     }
//     public String evaluateEquation()
//     public String whatDoYouSeeFor(String in_alphabet)
//     {

//     }
//   }
// }
