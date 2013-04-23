using Microsoft.Kinect;
using System;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RideOnMotion.KinectModule
{

	// ToDo
	// gerer le renvoi de status du sensor
	public class KinectSensorController
	{
        private KinectSensor _kinectSensor = null;
        private bool _depthFrameIsReady = false;
        private BitmapSource _depthBitmapSource = null;

        public BitmapSource DepthBitmapSource
        {
          get { return _depthBitmapSource; }
        }

        public bool DepthFrameIsReady
        {
            get { return _depthFrameIsReady; }
        }

        public KinectSensor Sensor
        {
            get { return _kinectSensor; }
        }

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

        public string SensorStatus
        {
            get { return _kinectSensor != null ? _kinectSensor.Status.ToString() : "No Kinect detected."; }
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
                    _kinectSensor.DepthStream.Enable( DepthImageFormat.Resolution640x480Fps30 );
                    _kinectSensor.DepthFrameReady += sensor_DepthFrameReady;
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

        /// <summary>
        /// Converts a depth image frame to a Bitmap source.
        /// From http://www.i-programmer.info/ebooks/practical-windows-kinect-in-c/3802-using-the-kinect-depth-sensor.html?start=1
        /// </summary>
        /// <param name="imageFrame">image frame to convert</param>
        /// <returns>Bitmap source</returns>
        private BitmapSource DepthToBitmapSource( DepthImageFrame imageFrame)
        {
            short[] pixelData = new short[imageFrame.PixelDataLength];
            imageFrame.CopyPixelDataTo(pixelData);

            BitmapSource bmap = BitmapSource.Create(
                imageFrame.Width,
                imageFrame.Height,
                96, 96,
                PixelFormats.Gray16,
                null,
                pixelData,
                imageFrame.Width*imageFrame.BytesPerPixel);
            return bmap;
        }

		private void sensor_SkeletonFrameReady( object sender, SkeletonFrameReadyEventArgs e )
		{
			throw new NotImplementedException();
		}

        private void sensor_DepthFrameReady( object sender, DepthImageFrameReadyEventArgs e )
        {
            _depthFrameIsReady = true;
            DepthImageFrame imageFrame = e.OpenDepthImageFrame();
            _depthBitmapSource = DepthToBitmapSource( imageFrame );
        }
	}

	
}
