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
using KinectInfo;
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
		/// The instance of Kinect sensor
		/// </summary>
		private KinectSensor sensor;

		/// <summary>
		/// The Main window View Model
		/// </summary>
		private MainWindowViewModel viewModel;

		/// <summary>
		/// Status Notifier
		/// </summary>
		private StatusNotifier notifier = new StatusNotifier();

		public MainWindow()
		{
			InitializeComponent();
			this.viewModel = new MainWindowViewModel();
			this.viewModel.CanStart = false;
			this.viewModel.CanStop = false;
			this.DataContext = this.viewModel;
		}

		/// <summary>
		/// Starts the sensor.
		/// </summary>
		private void StartSensor()
		{
			if ( this.sensor != null && !this.sensor.IsRunning )
			{
				this.sensor.Start();
				this.viewModel.CanStart = false;
				this.viewModel.CanStop = true;
			}
		}

		/// <summary>
		/// Stops the sensor.
		/// </summary>
		private void StopSensor()
		{
			if ( this.sensor != null && this.sensor.IsRunning )
			{
				this.sensor.Stop();
				this.viewModel.CanStart = true;
				this.viewModel.CanStop = false;
			}
		}

		/// <summary>
		/// Status changed for Kinect sensor
		/// </summary>
		/// <param name="sender">Kinect Sensor</param>
		/// <param name="e">Event Args</param>
		void KinectSensors_StatusChanged( object sender, StatusChangedEventArgs e )
		{
			this.viewModel.SensorStatus = e.Status.ToString();
		}

		/// <summary>
		/// Sets the Kinect info.
		/// </summary>
		private void SetKinectInfo()
		{
			if ( this.sensor != null )
			{
				this.viewModel.ConnectionID = this.sensor.DeviceConnectionId;
				this.viewModel.DeviceID = this.sensor.UniqueKinectId;
				this.viewModel.SensorStatus = this.sensor.Status.ToString();
				this.viewModel.IsColorStreamEnabled = this.sensor.ColorStream.IsEnabled;
				this.viewModel.IsDepthStreamEnabled = this.sensor.DepthStream.IsEnabled;
				this.viewModel.IsSkeletonStreamEnabled = this.sensor.SkeletonStream.IsEnabled;
				this.viewModel.SensorAngle = this.sensor.ElevationAngle;
			}
		}

		/// <summary>
		/// Handles the Click event of the ButtonExit control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
		private void ButtonExit_Click( object sender, RoutedEventArgs e )
		{
			this.StopSensor();
			this.Close();
		}

		private void MainWindow_Loaded( object sender, RoutedEventArgs e )
		{
			int deviceCount = KinectSensor.KinectSensors.Count;
			if (deviceCount > 0)
			{
				this.notifier.Sensors = KinectSensor.KinectSensors;
				this.sensor = KinectSensor.KinectSensors[0];
				KinectSensor.KinectSensors.StatusChanged += new System.EventHandler<StatusChangedEventArgs>( KinectSensors_StatusChanged );
				this.sensor = KinectSensor.KinectSensors[0];
				this.StartSensor();
				this.sensor.ColorStream.Enable();
				this.sensor.DepthStream.Enable();
				this.sensor.SkeletonStream.Enable();

				this.SetKinectInfo();
			}
			else
			{
				// No sensor connected. Take appropriate action
				MessageBox.Show( "No device is connected with system!" );
			}
		}

	}
}
