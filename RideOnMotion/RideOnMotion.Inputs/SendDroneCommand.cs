using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace RideOnMotion.Inputs
{
	public class SendDroneCommand
	{
        IActivityLogger _logger;
		DroneCommand _drone;
		public double DroneOriginalOrientation;
		public double DroneCurrentOrientation;
		public bool AbsoluteControlMode;
		DroneSpeeds _droneSpeeds;

        public SendDroneCommand(IActivityLogger parentLogger)
        {
            _logger = new DefaultActivityLogger();
            _logger.AutoTags = ActivityLogger.RegisteredTags.FindOrCreate( "Application" );
            _logger.Output.BridgeTo( parentLogger );
        }

		internal double DroneOrientationDifference
		{
			get
			{
				return DroneOriginalOrientation - DroneCurrentOrientation;
			}
		}

		public DroneCommand ActiveDrone
		{
			set
			{
				this._drone = value;
			}
		}

		public DroneSpeeds DroneSpeeds
		{
			get
			{
				return _droneSpeeds;
			}
			set
			{
				_droneSpeeds = value;
			}
		}
		public void Process( RideOnMotion.Inputs.InputState inputState )
		{
			if ( inputState.Land && _drone.CanLand )
			{
				_drone.Land();
				_logger.Info( "Drone is landing" );
			}
			else if ( inputState.TakeOff && _drone.CanTakeoff )
			{
				_drone.Takeoff();
				_logger.Info( "Drone is taking off" );
			}

			if ( inputState.Hover && _drone.CanEnterHoverMode )
			{
				_drone.EnterHoverMode();
				_logger.Info( "Drone is Hovering" );
			}
			else if ( inputState.Hover && _drone.CanLeaveHoverMode )
			{
				_drone.LeaveHoverMode();
				_logger.Info( "Drone is not Hovering anymore" );
			}

			if ( inputState.CameraSwap )
			{
				_drone.ChangeCamera();
				_logger.Info( "Drone chanched what he can see" );
			}

			if ( inputState.Emergency )
			{
				_drone.Emergency();
				_logger.Info( "Drone took his time to crash while you read that" );
			}
			else if ( inputState.FlatTrim )
			{
				_drone.FlatTrim();
				_logger.Info( "Flat trim done" );
			}

			if ( inputState.SpecialAction )
			{
				_drone.PlayLED();
				_logger.Info( "I can see the lights, it burns *.*" );
			}

			float roll = inputState.Roll / ( 1 / _droneSpeeds.DroneTranslationSpeed );
            inputState.Roll = roll;
			float pitch = inputState.Pitch / ( 1 / _droneSpeeds.DroneTranslationSpeed );
            inputState.Pitch = pitch;
			float yaw = inputState.Yaw / ( 1 / _droneSpeeds.DroneRotationSpeed );
            inputState.Yaw = yaw;
			float gaz = inputState.Gaz / ( 1 / _droneSpeeds.DroneElevationSpeed );
            inputState.Gaz = gaz;
			AbsoluteControlMode = true;
			float roll2;
			float pitch2;
			if ( AbsoluteControlMode )
			{
				roll2 = (( (float)Math.Cos( ( Math.PI / 180 ) * DroneOrientationDifference ) ) * roll ) 
					+ (( (float)Math.Sin( ( Math.PI / 180 ) * DroneOrientationDifference ) ) * -pitch );
				pitch2 = ( ( (float)Math.Cos( ( Math.PI / 180 ) * DroneOrientationDifference ) ) * pitch ) 
					+ ( ( (float)Math.Sin( ( Math.PI / 180 ) * DroneOrientationDifference ) ) * roll );

				inputState.Roll = roll2;
				inputState.Pitch = pitch2;
			}
			else
			{
				roll2 = roll;
				pitch2 = pitch;
			}
			_drone.Navigate( roll2, pitch2, yaw, gaz );
            _logger.Trace( inputState.ToString() );
		}

		public void MixInputAndProcess(InputState Keyboard,InputState Gamepad, InputState Kinect)
		{
			if ( Keyboard.Equals( Gamepad ) )
			{
				Process( Gamepad );
			}
			else
			{
				InputState MixedInput = MixInputs.MixInput( Keyboard, Gamepad, Kinect );
				if ( MixedInput != null )
				{
					Process( MixedInput );
				}

			}
		}

	}
}
