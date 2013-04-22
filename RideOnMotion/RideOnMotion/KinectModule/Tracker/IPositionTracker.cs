using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideOnMotion.KinectModule
{
	interface IPositionTracker
	{
		public int PositionX { get; }
		public int PositionY { get; }
		public int PositionZ { get; }

		public IList<ICaptionArea> CaptionAreas { get; }

		public void HookingSkeleton( Skeleton skeleton );

		public void AttachCaptionArea( ICaptionArea captionArea );
		public void DetachCaptionArea( ICaptionArea captionArea );
		private void ValidateCaptionArea( Joint skeletonJoint );
	}
}
