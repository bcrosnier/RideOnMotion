using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideOnMotion.KinectModule
{
	public interface ICaptionArea
	{
		IReadOnlyList<Point> Points { get; }
		void CheckPosition( Joint joint );
		IReadOnlyList<Action> AssociateFunctions { get; }
	}
}
