using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideOnMotion.Inputs
{
	public static class MixInputs
	{
		public static InputState MixInput( InputState Keyboard, InputState Gamepad, InputState Kinect )
		{
			if ( Keyboard == null && Gamepad == null && Kinect == null )
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
				if ( Kinect == null )
				{
					Kinect = new InputState();
				}
				InputState MixedInput = new InputState();
				if ( Keyboard.CameraSwap || Gamepad.CameraSwap || Kinect.CameraSwap )
					MixedInput.CameraSwap = true;
				if ( Keyboard.Emergency || Gamepad.Emergency || Kinect.Emergency )
					MixedInput.Emergency = true;
				if ( Keyboard.FlatTrim || Gamepad.FlatTrim || Kinect.FlatTrim )
					MixedInput.FlatTrim = true;
				if ( Keyboard.Hover || Gamepad.Hover || Kinect.Hover )
					MixedInput.Hover = true;
				if ( Keyboard.Land || Gamepad.Land || Kinect.Land )
					MixedInput.Land = true;
				if ( Keyboard.SpecialAction || Gamepad.SpecialAction || Kinect.SpecialAction )
					MixedInput.SpecialAction = true;
				if ( Keyboard.TakeOff || Gamepad.TakeOff || Kinect.TakeOff )
					MixedInput.TakeOff = true;

				if ( Gamepad.Gaz != 0 && ( Keyboard.Gaz != 0 || Kinect.Gaz != 0 ) )
				{
					MixedInput.Gaz = Gamepad.Gaz;
				}
				else if ( Keyboard.Gaz != 0 && Kinect.Gaz != 0 )
				{
					MixedInput.Gaz = Keyboard.Gaz;
				}
				else
				{
					MixedInput.Gaz += Kinect.Gaz;
					MixedInput.Gaz += Keyboard.Gaz;
					MixedInput.Gaz += Gamepad.Gaz;
				}

				if ( Gamepad.Pitch != 0 && ( Keyboard.Pitch != 0 || Kinect.Pitch != 0 ) )
				{
					MixedInput.Pitch = Gamepad.Pitch;
				}
				else if ( Keyboard.Pitch != 0 && Kinect.Pitch != 0 )
				{
					MixedInput.Pitch = Keyboard.Pitch;
				}
				else
				{
					MixedInput.Pitch += Kinect.Pitch;
					MixedInput.Pitch += Keyboard.Pitch;
					MixedInput.Pitch += Gamepad.Pitch;
				}

				if ( Gamepad.Roll != 0 && ( Keyboard.Roll != 0 || Kinect.Roll != 0 ) )
				{
					MixedInput.Roll = Gamepad.Roll;
				}
				else if ( Keyboard.Roll != 0 && Kinect.Roll != 0 )
				{
					MixedInput.Roll = Keyboard.Roll;
				}
				else
				{
					MixedInput.Roll += Kinect.Roll;
					MixedInput.Roll += Keyboard.Roll;
					MixedInput.Roll += Gamepad.Roll;
				}

				if ( Gamepad.Yaw != 0 && ( Keyboard.Yaw != 0 || Kinect.Yaw != 0 ) )
				{
					MixedInput.Yaw = Gamepad.Yaw;
				}
				else if ( Keyboard.Yaw != 0 && Kinect.Yaw != 0 )
				{
					MixedInput.Yaw = Keyboard.Yaw;
				}
				else
				{
					MixedInput.Yaw += Kinect.Yaw;
					MixedInput.Yaw += Keyboard.Yaw;
					MixedInput.Yaw += Gamepad.Yaw;
				}
				return MixedInput;
			}
		}
	}
}
