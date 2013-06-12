using ARDrone.Capture;
using ARDrone.Control;
using ARDrone.Control.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RideOnMotion.Utilities;

namespace RideOnMotion
{
	//god class !
	public class DroneCommand
	{
		private DroneControl _droneControl;
		bool _isDronePaired = false;

		public bool CanTakeoff
		{
			get
			{
				return _droneControl.CanTakeoff;
			}
		}
		public bool CanLand
		{
			get
			{
				return _droneControl.CanLand;
			}
		}
		public bool CanEnterHoverMode
		{
			get
			{
				return _droneControl.CanEnterHoverMode;
			}
		}
		public bool CanLeaveHoverMode
		{
			get
			{
				return _droneControl.CanLeaveHoverMode;
			}
		}

		public bool IsDronePaired
        {
            get
            {
                return _isDronePaired;
            }
			set
			{
                _isDronePaired = value;
			}
		}

		public DroneCommand( DroneControl droneControl )
		{
			_droneControl = droneControl;
		}

		public void Connect()
		{
			_droneControl.ConnectToDrone();

			Logger.Instance.NewEntry( CKLogLevel.Info, CKTraitTags.ARDrone, "Command : Connect" );
		}

		public void Disconnect()
		{
			_droneControl.Disconnect();

			Logger.Instance.NewEntry( CKLogLevel.Info, CKTraitTags.ARDrone, "Command : Disconnect" );
		}

		public void ChangeCamera()
		{
			Command switchCameraCommand = new SwitchCameraCommand( DroneCameraMode.NextMode );

			if( !_droneControl.IsCommandPossible( switchCameraCommand ) )
				return;

			_droneControl.SendCommand( switchCameraCommand );

		}

		public void Takeoff()
		{
			Command takeOffCommand = new FlightModeCommand( DroneFlightMode.TakeOff );

			if( !_droneControl.IsCommandPossible( takeOffCommand ) )
				return;

			_droneControl.SendCommand( takeOffCommand );

		}

		public void Land()
		{
			Command landCommand = new FlightModeCommand( DroneFlightMode.Land );

			if( !_droneControl.IsCommandPossible( landCommand ) )
				return;

			_droneControl.SendCommand( landCommand );
		}

		public void Emergency()
		{
			Command emergencyCommand = new FlightModeCommand( DroneFlightMode.Emergency );

			if( !_droneControl.IsCommandPossible( emergencyCommand ) )
				return;

			_droneControl.SendCommand( emergencyCommand );
		}

		public void FlatTrim()
		{
			Command resetCommand = new FlightModeCommand( DroneFlightMode.Reset );
			Command flatTrimCommand = new FlatTrimCommand();

			if( !_droneControl.IsCommandPossible( resetCommand ) || !_droneControl.IsCommandPossible( flatTrimCommand ) )
				return;

			_droneControl.SendCommand( resetCommand );
			_droneControl.SendCommand( flatTrimCommand );
		}

		public void EnterHoverMode()
		{
			Command enterHoverModeCommand = new HoverModeCommand( DroneHoverMode.Hover );

			if( !_droneControl.IsCommandPossible( enterHoverModeCommand ) )
				return;

			_droneControl.SendCommand( enterHoverModeCommand );
		}

		public void LeaveHoverMode()
		{
			Command leaveHoverModeCommand = new HoverModeCommand( DroneHoverMode.StopHovering );

			if( !_droneControl.IsCommandPossible( leaveHoverModeCommand ) )
				return;

			_droneControl.SendCommand( leaveHoverModeCommand );
		}

		public void Navigate( float roll, float pitch, float yaw, float gaz )
		{
			FlightMoveCommand flightMoveCommand = new FlightMoveCommand( roll, pitch, yaw, gaz );

			if( _droneControl.IsCommandPossible( flightMoveCommand ) )
			{
				_droneControl.SendCommand( flightMoveCommand );
			}
		}

		public void PlayLED()
		{
			PlayLedAnimationCommand PlayLedCommand = new PlayLedAnimationCommand( LedAnimation.GreenBlink, 10, 3 );

			if( !_droneControl.IsCommandPossible( PlayLedCommand ) )
				return;

			_droneControl.SendCommand( PlayLedCommand );
		}

		public void Pair( )
		{
			String mac = MacAddress.GetWifiMacAddress();
			if ( mac == null )
			{
				return;
			}

			PairingCommand pairingCommand = new PairingCommand( DronePairingMode.Pair, mac);

			if ( !_droneControl.IsCommandPossible( pairingCommand ) )
				return;

			_droneControl.SendCommand( pairingCommand );
			_isDronePaired = true;
		}
		public void Unpair()
		{
			PairingCommand pairingCommand = new PairingCommand( DronePairingMode.Unpair, "00:00:00:00:00:00" );

			if ( !_droneControl.IsCommandPossible( pairingCommand ) )
				return;

			_droneControl.SendCommand( pairingCommand );
			_isDronePaired = false;
		}
	}
}
