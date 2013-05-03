using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideOnMotion.KinectModule
{
	public class PositionTrackerController
	{
		IList<IPositionTracker> _positionTrackers;

		public event EventHandler<AreaActivedEventArgs> AreaActived;

		public PositionTrackerController()
		{
			_positionTrackers = new List<IPositionTracker>();
            /*
			List<ICaptionArea> listOfCaptionAreas = new List<ICaptionArea>()
			{
				{ new CaptionArea( new Point( 0, 0 ), 0, 0, null ) },
				{ new CaptionArea( new Point( 0, 0 ), 0, 0, null ) },
				{ new CaptionArea( new Point( 0, 0 ), 0, 0, null ) },
				{ new CaptionArea( new Point( 0, 0 ), 0, 0, null ) }
			};
			
			_positionTrackers.Add( new LeftHandPositionTracker( listOfCaptionAreas ) );

			listOfCaptionAreas = new List<ICaptionArea>()
			{
				{ new CaptionArea( new Point( 0, 0 ), 0, 0, null ) },
				{ new CaptionArea( new Point( 0, 0 ), 0, 0, null ) },
				{ new CaptionArea( new Point( 0, 0 ), 0, 0, null ) },
				{ new CaptionArea( new Point( 0, 0 ), 0, 0, null ) }
			};

			_positionTrackers.Add( new RightHandPositionTracker( listOfCaptionAreas ) );
             */
		}

		public void AttachPositionTracker( IPositionTracker positionTracker )
		{
			if( positionTracker == null ) throw new ArgumentNullException( "positionTracker cannot be null" );
			if( !_positionTrackers.Contains( positionTracker ) ) _positionTrackers.Add( positionTracker );
		}

		public void DetachPositionTracker( IPositionTracker positionTracker )
		{
			if( positionTracker == null ) throw new ArgumentNullException( "positionTracker cannot be null" );
			if( _positionTrackers.Contains( positionTracker ) ) _positionTrackers.Remove( positionTracker );
		}

		public void NotifyPositionTrackers( Skeleton skeleton )
		{
			if( skeleton != null && skeleton.TrackingState == SkeletonTrackingState.Tracked )
			{
				foreach( IPositionTracker positionTracker in _positionTrackers ) positionTracker.HookingSkeleton( skeleton );
			}
		}

		protected void OnAreaActived( ICaptionArea captionArea )
		{
			EventHandler<AreaActivedEventArgs> handler = AreaActived;
			if( handler != null )
			{
				handler( this, new AreaActivedEventArgs( captionArea ) );
			}
		}

		private void MapEventOfCaptionArea( IList<ICaptionArea> captionAreas )
		{
			foreach( ICaptionArea captionArea in captionAreas )
				captionArea.PropertyChanged += captionArea_PropertyChanged;
		}

		private void captionArea_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
		{
			OnAreaActived( (ICaptionArea)sender );
		}
	}

	public class AreaActivedEventArgs : EventArgs
	{
		public ICaptionArea CaptionArea
		{
			get;
			private set;
		}

		public AreaActivedEventArgs( ICaptionArea captionArea )
		{
			CaptionArea = captionArea;
		}
	}
}
