using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecognizerGenerator
{
  public class MachineState
  {
    public string Name { get; set; } = "";

    public MachineState() { }
    public MachineState(string parName)
    {
      Name = parName;
    }
  }
}
