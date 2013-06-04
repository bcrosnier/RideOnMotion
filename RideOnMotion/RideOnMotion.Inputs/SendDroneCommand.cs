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
			if ( inputState.TakeOff && _drone.CanTakeoff )
				_drone.Takeoff();
			else if ( inputState.Land && _drone.CanLand )
				_drone.Land();

			if ( inputState.Hover && _drone.CanEnterHoverMode )
				_drone.EnterHoverMode();
			else if ( inputState.Hover && _drone.CanLeaveHoverMode )
				_drone.LeaveHoverMode();

			if ( inputState.Emergency )
				_drone.Emergency();
			else if ( inputState.FlatTrim )
				_drone.FlatTrim();

			float roll = inputState.Roll / 1.0f;
			float pitch = inputState.Pitch / 1.0f;
			float yaw = inputState.Yaw / 2.0f;
			float gaz = inputState.Gaz / 2.0f;
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
