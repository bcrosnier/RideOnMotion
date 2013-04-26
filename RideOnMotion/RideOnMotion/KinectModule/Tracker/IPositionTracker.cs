using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideOnMotion.KinectModule
{
	public interface IPositionTracker
	{
		IReadOnlyList<ICaptionArea> CaptionAreas { get; }

		void HookingSkeleton( Skeleton skeleton );

		void AttachCaptionArea( ICaptionArea captionArea );
		void DetachCaptionArea( ICaptionArea captionArea );
	}
}
