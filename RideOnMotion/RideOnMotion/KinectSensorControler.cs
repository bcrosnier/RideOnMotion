using Microsoft.Kinect;
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
        KinectSensor _kinectSensor = null;

        /// <summary>
        /// Indicates whether the sensor has been started. Returns false when no sensor was detected.
        /// </summary>
        public bool SensorIsRunning
        {
            // If _kinectSensor exists, return IsRunning, else return false
            get { return _kinectSensor != null ? _kinectSensor.IsRunning : false; }
        }

        /// <summary>
        /// Indicates whether the Kinect sensor has been detected.
        /// </summary>
        public bool HasSensor
        {
            // If _kinectSensor exists, return true, else return false
            get { return _kinectSensor != null ? true : false; }
        }

		public KinectSensorController()
        {
            int deviceCount = KinectSensor.KinectSensors.Count; // Blocking call.

            if ( deviceCount > 0 )
            {
                _kinectSensor = KinectSensor.KinectSensors.Where( item => item.Status == KinectStatus.Connected ).FirstOrDefault();

                if ( _kinectSensor.SkeletonStream.IsEnabled )
                {
                    _kinectSensor.SkeletonStream.Enable();
                    //connect event
                    _kinectSensor.SkeletonFrameReady += sensor_SkeletonFrameReady;
                }

                if ( !_kinectSensor.DepthStream.IsEnabled )
                {
                    _kinectSensor.DepthStream.Enable();
                }
            }
		}

        /// <summary>
        /// Attempts to start the detected Kinect sensor.
        /// </summary>
        public void StartSensor()
        {
            if ( !SensorIsRunning && HasSensor )
            {
                _kinectSensor.Start(); // Blocking call.
            }
        }

        /// <summary>
        /// Attempts to stop the sensor.
        /// </summary>
        public void StopSensor()
        {
            if ( SensorIsRunning )
            {
                _kinectSensor.Stop();
            }
        }

		private void sensor_SkeletonFrameReady( object sender, SkeletonFrameReadyEventArgs e )
		{
			throw new NotImplementedException();
		}

	}

	
}
