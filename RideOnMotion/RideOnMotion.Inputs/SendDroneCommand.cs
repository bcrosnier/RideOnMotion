using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideOnMotion.Inputs
{
	public class SendDroneCommand
	{
		DroneCommand _drone;
		public double DroneOriginalOrientation;
		public double DroneCurrentOrientation;
		public bool RelativeDirection;

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
			float pitch = inputState.Pitch / 3.0f;
			float yaw = inputState.Yaw / 2.0f;
			float gaz = inputState.Gaz / 3.0f;
			RelativeDirection = false;
			if ( RelativeDirection )
			{
				_drone.Navigate( roll, pitch, yaw, gaz );
			}
			else
			{
				float roll2 = (( (float)Math.Cos( ( Math.PI / 180 ) * DroneOrientationDifference ) ) * roll ) 
					+ (( (float)Math.Sin( ( Math.PI / 180 ) * DroneOrientationDifference ) ) * -pitch );
				float pitch2 = ( ( (float)Math.Cos( ( Math.PI / 180 ) * DroneOrientationDifference ) ) * pitch ) 
					+ ( ( (float)Math.Sin( ( Math.PI / 180 ) * DroneOrientationDifference ) ) * roll ); ;
				// for debugging purpose
				//Logger.Instance.NewEntry( CKLogLevel.Info, CKTraitTags.ARDrone, "Navigate with : roll2 : " + roll2 + " , pitch2 : " + pitch2);
				_drone.Navigate( roll2, pitch2, yaw, gaz );
			}
			Logger.Instance.NewEntry( CKLogLevel.Info, CKTraitTags.ARDrone, inputState.ToString());
		}

		public void MixInputAndProcess(InputState Keyboard,InputState Gamepad)
		{
			if ( Keyboard.Equals( Gamepad ) )
			{
				Process( Gamepad );
			}
			else
			{
				InputState MixedInput = MixInput( Keyboard, Gamepad );
				if ( MixedInput != null )
				{
					Process( MixedInput );
				}

			}
		}

		public static InputState MixInput( InputState Keyboard, InputState Gamepad )
		{
			if ( Keyboard == null && Gamepad == null )
			{
				return null;
			}
			else
			{
				if ( Keyboard == null )
				{
					Keyboard = new InputState();
				}
				if ( Gamepad == null )
				{
					Gamepad = new InputState();
				}
				InputState MixedInput = new InputState();
				if ( Keyboard.CameraSwap || Gamepad.CameraSwap )
					MixedInput.CameraSwap = true;
				if ( Keyboard.Emergency || Gamepad.Emergency )
					MixedInput.Emergency = true;
				if ( Keyboard.FlatTrim || Gamepad.FlatTrim )
					MixedInput.FlatTrim = true;
				if ( Keyboard.Hover || Gamepad.Hover )
					MixedInput.Hover = true;
				if ( Keyboard.Land || Gamepad.Land )
					MixedInput.Land = true;
				if ( Keyboard.SpecialAction || Gamepad.SpecialAction )
					MixedInput.SpecialAction = true;
				if ( Keyboard.TakeOff || Gamepad.TakeOff )
					MixedInput.TakeOff = true;

				if ( Keyboard.Gaz != 0 && Gamepad.Gaz != 0 )
				{
					MixedInput.Gaz = Gamepad.Gaz;
				}
				else
				{
					MixedInput.Gaz += Keyboard.Gaz;
					MixedInput.Gaz += Gamepad.Gaz;
				}

				if ( Keyboard.Pitch != 0 && Gamepad.Pitch != 0 )
				{
					MixedInput.Pitch = Gamepad.Pitch;
				}
				else
				{
					MixedInput.Pitch += Keyboard.Pitch;
					MixedInput.Pitch += Gamepad.Pitch;
				}

				if ( Keyboard.Roll != 0 && Gamepad.Roll != 0 )
				{
					MixedInput.Roll = Gamepad.Roll;
				}
				else
				{
					MixedInput.Roll += Keyboard.Roll;
					MixedInput.Roll += Gamepad.Roll;
				}

				if ( Keyboard.Yaw != 0 && Gamepad.Yaw != 0 )
				{
					MixedInput.Yaw = Gamepad.Yaw;
				}
				else
				{
					MixedInput.Yaw += Keyboard.Yaw;
					MixedInput.Yaw += Gamepad.Yaw;
				}
				return MixedInput;
			}
		}
	}
}
