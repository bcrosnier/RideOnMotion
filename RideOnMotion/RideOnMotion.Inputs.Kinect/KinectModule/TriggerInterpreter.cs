using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RideOnMotion.Inputs.Kinect;

namespace RideOnMotion.Utilities
{
	class TriggerInterpreter
	{
		//array _tab
		public TriggerInterpreter()
		{
			
		}
		public void TriggerRecept( object sender, AreaActivedEventArgs e )
		{
			//CommandTraitment();
		}

		/// <summary>
		/// interpreter of the trigger value
		/// </summary>
		/// <param name="arrayOfTriggerValue">Must contains 4 int(value of roll, pitch, yaw and gaz/elevation</param>
		public void CommandTraitment(int[] arrayOfTriggerValue)
		{
			int roll = arrayOfTriggerValue[0];
			int pitch = arrayOfTriggerValue[1];
			int yaw = arrayOfTriggerValue[2];
			int gaz = arrayOfTriggerValue[3];

			if( roll == 3 ) { roll = 1; } else if( roll == 4 ) { roll = -1; } else { roll = 0; }
			if( pitch == 1 ) { pitch = 1; } else if( pitch == 2 ) { pitch = -1; } else { pitch = 0; }
			if( yaw == 7 ) { yaw = 1; } else if( yaw == 8 ) { yaw = -1; } else { yaw = 0; }
			if( gaz == 5 ) { gaz = 1; } else if( gaz == 6 ) { gaz = -1; } else { gaz = 0; }

			
			//navigate( roll, pitch, yaw, gaz );


		}
	}
}
