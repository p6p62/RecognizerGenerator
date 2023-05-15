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
    //public ObservableCollection<InputSymbol> InputSymbols { get; set; } = new() { new("a"), new("b") };
    //public ObservableCollection<MachineState> States { get; set; } = new() { new("N"), new("E"), new("B") };
    //public MachineState[,] TransitionTable { get; set; } = new MachineState[2, 3] { { new("DICK"), new(), new() }, { new(), new(), new() } };

    public MainWindow()
    {
      //DataContext = this;
      InitializeComponent();
    }

    private void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
      for (int i = 0; i < TransitionStatesDataGrid.Columns.Count; i++)
      {
        TransitionStatesDataGrid.Columns[i] = new DataGridTemplateColumn() { CellTemplate = (DataTemplate)Resources["DataTemplate_Level2"] };
      }
    }
  }
}
