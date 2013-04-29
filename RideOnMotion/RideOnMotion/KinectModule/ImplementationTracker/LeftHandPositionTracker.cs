using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace RideOnMotion.KinectModule
{
	public class LeftHandPositionTracker : IPositionTracker
	{
		readonly IList<ICaptionArea> _captionAreas;

		public IReadOnlyList<ICaptionArea> CaptionAreas
		{
			get { return _captionAreas.AsReadOnlyList(); }
		}

		public LeftHandPositionTracker()
			: this( new List<ICaptionArea>() )
		{
		}

		public LeftHandPositionTracker(IList<ICaptionArea> listOfCaptionAreas)
		{
			_captionAreas = listOfCaptionAreas;
		}

		public void HookingSkeleton( Microsoft.Kinect.Skeleton skeleton )
		{
			ValidateCaptionArea( skeleton.Joints[JointType.HandLeft] );
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

		private void ValidateCaptionArea( Microsoft.Kinect.Joint skeletonJoint )
		{
			foreach( ICaptionArea captionArea in _captionAreas ) captionArea.CheckPosition( skeletonJoint );
		}
	}
}
