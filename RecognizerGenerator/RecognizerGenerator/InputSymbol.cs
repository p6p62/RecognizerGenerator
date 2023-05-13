using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecognizerGenerator
{
  public class InputSymbol
  {
    public string Name { get; set; } = "";
    public string AcceptedCharactersExpression { get; set; } = "";

    public InputSymbol() { }
    public InputSymbol(string parName)
    {
      Name = parName;
    }
  }
}
