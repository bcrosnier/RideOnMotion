using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideOnMotion.Inputs.Kinect
{
	public interface IPositionTracker<T>
	{
		IReadOnlyList<ICaptionArea> CaptionAreas { get; }

		void Hooking( T something );

		void AttachCaptionArea( ICaptionArea captionArea );
		void DetachCaptionArea( ICaptionArea captionArea );
	}
}
