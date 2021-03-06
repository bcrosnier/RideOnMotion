﻿using ARDrone.Capture;
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
		public DroneControl DroneControl { get { return _droneControl; } }
		public int FrameRate { get { return GetCurrentFrameRate(); } }

        public event EventHandler<String> NetworkConnectionStateChanged;
        public event EventHandler<bool> ConnectionStateChanged;

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
		public event EventHandler<DroneDataReadyEventArgs> DroneDataReady;

        public DroneInitializer( DroneConfig config )
        {
            _currentDroneConfig = config;
            _droneControl = new DroneControl( _currentDroneConfig );
            _droneCommand = new DroneCommand( _droneControl );

            _droneControl.Error += droneControl_Error;
            _droneControl.ConnectionStateChanged += droneControl_ConnectionStateChanged;
            _droneControl.NetworkConnectionStateChanged += droneControl_NetworkConnectionStateChanged;

            InitializeVideoUpdate();
            InitializeHudInterface();

            _timerStatusUpdate = new DispatcherTimer();
			_timerStatusUpdate.Interval = new TimeSpan( 0, 0, 0, 0, 30 );
            _timerStatusUpdate.Tick += new EventHandler( timerStatusUpdate_Tick );
        }

        void droneControl_NetworkConnectionStateChanged(object sender, DroneNetworkConnectionStateChangedEventArgs e)
        {
            if ( this.NetworkConnectionStateChanged != null )
            {
                this.NetworkConnectionStateChanged( sender, "(" + e.CurrentInterfaceName + ")" + e.State.ToString() );
            }
        }

		private void InitializeVideoUpdate()
		{
			_timerVideoUpdate = new DispatcherTimer();
			_timerVideoUpdate.Interval = new TimeSpan( 0, 0, 0, 0, 40 );
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
			Task.Factory.StartNew( () =>
			{
				_droneCommand.Connect();
				_timerVideoUpdate.Start();
				_timerStatusUpdate.Start();
			} );
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
				Logger.Instance.NewEntry( CKLogLevel.Info, CKTraitTags.ARDrone, "ARDrone is connected" );
			}
			else
			{
				Logger.Instance.NewEntry( CKLogLevel.Info, CKTraitTags.ARDrone, "ARDrone is disconnected" );
            }

            if ( this.ConnectionStateChanged != null )
            {
                ConnectionStateChanged( sender, e.Connected );
            }
		}

		private void droneControl_Error( object sender, DroneErrorEventArgs e )
		{
			Logger.Instance.NewEntry( CKLogLevel.Error, CKTraitTags.ARDrone, "DroneControl : "+ e.CausingException.Message );
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
				else
				{
					if( DroneFrameReady != null )
						DroneFrameReady( this, new DroneFrameReadyEventArgs( imageSource ) );
				}
				UpdateHudStatus();
			}
		}

		private void timerStatusUpdate_Tick( object sender, EventArgs e )
		{
			if( _droneControl.IsConnected )
			{
				if( DroneDataReady != null )
					DroneDataReady( this, new DroneDataReadyEventArgs( _droneControl.NavigationData ) );
			}
		}

		private void UpdateHudStatus()
		{
			if( _droneControl.IsConnected )
			{
				DroneData data = _droneControl.NavigationData;

				_hudInterface.SetFlightVariables( data.Phi, data.Theta, data.Psi );
				_hudInterface.SetAltitude( data.Altitude );
				_hudInterface.SetOverallSpeed( data.VX, data.VY, data.VZ );
				_hudInterface.SetBatteryLevel( data.BatteryLevel );
			}
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
	public class DroneDataReadyEventArgs : EventArgs
	{
		private DroneData _data;
		public DroneData Data { get { return _data; } }

		public DroneDataReadyEventArgs( DroneData data )
		{
			_data = data;
		}
	}
}
