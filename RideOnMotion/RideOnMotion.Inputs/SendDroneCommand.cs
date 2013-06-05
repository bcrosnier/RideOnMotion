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

		public void process( RideOnMotion.Inputs.InputState inputState )
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
				_drone.Navigate( roll, pitch, yaw, gaz );
		}


		public DroneCommand ActiveDrone
		{
			set
			{
				this._drone = value;
			}
		}
	}
}
