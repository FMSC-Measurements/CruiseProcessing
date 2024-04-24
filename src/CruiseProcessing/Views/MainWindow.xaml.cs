using CruiseProcessing.Services;
using CruiseProcessing.ViewModel;
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

namespace CruiseProcessing.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        protected MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(DialogService dialogService, MainWindowViewModel viewModel)
            : this()
        {
            DataContext = viewModel;
        }

        private void OnReportsClicked(object sender, RoutedEventArgs e)
        {
            _tabControl.SelectedItem = _reportsPage;
        }

        private void OnOutputClicked(object sender, RoutedEventArgs e)
        {
            _tabControl.SelectedItem = _outputPage;
        }

        private void OnEquationsClicked(object sender, RoutedEventArgs e)
        {
            _tabControl.SelectedItem = _equationsPage;
        }
    }
}
