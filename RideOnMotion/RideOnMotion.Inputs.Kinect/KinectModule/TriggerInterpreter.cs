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
			PositionTrackerController PosTracker = new PositionTrackerController();
			PosTracker.AreaActived += TriggerRecept;
		}
		public void TriggerRecept( object sender, AreaActivedEventArgs e )
		{
			AreaActivedEventArgs ReceptedEvent = e;
			
		}
	}
}
