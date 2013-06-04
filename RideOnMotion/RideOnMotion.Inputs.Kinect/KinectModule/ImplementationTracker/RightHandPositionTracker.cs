using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using Microsoft.Kinect.Toolkit.Interaction;

namespace RideOnMotion.Inputs.Kinect
{
	public class RightHandPositionTracker : IPositionTracker<UserInfo>
	{
		readonly IList<ICaptionArea> _captionAreas;

		public IReadOnlyList<ICaptionArea> CaptionAreas
		{
			get { return _captionAreas.AsReadOnlyList(); }
		}

		public RightHandPositionTracker()
			: this( new List<ICaptionArea>() )
		{
		}

		public RightHandPositionTracker( IList<ICaptionArea> listOfCaptionAreas )
		{
			_captionAreas = listOfCaptionAreas;
		}

		public void Hooking( UserInfo userInfo )
		{
			float x = (float)( userInfo.HandPointers[1].X * ( KinectSensorController.DEPTH_FRAME_WIDTH / 1.5 ) );
			float y = (float)( userInfo.HandPointers[1].Y * ( KinectSensorController.DEPTH_FRAME_HEIGHT / 1.5 ) );
			x = ( x < -1 ) ? -1 : x;
			y = ( y < -1 ) ? -1 : y;
			foreach( ICaptionArea captionArea in _captionAreas ) captionArea.CheckPosition( x, y );
		}

		public void AttachCaptionArea( ICaptionArea captionArea )
		{
			if( captionArea == null ) throw new ArgumentNullException( "captionArea cannot be null" );
			if( !_captionAreas.Contains( captionArea ) ) _captionAreas.Add( captionArea );
		}

		public void DetachCaptionArea( ICaptionArea captionArea )
		{
			if( captionArea == null ) throw new ArgumentNullException( "captionArea cannot be null" );
			if( _captionAreas.Contains( captionArea ) ) _captionAreas.Remove( captionArea );
		}
	}
}
