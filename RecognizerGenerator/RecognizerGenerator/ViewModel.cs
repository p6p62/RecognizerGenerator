using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecognizerGenerator
{
  public class ViewModel
  {
    public ObservableCollection<InputSymbol> InputSymbols { get; set; } = new() { new("a"), new("b") };
    public ObservableCollection<MachineState> States { get; set; } = new() { new("N"), new("E"), new("B") };
    public MachineState[,] TransitionTable { get; set; } = new MachineState[3, 2] { { new("DICK"), new() }, { new(), new() }, { new(), new() } };
  }
}
