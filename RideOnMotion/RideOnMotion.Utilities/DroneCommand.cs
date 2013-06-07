using ARDrone.Capture;
using ARDrone.Control;
using ARDrone.Control.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideOnMotion
{
	//god class !
	public class DroneCommand
	{
		private DroneControl _droneControl;

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

			Logger.Instance.NewEntry( CKLogLevel.Info, CKTraitTags.ARDrone, "Command : ChangeCamera" );

		}

		public void Takeoff()
		{
			Command takeOffCommand = new FlightModeCommand( DroneFlightMode.TakeOff );

			if( !_droneControl.IsCommandPossible( takeOffCommand ) )
				return;

			_droneControl.SendCommand( takeOffCommand );

			Logger.Instance.NewEntry( CKLogLevel.Info, CKTraitTags.ARDrone, "Command : Takeoff" );

		}

		public void Land()
		{
			Command landCommand = new FlightModeCommand( DroneFlightMode.Land );

			if( !_droneControl.IsCommandPossible( landCommand ) )
				return;

			_droneControl.SendCommand( landCommand );

			Logger.Instance.NewEntry( CKLogLevel.Info, CKTraitTags.ARDrone, "Command : Land" );
		}

		public void Emergency()
		{
			Command emergencyCommand = new FlightModeCommand( DroneFlightMode.Emergency );

			if( !_droneControl.IsCommandPossible( emergencyCommand ) )
				return;

			_droneControl.SendCommand( emergencyCommand );

			Logger.Instance.NewEntry( CKLogLevel.Info, CKTraitTags.ARDrone, "Command : Emergency" );
		}

		public void FlatTrim()
		{
			Command resetCommand = new FlightModeCommand( DroneFlightMode.Reset );
			Command flatTrimCommand = new FlatTrimCommand();

			if( !_droneControl.IsCommandPossible( resetCommand ) || !_droneControl.IsCommandPossible( flatTrimCommand ) )
				return;

			_droneControl.SendCommand( resetCommand );
			_droneControl.SendCommand( flatTrimCommand );

			Logger.Instance.NewEntry( CKLogLevel.Info, CKTraitTags.ARDrone, "Command : FlatTrim" );
		}

		public void EnterHoverMode()
		{
			Command enterHoverModeCommand = new HoverModeCommand( DroneHoverMode.Hover );

			if( !_droneControl.IsCommandPossible( enterHoverModeCommand ) )
				return;

			_droneControl.SendCommand( enterHoverModeCommand );

			Logger.Instance.NewEntry( CKLogLevel.Info, CKTraitTags.ARDrone, "Command : EnterHoverMode" );
		}

		public void LeaveHoverMode()
		{
			Command leaveHoverModeCommand = new HoverModeCommand( DroneHoverMode.StopHovering );

			if( !_droneControl.IsCommandPossible( leaveHoverModeCommand ) )
				return;

			_droneControl.SendCommand( leaveHoverModeCommand );

			Logger.Instance.NewEntry( CKLogLevel.Info, CKTraitTags.ARDrone, "Command : LeaveHoverMode" );
		}

		public void Navigate( float roll, float pitch, float yaw, float gaz )
		{
			FlightMoveCommand flightMoveCommand = new FlightMoveCommand( roll, pitch, yaw, gaz );

			if( _droneControl.IsCommandPossible( flightMoveCommand ) )
			{
				_droneControl.SendCommand( flightMoveCommand );

				Logger.Instance.NewEntry( CKLogLevel.Info, CKTraitTags.ARDrone, "Navigate with : " + roll + " " + pitch + " " + yaw + " " + gaz );
			}
		}

		public void PlayLED()
		{
			PlayLedAnimationCommand PlayLedCommand = new PlayLedAnimationCommand( LedAnimation.GreenBlink, 10, 3 );

			if( !_droneControl.IsCommandPossible( PlayLedCommand ) )
				return;

			_droneControl.SendCommand( PlayLedCommand );

			Logger.Instance.NewEntry( CKLogLevel.Info, CKTraitTags.ARDrone, "Command : PlayLED" );
		}
	}
}
