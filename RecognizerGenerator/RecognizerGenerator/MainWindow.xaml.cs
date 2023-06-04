using Gu.Wpf.DataGrid2D;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.WpfGraphControl;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RecognizerGenerator
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private readonly ViewModel _dataContext;
    private readonly GraphViewer _graphViewer;
    private readonly Graph _transitionsGraph;

    public MainWindow()
    {
      InitializeComponent();

      _graphViewer = new();
      _graphViewer.BindToPanel(TransitionsGraphDockPanel);
      _transitionsGraph = new();
      _transitionsGraph.Attr.LayerDirection = LayerDirection.LR;

      _dataContext = (ViewModel)DataContext;
    }

    private void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
      List<List<MachineState>> transitionTable = _dataContext.TransitionTable.Select(r => r.ToList()).ToList();
      FiniteStateMachine recognizerFiniteStateMachine = new(_dataContext.States.ToList(),
        _dataContext.InitialState ?? _dataContext.States.Last(), _dataContext.InputSymbols.ToList(), transitionTable);

      CodeGeneratorToPascal generator = new(recognizerFiniteStateMachine, _dataContext.IsLastCharacterUniversal);
      string[] outputCode = generator.GenerateRecognizerCode();

      RecognizerOutputCodeTextBox.Text = string.Join('\n', outputCode);
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
      RecognizerOutputCodeTextBox.Text = "";
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
      Clipboard.SetText(RecognizerOutputCodeTextBox.Text);
    }

    private void ClearGraph()
    {
      _transitionsGraph.Edges.ToList().ForEach(edge => _transitionsGraph.RemoveEdge(edge));
      _transitionsGraph.Nodes.ToList().ForEach(node => _transitionsGraph.RemoveNode(node));
    }

    private void GraphButton_Click(object sender, RoutedEventArgs e)
    {
      ClearGraph();

      // добавление состояний
      foreach (MachineState state in _dataContext.States)
        _transitionsGraph.AddNode(state.Name).Attr.Shape = Microsoft.Msagl.Drawing.Shape.Circle;

      // настройка связей
      for (int i = 0; i < _dataContext.States.Count; i++)
      {
        string sourceStateName = _dataContext.States[i].Name;
        for (int j = 0; j < _dataContext.InputSymbols.Count; j++)
        {
          string targetStateName = _dataContext.TransitionTable[i][j].Name;
          string inputSymbolName = _dataContext.InputSymbols[j].Name;
          Edge? edge = _transitionsGraph.Edges.SingleOrDefault(o => o.Source == sourceStateName && o.Target == targetStateName);
          if (edge != null)
            edge.LabelText = $"{edge.LabelText}, {inputSymbolName}";
          else
            _transitionsGraph.AddEdge(sourceStateName, inputSymbolName, targetStateName);
        }
      }

      // настройка отображения стрелки у начального состояния
      if (_dataContext.InitialState != null)
      {
        string arrowNodeId = "Initial arrow node";
        Node initialArrowNode = _transitionsGraph.AddNode(arrowNodeId);
        initialArrowNode.IsVisible = false;
        _transitionsGraph.AddEdge(arrowNodeId, _dataContext.InitialState.Name);
      }

      foreach (MachineState state in _dataContext.States)
        if (state.IsFinalState)
          _transitionsGraph.FindNode(state.Name).Attr.Shape = Microsoft.Msagl.Drawing.Shape.DoubleCircle;

      _graphViewer.Graph = _transitionsGraph;
    }
  }
}
