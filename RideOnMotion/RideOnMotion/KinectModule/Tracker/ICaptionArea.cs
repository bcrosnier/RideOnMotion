using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideOnMotion.KinectModule
{
	public interface ICaptionArea : INotifyPropertyChanged
	{
		IReadOnlyList<Point> Points { get; }
		void CheckPosition( Joint joint );
		IReadOnlyList<Action> AssociateFunctions { get; }
		bool IsActive { get; }
	}
}
