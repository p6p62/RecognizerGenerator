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
    private TransitionsGraphWindow? _transitionsGraphWindow;

    public MainWindow()
    {
      InitializeComponent();

      _dataContext = (ViewModel)DataContext;
    }

    private void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
      if (_dataContext.InitialState is not null)
      {
        if (_dataContext.States.Any(s => s.IsFinalState))
        {
          List<List<MachineState>> transitionTable = _dataContext.TransitionTable.Select(r => r.ToList()).ToList();
          FiniteStateMachine recognizerFiniteStateMachine = new(_dataContext.States.ToList(), _dataContext.InitialState, _dataContext.InputSymbols.ToList(), transitionTable);

          string[] outputCode = Array.Empty<string>();
          switch (OutputLanguageComboBox.SelectedIndex)
          {
            case 0:
              CodeGeneratorToPascal pascalGenerator = new(recognizerFiniteStateMachine, _dataContext.IsLastCharacterUniversal);
              outputCode = pascalGenerator.GenerateRecognizerCode();
              break;
            case 1:
              CodeGeneratorToPython3 pythonGenerator = new(recognizerFiniteStateMachine, _dataContext.IsLastCharacterUniversal);
              outputCode = pythonGenerator.GenerateRecognizerCode();
              break;
          }

          RecognizerOutputCodeTextBox.Text = string.Join('\n', outputCode);
        }
        else
          MessageBox.Show("Множество конечных состояний автомата пустое", "Ошибка");
      }
      else
        MessageBox.Show("Не задано начальное состояние автомата", "Ошибка");
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
      RecognizerOutputCodeTextBox.Text = "";
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
      Clipboard.SetText(RecognizerOutputCodeTextBox.Text);
    }

    private void GraphButton_Click(object sender, RoutedEventArgs e)
    {
      if (_transitionsGraphWindow == null || !_transitionsGraphWindow.IsLoaded)
        _transitionsGraphWindow = new();
      _transitionsGraphWindow.Show();
      _transitionsGraphWindow.ShowGraph(_dataContext);
    }
  }
}
