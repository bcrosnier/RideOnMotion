using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideOnMotion.KinectModule
{
	public class Point
	{
		public float X { get; set; }
		public float Y { get; set; }

		public Point( float x, float y )
		{
			if( x < 0 ) throw new ArgumentNullException( "x cannot be negative" );
			if( y < 0 ) throw new ArgumentNullException( "y cannot be negative" );

			X = x;
			Y = y;
		}
	}
}
