using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideOnMotion.KinectModule.Tracker
{
	interface IPositionTracker
	{
		public int positionX { get; set; }
		public int positionY { get; set; }
		public int positionZ { get; set; }
	}
}
