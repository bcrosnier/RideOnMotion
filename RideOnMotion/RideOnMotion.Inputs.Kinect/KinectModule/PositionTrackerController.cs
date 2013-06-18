using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideOnMotion.Inputs.Kinect
{
	public class PositionTrackerController
	{
		IList<IPositionTracker<UserInfo>> _positionTrackers;

		public event EventHandler<AreaActivatedEventArgs> AreaActivated;

		public PositionTrackerController()
		{
			_positionTrackers = new List<IPositionTracker<UserInfo>>();
           
		}

		public void AttachPositionTracker( IPositionTracker<UserInfo> positionTracker )
		{
			if( positionTracker == null ) throw new ArgumentNullException( "positionTracker cannot be null" );
			if( !_positionTrackers.Contains( positionTracker ) ) _positionTrackers.Add( positionTracker );
		}

		public void DetachPositionTracker( IPositionTracker<UserInfo> positionTracker )
		{
			if( positionTracker == null ) throw new ArgumentNullException( "positionTracker cannot be null" );
			if( _positionTrackers.Contains( positionTracker ) ) _positionTrackers.Remove( positionTracker );
		}

		public void NotifyPositionTrackers( UserInfo userInfo )
		{
			foreach( IPositionTracker<UserInfo> positionTracker in _positionTrackers ) positionTracker.Hooking( userInfo );
		}

		protected void OnAreaActivated( ICaptionArea captionArea )
		{
			EventHandler<AreaActivatedEventArgs> handler = AreaActivated;
			if( handler != null )
			{
				handler( this, new AreaActivatedEventArgs( captionArea ) );
			}
		}

		private void MapEventOfCaptionArea( IList<ICaptionArea> captionAreas )
		{
			foreach( ICaptionArea captionArea in captionAreas )
				captionArea.PropertyChanged += captionArea_PropertyChanged;
		}

		private void captionArea_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
		{
			OnAreaActivated( (ICaptionArea)sender );
		}
	}

	public class AreaActivatedEventArgs : EventArgs
	{
		public ICaptionArea CaptionArea
		{
			get;
			private set;
		}

		public AreaActivatedEventArgs( ICaptionArea captionArea )
		{
			CaptionArea = captionArea;
		}
	}
}
