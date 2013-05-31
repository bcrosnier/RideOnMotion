using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideOnMotion.Inputs.Kinect
{
	public interface ICaptionArea : INotifyPropertyChanged
    {
        float X { get; }
        float Y { get; }
        float Width { get; }
        float Height { get; }
		String Name { get; }
		int Id { get; }

		IReadOnlyList<Point> Points { get; }
		void CheckPosition( float x, float y );
		IReadOnlyList<Action> AssociateFunctions { get; }
		bool IsActive { get; }
	}
}
