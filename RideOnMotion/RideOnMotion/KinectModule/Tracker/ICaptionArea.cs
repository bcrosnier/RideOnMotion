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
        float X { get; }
        float Y { get; }
        float Width { get; }
        float Height { get; }

		IReadOnlyList<Point> Points { get; }
		void CheckPosition( Joint joint );
		IReadOnlyList<Action> AssociateFunctions { get; }
		bool IsActive { get; }
	}
}
