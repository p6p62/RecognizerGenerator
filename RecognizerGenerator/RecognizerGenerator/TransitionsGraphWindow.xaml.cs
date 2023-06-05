using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.WpfGraphControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RecognizerGenerator
{
  /// <summary>
  /// Логика взаимодействия для TransitionsGraphWindow.xaml
  /// </summary>
  public partial class TransitionsGraphWindow : Window
  {
    private readonly GraphViewer _graphViewer = new();
    private readonly Graph _transitionsGraph = new();

    public TransitionsGraphWindow()
    {
      InitializeComponent();
      _graphViewer.BindToPanel(TransitionsGraphDockPanel);
      _transitionsGraph.Attr.LayerDirection = LayerDirection.LR;
    }

    public void ShowGraph(ViewModel dataContext)
    {
      _transitionsGraph.Edges.ToList().ForEach(e => _transitionsGraph.RemoveEdge(e));
      _transitionsGraph.Nodes.ToList().ForEach(n => _transitionsGraph.RemoveNode(n));

      // добавление состояний
      foreach (MachineState state in dataContext.States)
        _transitionsGraph.AddNode(state.Name).Attr.Shape = Microsoft.Msagl.Drawing.Shape.Circle;

      // настройка связей
      for (int i = 0; i < dataContext.States.Count; i++)
      {
        string sourceStateName = dataContext.States[i].Name;
        for (int j = 0; j < dataContext.InputSymbols.Count; j++)
        {
          string targetStateName = dataContext.TransitionTable[i][j].Name;
          string inputSymbolName = dataContext.InputSymbols[j].Name;
          Edge? edge = _transitionsGraph.Edges.SingleOrDefault(o => o.Source == sourceStateName && o.Target == targetStateName);
          if (edge != null)
            edge.LabelText = $"{edge.LabelText}, {inputSymbolName}";
          else
            _transitionsGraph.AddEdge(sourceStateName, inputSymbolName, targetStateName);
        }
      }

      // настройка отображения стрелки у начального состояния
      if (dataContext.InitialState != null)
      {
        string arrowNodeId = "Initial arrow node";
        Node initialArrowNode = _transitionsGraph.AddNode(arrowNodeId);
        initialArrowNode.IsVisible = false;
        _transitionsGraph.AddEdge(arrowNodeId, dataContext.InitialState.Name);
      }

      // настройка отображения конечных состояний
      foreach (MachineState state in dataContext.States)
        if (state.IsFinalState)
          _transitionsGraph.FindNode(state.Name).Attr.Shape = Microsoft.Msagl.Drawing.Shape.DoubleCircle;

      _graphViewer.Graph = _transitionsGraph;
    }
  }
}
