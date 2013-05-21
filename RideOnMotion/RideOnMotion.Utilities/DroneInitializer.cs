using ARDrone.Capture;
using ARDrone.Control;
using ARDrone.Control.Commands;
using ARDrone.Control.Data;
using ARDrone.Control.Events;
using ARDrone.Hud;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace RideOnMotion
{
	public class DroneInitializer
	{
		private DispatcherTimer _timerVideoUpdate;
		private DispatcherTimer _timerStatusUpdate;

		private HudConfig _currentHudConfig;
		private HudInterface _hudInterface;

		private DroneConfig _currentDroneConfig;
		private DroneControl _droneControl;

		private DroneCommand _droneCommand;
		private int _frameCountSinceLastCapture = 0;
		private DateTime _lastFrameRateCaptureTime;
		private int _averageFrameRate = 0;

		public DroneCommand DroneCommand { get { return _droneCommand; } }
		public DroneConfig DroneConfig { get { return _currentDroneConfig; } }
		public int FrameRate { get { return GetCurrentFrameRate(); } }

		private int GetCurrentFrameRate()
		{
			int timePassed = (int)( DateTime.Now - _lastFrameRateCaptureTime ).TotalMilliseconds;
			int frameRate = _frameCountSinceLastCapture * 1000 / timePassed;
			_averageFrameRate = ( _averageFrameRate + frameRate ) / 2;

			_lastFrameRateCaptureTime = DateTime.Now;
			_frameCountSinceLastCapture = 0;

			return _averageFrameRate;
		}

		public event EventHandler<DroneFrameReadyEventArgs> DroneFrameReady;

		public DroneInitializer()
			: this("192.168.1.2", "192.168.1.1", "ardrone_006431")
		{
		}

		public DroneInitializer( string ownIPAddress, string droneIPAddress, string droneNetworkIdentifier, int videoPort = 5555, int navigationPort = 5554, int commandPort = 5556, int controlInfoPort = 5559, bool useSpecificFirmwareVersion = false, SupportedFirmwareVersion firmwareVersion = DroneConfig.DefaultSupportedFirmwareVersion, int timeoutValue = 500 )
		{
			InitializeDroneControl( ownIPAddress, droneIPAddress, droneNetworkIdentifier, videoPort, navigationPort, commandPort, controlInfoPort, useSpecificFirmwareVersion, firmwareVersion, timeoutValue );

			InitializeVideoUpdate();

			InitializeHudInterface();

			_droneCommand = new DroneCommand( _droneControl );

		}

		private void InitializeDroneControl( string ownIPAddress, string droneIPAddress, string droneNetworkIdentifier, int videoPort, int navigationPort, int commandPort, int controlInfoPort, bool useSpecificFirmwareVersion, SupportedFirmwareVersion firmwareVersion, int timeoutValue )
		{
			_currentDroneConfig = new DroneConfig();

			_currentDroneConfig.StandardOwnIpAddress = ownIPAddress;
			_currentDroneConfig.DroneIpAddress = droneIPAddress;
			_currentDroneConfig.DroneNetworkIdentifierStart = droneNetworkIdentifier;

			_currentDroneConfig.VideoPort = videoPort;
			_currentDroneConfig.NavigationPort = navigationPort;
			_currentDroneConfig.CommandPort = commandPort;
			_currentDroneConfig.ControlInfoPort = controlInfoPort;

			_currentDroneConfig.UseSpecificFirmwareVersion = useSpecificFirmwareVersion;
			_currentDroneConfig.FirmwareVersion = firmwareVersion;

			_currentDroneConfig.TimeoutValue = timeoutValue;

			_droneControl = new DroneControl( _currentDroneConfig );

			_droneControl.Error += droneControl_Error;
			_droneControl.ConnectionStateChanged += droneControl_ConnectionStateChanged;

			_timerStatusUpdate = new DispatcherTimer();
			_timerStatusUpdate.Interval = new TimeSpan( 0, 0, 1 );
			_timerStatusUpdate.Tick += new EventHandler( timerStatusUpdate_Tick );
		}

		private void InitializeVideoUpdate()
		{
			_timerVideoUpdate = new DispatcherTimer();
			_timerVideoUpdate.Interval = new TimeSpan( 0, 0, 0, 0, 50 );
			_timerVideoUpdate.Tick += new EventHandler( timerVideoUpdate_Tick );

			_lastFrameRateCaptureTime = DateTime.Now;
		}

		private void InitializeHudInterface()
		{
			_currentHudConfig = new HudConfig();

			HudConstants hudConstants = new HudConstants( _droneControl.FrontCameraFieldOfViewDegrees );

			_hudInterface = new HudInterface( _currentHudConfig, hudConstants );
		}

		public void StartDrone()
		{
			_droneCommand.Connect();
			_timerVideoUpdate.Start();
			_timerStatusUpdate.Start();
		}

		public void EndDrone()
		{
			_droneCommand.Disconnect();
			_timerVideoUpdate.Stop();
			_timerStatusUpdate.Stop();
		}

		//EVENT HANDLER
		private void droneControl_ConnectionStateChanged( object sender, DroneConnectionStateChangedEventArgs e )
		{
			if( e.Connected )
			{
				Logger.Instance.NewEntry( CK.Core.LogLevel.Info, CKTraitTags.ARDrone, "ARDrone is connected" );
			}
			else
			{
				Logger.Instance.NewEntry( CK.Core.LogLevel.Info, CKTraitTags.ARDrone, "ARDrone is disconnected" );
			}
		}

		private void droneControl_Error( object sender, DroneErrorEventArgs e )
		{
			Logger.Instance.NewEntry( CK.Core.LogLevel.Error, CKTraitTags.ARDrone, e.CausingException.Message );
		}

		private void timerVideoUpdate_Tick( object sender, EventArgs e )
		{
			if( _droneControl.IsConnected )
			{
				ImageSource imageSource = _droneControl.ImageSourceImage;

				if( imageSource != null &&
					( _droneControl.CurrentCameraType == DroneCameraMode.FrontCamera ||
					 _droneControl.CurrentCameraType == DroneCameraMode.PictureInPictureFront ) )
				{
					_frameCountSinceLastCapture++;

					ImageSource resultingSource = _hudInterface.DrawHud( (BitmapSource)imageSource );
					if( DroneFrameReady != null )
						DroneFrameReady( this, new DroneFrameReadyEventArgs( resultingSource ) );
				}
			}
		}

		private void timerStatusUpdate_Tick( object sender, EventArgs e )
		{
		}
	}

	public class DroneFrameReadyEventArgs : EventArgs
	{
		private ImageSource _frame;
		public ImageSource Frame { get { return _frame; } }

		public DroneFrameReadyEventArgs( ImageSource frame )
		{
			_frame = frame;
		}
	}
}
