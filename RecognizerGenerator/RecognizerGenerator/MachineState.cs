using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RecognizerGenerator
{
  public class MachineState : FiniteStateMachinePart
  {
    private bool _isFinalState = false;
    public bool IsFinalState
    {
      get => _isFinalState;
      set => SetField(ref _isFinalState, value);
    }
    public MachineState() { }
    public MachineState(string parName) : base(parName) { }
  }
}
