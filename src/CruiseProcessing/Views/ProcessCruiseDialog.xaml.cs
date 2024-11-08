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
    /// Interaction logic for ProcessCruiseDialog.xaml
    /// </summary>
    public partial class ProcessCruiseDialog : Window
    {
        protected ProcessCruiseDialog()
        {
            InitializeComponent();
        }

        public ProcessCruiseDialog(ProcessCruiseViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        private void _closeButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
