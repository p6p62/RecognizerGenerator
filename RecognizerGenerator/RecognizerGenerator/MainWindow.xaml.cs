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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RecognizerGenerator
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public List<InputSymbol> InputSymbols { get; set; } = new() { new("a"), new("b") };
    public List<MachineState> States { get; set; } = new() { new("N"), new("E"), new("B") };

    public MainWindow()
    {
      InitializeComponent();
      DataContext = this;
    }

    private void GenerateButton_Click(object sender, RoutedEventArgs e)
    {

    }
  }
}
