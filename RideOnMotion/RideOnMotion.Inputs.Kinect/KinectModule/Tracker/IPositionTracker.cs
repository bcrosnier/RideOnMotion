using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideOnMotion.Inputs.Kinect
{
    /// <summary>
    /// Position tracker.
    /// Tracks if a position (typed as T) triggers one of its ICaptionAreas.
    /// </summary>
    /// <typeparam name="T">Format of the position</typeparam>
	public interface IPositionTracker<T>
	{
        /// <summary>
        /// Areas attached to this tracker.
        /// </summary>
		IReadOnlyList<ICaptionArea> CaptionAreas { get; }

		void Hooking( T something );

		void AttachCaptionArea( ICaptionArea captionArea );
		void DetachCaptionArea( ICaptionArea captionArea );
	}
}
