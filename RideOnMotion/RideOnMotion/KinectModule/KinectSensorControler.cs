using Microsoft.Kinect;
using RideOnMotion.KinectModule.Tracker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideOnMotion.KinectModule
{

	// ToDo
	// gerer le renvoi de status du sensor
	public class KinectSensorController
	{
		KinectSensor _kinectSensor;
		IList<IPositionTracker> _positionTrackers;

		public KinectSensorController()
		{
			_kinectSensor = KinectSensor.KinectSensors.Where( item => item.Status == KinectStatus.Connected ).FirstOrDefault();

			if( _kinectSensor.SkeletonStream.IsEnabled )
			{
				_kinectSensor.SkeletonStream.Enable();
				//connect event
				_kinectSensor.SkeletonFrameReady += sensor_SkeletonFrameReady;
			}
		}

		public void StartSensor()
		{
			_kinectSensor.Start();
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

		private void sensor_SkeletonFrameReady( object sender, SkeletonFrameReadyEventArgs e )
		{
			Skeleton[] totalSkeleton = new Skeleton[6];
			using( SkeletonFrame skeletonFrame = e.OpenSkeletonFrame() )
			{

				// copy the frame data in to the collection
				skeletonFrame.CopySkeletonDataTo( totalSkeleton );

				Skeleton firstSkeleton = ( from trackskeleton in totalSkeleton
										   where trackskeleton.TrackingState == SkeletonTrackingState.Tracked
										   select trackskeleton ).FirstOrDefault();

				NotifyPositionTrackers( firstSkeleton );
			}
		}

		private void NotifyPositionTrackers( Skeleton skeleton )
		{
			foreach( IPositionTracker positionTracker in _positionTrackers ) positionTracker.HookingSkeleton( skeleton );
		}

	}

	
}
