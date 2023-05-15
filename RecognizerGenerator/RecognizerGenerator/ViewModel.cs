using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecognizerGenerator
{
  public class ViewModel
  {
    public ObservableCollection<InputSymbol> InputSymbols { get; set; } = new() { new("a"), new("b") };
    public ObservableCollection<string> InputSymbolsNames { get; } = new();
    public ObservableCollection<MachineState> States { get; set; } = new() { new("N"), new("E"), new("B") };
    public ObservableCollection<string> StatesNames { get; } = new();
    public MachineState[,] TransitionTable { get; set; } = new MachineState[3, 2] { { new("DICK"), new() }, { new(), new() }, { new(), new() } };

    public ViewModel()
    {
      InputSymbols.ToList().ForEach(s => InputSymbolsNames.Add(s.Name));
      States.ToList().ForEach(s => StatesNames.Add(s.Name));

      InputSymbols.CollectionChanged += UpdateSymbols;
      States.CollectionChanged += UpdateStates;
      FiniteStateMachinePart.PropertyChangedCommon += (s, _) => UpdateSymbols(s, null);
      FiniteStateMachinePart.PropertyChangedCommon += (s, _) => UpdateStates(s, null);
    }

    private void UpdateSymbols(object? sender, NotifyCollectionChangedEventArgs? e)
    {
      InputSymbolsNames.Clear();
      foreach (InputSymbol symbol in InputSymbols)
        InputSymbolsNames.Add(symbol.Name);
    }

    private void UpdateStates(object? sender, NotifyCollectionChangedEventArgs? e)
    {
      StatesNames.Clear();
      foreach (MachineState state in States)
        StatesNames.Add(state.Name);
    }
  }
}
