using RideOnMotion.KinectModule;
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

namespace RideOnMotion
{
    /// <summary>
    /// Interaction logic for DepthViewer.xaml
    /// </summary>
    public partial class DepthViewer : UserControl
    {
        private KinectSensorController _sensorController = null;

        public DepthViewer()
        {
            InitializeComponent();
        }

        public void initializeViewer( KinectSensorController sensorController )
        {
            _sensorController = sensorController;
            _sensorController.DepthBitmapSourceReady += _sensorController_DepthBitmapSourceReady;

            updateImageBitmapSource(_sensorController.DepthBitmapSource);
        }

        void _sensorController_DepthBitmapSourceReady( object sender, BitmapSourceEventArgs e )
        {
            updateImageBitmapSource(e.BitmapSource);
        }

        private void updateImageBitmapSource( BitmapSource source )
        {
            this.DepthImage.Source = source;
        }

    }

}
