using System.Windows;
using Microsoft.Kinect;
using KinectStatusNotifier;
using RideOnMotion.KinectModule;

namespace RideOnMotion
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		/// <summary>
		/// Active Kinect sensor controller.
		/// </summary>
        private KinectSensorController sensorController;

		/// <summary>
		/// Kinect Status Notifier. Notably used by the Kinect system tray icon.
		/// </summary>
		private StatusNotifier notifier = new StatusNotifier();

		public MainWindow()
		{
            InitializeComponent();

            this.sensorController = new KinectSensorController();
		}

		/// <summary>
		/// Status changed for Kinect sensor
		/// </summary>
		/// <param name="sender">Kinect Sensor</param>
		/// <param name="e">Event Args</param>
		void KinectSensors_StatusChanged( object sender, StatusChangedEventArgs e )
		{
            StatusText.Text = e.Status.ToString();
		}

		private void MainWindow_Loaded( object sender, RoutedEventArgs e )
		{
            if ( !this.sensorController.HasSensor )
            {
                MessageBox.Show( "No Kinect device detected.\nPlease ensure it is plugged in and correctly installed.", "No Kinect detected" );
            }
            else
            {
                // Subscribe to Kinect events here until we can do it in the controller
                this.notifier.Sensors = KinectSensor.KinectSensors;
                KinectSensor.KinectSensors.StatusChanged += new System.EventHandler<StatusChangedEventArgs>( KinectSensors_StatusChanged );

                this.sensorController.StartSensor(); // Blocking.
            }
            this.StatusText.Text = sensorController.SensorStatus;
            this.DepthViewer.initializeViewer( this.sensorController );
		}

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.sensorController.StopSensor();
        }

	}
}
