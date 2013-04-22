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

		private void sensor_SkeletonFrameReady( object sender, SkeletonFrameReadyEventArgs e )
		{
			throw new NotImplementedException();
		}

	}

	
}
