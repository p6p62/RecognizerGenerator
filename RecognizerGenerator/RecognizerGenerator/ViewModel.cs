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
    public ObservableCollection<ObservableCollection<MachineState>> TransitionTable { get; set; } = new();

    public ViewModel()
    {
      FillBeginInitializationData();

      InputSymbols.CollectionChanged += UpdateSymbols;
      States.CollectionChanged += UpdateStates;
      FiniteStateMachinePart.PropertyChangedCommon += (s, _) => UpdateSymbols(s, null);
      FiniteStateMachinePart.PropertyChangedCommon += (s, _) => UpdateStates(s, null);
    }

    private void FillBeginInitializationData()
    {
      for (int i = 0; i < States.Count; i++)
      {
        ObservableCollection<MachineState> l = new();
        for (int j = 0; j < InputSymbols.Count; j++)
          l.Add(new());
        TransitionTable.Add(l);
      }

      InputSymbols.ToList().ForEach(s => InputSymbolsNames.Add(s.Name));
      States.ToList().ForEach(s => StatesNames.Add(s.Name));
    }

    private void UpdateSymbols(object? sender, NotifyCollectionChangedEventArgs? e)
    {
      ResizeInputSymbolsColumns(sender, e);
      InputSymbolsNames.Clear();
      foreach (InputSymbol symbol in InputSymbols)
        InputSymbolsNames.Add(symbol.Name);
    }

    private void UpdateStates(object? sender, NotifyCollectionChangedEventArgs? e)
    {
      ResizeStateRows(sender, e);
      StatesNames.Clear();
      foreach (MachineState state in States)
        StatesNames.Add(state.Name);
    }

    private void ResizeStateRows(object? sender, NotifyCollectionChangedEventArgs? e)
    {
      switch (e?.Action)
      {
        case NotifyCollectionChangedAction.Add:
          TransitionTable.Insert(e.NewStartingIndex, new(new MachineState[InputSymbols.Count].Select(_ => new MachineState()).ToList()));
          break;
        case NotifyCollectionChangedAction.Remove:
          TransitionTable.RemoveAt(e.OldStartingIndex);
          break;
        case NotifyCollectionChangedAction.Replace:
        case NotifyCollectionChangedAction.Move:
        case NotifyCollectionChangedAction.Reset:
          break;
      }
    }

    private void ResizeInputSymbolsColumns(object? sender, NotifyCollectionChangedEventArgs? e)
    {
      switch (e?.Action)
      {
        case NotifyCollectionChangedAction.Add:
          foreach (ObservableCollection<MachineState> stateRow in TransitionTable)
            stateRow.Insert(e.NewStartingIndex, new());
          break;
        case NotifyCollectionChangedAction.Remove:
          foreach (ObservableCollection<MachineState> stateRow in TransitionTable)
            stateRow.RemoveAt(e.OldStartingIndex);
          break;
        case NotifyCollectionChangedAction.Replace:
        case NotifyCollectionChangedAction.Move:
        case NotifyCollectionChangedAction.Reset:
          break;
      }
    }
  }
}
