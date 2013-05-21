using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace RideOnMotion.UI
{
    /// <summary>
    /// Interaction logic for DroneSettingsWindow.xaml
    /// </summary>
    public partial class DroneSettingsWindow : Window
    {
        private DroneSettingsWindowViewModel _viewModel;

        public DroneSettingsWindow()
        {
            _viewModel = new DroneSettingsWindowViewModel();
            InitializeComponent();
        }

        private void ButtonOK_Click( object sender, RoutedEventArgs e )
        {
            this.Close();
        }

        private void ButtonCancel_Click( object sender, RoutedEventArgs e )
        {
            this.Close();
        }

        private void ButtonApply_Click( object sender, RoutedEventArgs e )
        {

        }
    }

    public class DroneSettingsWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
