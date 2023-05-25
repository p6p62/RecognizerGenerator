using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace RecognizerGenerator
{
  public class ViewModel
  {
    public ObservableCollection<InputSymbol> InputSymbols { get; set; } = new()
    {
      new("a") { AcceptedCharactersExpression = "a-zA-Z_" },
      new("b") { AcceptedCharactersExpression = "0-9" },
      new("x")
    };
    public ObservableCollection<string> InputSymbolsNames { get; } = new();

    public ObservableCollection<MachineState> States { get; set; } = new()
    {
      new("S"),
      new("A") { IsFinalState = true },
      new("E")
    };
    public ObservableCollection<string> StatesNames { get; } = new();

    public MachineState? InitialState { get; set; }

    public ObservableCollection<ObservableCollection<MachineState>> TransitionTable { get; set; } = new()
    {
      new() { new("A"), new("E"), new("E") },
      new() { new("A"), new("A"), new("E") },
      new() { new("E"), new("E"), new("E") },
    };

    public bool IsLastCharacterUniversal { get; set; } = true;

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
      InputSymbols.ToList().ForEach(s => InputSymbolsNames.Add(s.Name));
      States.ToList().ForEach(s => StatesNames.Add(s.Name));

      InitialState = States[0];
    }

    private void UpdateSymbols(object? sender, NotifyCollectionChangedEventArgs? e)
    {
      ResizeInputSymbolsColumns(sender, e);
      for (int i = 0; i < InputSymbols.Count; i++)
        InputSymbolsNames[i] = InputSymbols[i].Name;
    }

    private void UpdateStates(object? sender, NotifyCollectionChangedEventArgs? e)
    {
      ResizeStateRows(sender, e);
      if (StatesNames.Zip(States.Select(s => s.Name)).Any(t => t.First != t.Second))
        for (int i = 0; i < States.Count; i++)
          StatesNames[i] = States[i].Name;
    }

    private void ResizeStateRows(object? sender, NotifyCollectionChangedEventArgs? e)
    {
      switch (e?.Action)
      {
        case NotifyCollectionChangedAction.Add:
          StatesNames.Add("");
          string defaultStateName = States.Count > 1 ? States[^2].Name : "";
          TransitionTable.Insert(e.NewStartingIndex, new(new MachineState[InputSymbols.Count].Select(_ => new MachineState(defaultStateName)).ToList()));
          break;
        case NotifyCollectionChangedAction.Remove:
          TransitionTable.RemoveAt(e.OldStartingIndex);
          StatesNames.RemoveAt(e.OldStartingIndex);
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
          InputSymbolsNames.Add("");
          foreach (ObservableCollection<MachineState> stateRow in TransitionTable)
            if (InputSymbols.Count > stateRow.Count)
              stateRow.Insert(e.NewStartingIndex, new(States.Last().Name));
          break;
        case NotifyCollectionChangedAction.Remove:
          foreach (ObservableCollection<MachineState> stateRow in TransitionTable)
            stateRow.RemoveAt(e.OldStartingIndex);
          InputSymbolsNames.RemoveAt(e.OldStartingIndex);
          break;
        case NotifyCollectionChangedAction.Replace:
        case NotifyCollectionChangedAction.Move:
        case NotifyCollectionChangedAction.Reset:
          break;
      }
    }
  }
}
