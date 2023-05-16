using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecognizerGenerator
{
  public class InputSymbol : FiniteStateMachinePart
  {
    public string AcceptedCharactersExpression { get; set; } = "";
    public InputSymbol() { }
    public InputSymbol(string parName) : base(parName) { }
  }
}
