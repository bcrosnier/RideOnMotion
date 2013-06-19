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
		public bool RelativeDirection;

        public SendDroneCommand(IActivityLogger parentLogger)
        {
            _logger = new DefaultActivityLogger();
            _logger.AutoTags = ActivityLogger.RegisteredTags.FindOrCreate( "SendDroneCommand" );
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
		public void Process( RideOnMotion.Inputs.InputState inputState )
		{
			if ( inputState.Land && _drone.CanLand )
				_drone.Land();
			else if ( inputState.TakeOff && _drone.CanTakeoff )
				_drone.Takeoff();

			if ( inputState.Hover && _drone.CanEnterHoverMode )
				_drone.EnterHoverMode();
			else if ( inputState.Hover && _drone.CanLeaveHoverMode )
				_drone.LeaveHoverMode();

			if ( inputState.CameraSwap )
				_drone.ChangeCamera();

			if ( inputState.Emergency )
				_drone.Emergency();
			else if ( inputState.FlatTrim )
				_drone.FlatTrim();

			if ( inputState.SpecialAction )
				_drone.PlayLED();

			float roll = inputState.Roll / 3.0f;
            inputState.Roll = roll;
			float pitch = inputState.Pitch / 3.0f;
            inputState.Pitch = pitch;
			float yaw = inputState.Yaw / 2.0f;
            inputState.Yaw = yaw;
			float gaz = inputState.Gaz / 3.0f;
            inputState.Gaz = gaz;
			RelativeDirection = true;
			float roll2;
			float pitch2;
			if ( RelativeDirection )
			{
				roll2 = roll;
				pitch2 = pitch;
			}
			else
			{
				roll2 = (( (float)Math.Cos( ( Math.PI / 180 ) * DroneOrientationDifference ) ) * roll ) 
					+ (( (float)Math.Sin( ( Math.PI / 180 ) * DroneOrientationDifference ) ) * -pitch );
				pitch2 = ( ( (float)Math.Cos( ( Math.PI / 180 ) * DroneOrientationDifference ) ) * pitch ) 
					+ ( ( (float)Math.Sin( ( Math.PI / 180 ) * DroneOrientationDifference ) ) * roll );

				inputState.Roll = roll2;
				inputState.Pitch = pitch2;
				// for debugging purpose
                //_logger.Trace("Navigate with : roll2 : " + roll2 + " , pitch2 : " + pitch2);
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
