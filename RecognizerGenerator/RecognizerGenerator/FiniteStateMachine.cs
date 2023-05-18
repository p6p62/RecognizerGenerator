using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecognizerGenerator
{
  class FiniteStateMachine
  {
    public List<MachineState> States { get; set; }
    public MachineState InitialState { get; set; }
    public List<InputSymbol> InputSymbols { get; set; }
    public List<List<MachineState>> TransitionTable { get; set; }

    public FiniteStateMachine(List<MachineState> states, MachineState initialState, List<InputSymbol> inputSymbols, List<List<MachineState>> transitionTable)
    {
      States = states;
      InitialState = initialState;
      InputSymbols = inputSymbols;
      TransitionTable = transitionTable;
    }
  }
}
