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
		/// <param name="arrayOfTriggerValue">Must contains 8 bool value of (pitch[0-1], roll[2-3], gaz[4-5], yaw[6-7]) triggers</param>
		public void CommandTraitment( bool[] arrayOfTriggerValue )
		{

			int roll; // = arrayOfTriggerValue[2-3];
			int pitch; // = arrayOfTriggerValue[0-1];
			int yaw; // = arrayOfTriggerValue[6-7];
			int gaz; // = arrayOfTriggerValue[4-5];

			if( arrayOfTriggerValue[2] ) { roll = -1; }	else if( arrayOfTriggerValue[3] ) { roll = 1; } else { roll = 0; }
			if( arrayOfTriggerValue[0] ) { pitch = 1; } else if( arrayOfTriggerValue[1] ) { pitch = -1; } else { pitch = 0; }
			if( arrayOfTriggerValue[6] ) { yaw = -1; } else if( arrayOfTriggerValue[7] ) { yaw = 1; } else { yaw = 0; }
			if( arrayOfTriggerValue[4] ) { gaz = 1; } else if( arrayOfTriggerValue[5] ) { gaz = -1; } else { gaz = 0; }

			//navigate( roll, pitch, yaw, gaz );

		}
	}
}
