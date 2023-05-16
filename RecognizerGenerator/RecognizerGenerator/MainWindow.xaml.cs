using Gu.Wpf.DataGrid2D;
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

    public MainWindow()
    {
      InitializeComponent();
      TransitionStatesDataGrid.SetEditingTemplate((DataTemplate?)Resources["DataTemplate_StateChoose"]);
      _dataContext = (ViewModel)DataContext;
    }

    private void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
    }
  }
}
