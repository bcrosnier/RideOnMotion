using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideOnMotion.Inputs.Kinect
{
    /// <summary>
    /// Controller for the PositionTrackers.
    /// Contains all trackers and can mass-notify them with a new position.
    /// </summary>
	public class PositionTrackerController
	{
		IList<IPositionTracker<UserInfo>> _positionTrackers;
        
        /// <summary>
        /// Creates an empty PositionTrackerController.
        /// </summary>
		public PositionTrackerController()
		{
			_positionTrackers = new List<IPositionTracker<UserInfo>>();
		}

        /// <summary>
        /// Add a PositionTracker to this controller.
        /// </summary>
        /// <param name="positionTracker">PositionTracker to add.</param>
		public void AttachPositionTracker( IPositionTracker<UserInfo> positionTracker )
		{
			if( positionTracker == null ) throw new ArgumentNullException( "positionTracker cannot be null" );
			if( !_positionTrackers.Contains( positionTracker ) ) _positionTrackers.Add( positionTracker );
		}

        /// <summary>
        /// Removes given PositionTracker from this controller.
        /// </summary>
        /// <param name="positionTracker">PositionTracker to remove. Will not throw an execption if it doesn't exist.</param>
		public void DetachPositionTracker( IPositionTracker<UserInfo> positionTracker )
		{
			if( positionTracker == null ) throw new ArgumentNullException( "positionTracker cannot be null" );
			if( _positionTrackers.Contains( positionTracker ) ) _positionTrackers.Remove( positionTracker );
		}

        /// <summary>
        /// Tell all position trackers to check new position data.
        /// </summary>
        /// <remarks>
        /// Position trackers will update all their CaptionArea, which should implement INotifyPropertyChanged.
        /// Didn't keep those? Oops~♪ --BC
        /// </remarks>
        /// <param name="userInfo">New position data for all trackers to check against.</param>
		public void NotifyPositionTrackers( UserInfo userInfo )
		{
			foreach( IPositionTracker<UserInfo> positionTracker in _positionTrackers ) positionTracker.Hooking( userInfo );
		}
	}
}
