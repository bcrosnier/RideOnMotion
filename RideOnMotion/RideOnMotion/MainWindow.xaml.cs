using System.Windows;
using Microsoft.Kinect;
using KinectStatusNotifier;

namespace RideOnMotion
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		/// <summary>
		/// Active Kinect sensor.
		/// </summary>
		private KinectSensor sensor;

		/// <summary>
		/// Kinect Status Notifier. Notably used by the Kinect system tray icon.
		/// </summary>
		private StatusNotifier notifier = new StatusNotifier();

		public MainWindow()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Starts the sensor.
		/// </summary>
        [System.Obsolete("Use Franck's KinectSensorController when possible.")]
		private void StartSensor()
		{
			if ( this.sensor != null && !this.sensor.IsRunning )
			{
				this.sensor.Start();
			}
		}

		/// <summary>
		/// Stops the sensor.
        /// </summary>
        [System.Obsolete("Use Franck's KinectSensorController when possible.")]
		private void StopSensor()
		{
			if ( this.sensor != null && this.sensor.IsRunning )
			{
				this.sensor.Stop();
			}
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
            // Start first detected sensor
			int deviceCount = KinectSensor.KinectSensors.Count; // Blocking.
			if (deviceCount > 0)
			{
				this.notifier.Sensors = KinectSensor.KinectSensors;
				KinectSensor.KinectSensors.StatusChanged += new System.EventHandler<StatusChangedEventArgs>( KinectSensors_StatusChanged );
				
                this.sensor = KinectSensor.KinectSensors[0];

				this.StartSensor(); // Blocking.

                // We don't use these for now, but we will. -- BC
                /*
				this.sensor.ColorStream.Enable();
				this.sensor.DepthStream.Enable();
				this.sensor.SkeletonStream.Enable();
                */
			}
			else
			{
				// No sensor detected. Take appropriate action
				MessageBox.Show( "No Kinect device detected. Please ensure it is plugged in and correctly installed." );
			}
		}

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.StopSensor();
        }

	}
}
