using CruiseProcessing.ViewModels;
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
    /// Interaction logic for VolumeEquationsView.xaml
    /// </summary>
    public partial class VolumeEquationsView : Window
    {
        public VolumeEquationsView()
        {
            InitializeComponent();
        }

        public VolumeEquationsView(VolumeEquationsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.Initialize();
            viewModel.VolumeEquationsSaved += OnVolumeEquationsSaved;
        }

        protected override void OnClosed(EventArgs e)
        {
            var viewModel = DataContext as VolumeEquationsViewModel;
            viewModel.VolumeEquationsSaved -= OnVolumeEquationsSaved;

            base.OnClosed(e);
        }

        private void OnVolumeEquationsSaved(object sender, EventArgs e)
        {
            Close();
        }

        private void _cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        
    }
}
