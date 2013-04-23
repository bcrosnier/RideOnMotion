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
        KinectSensorController _sensorController = null;

        public DepthViewer()
        {
            InitializeComponent();
        }

        public void initializeViewer( KinectSensorController sensorController )
        {
            _sensorController = sensorController;
        }
    }

}
