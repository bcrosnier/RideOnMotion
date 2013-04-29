using Microsoft.Kinect;
using System;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RideOnMotion;

namespace RideOnMotion.KinectModule
{

	// ToDo
	// gerer le renvoi de status du sensor
	public class KinectSensorController
	{
        private KinectSensor _kinectSensor = null;
        private bool _depthFrameIsReady = false;
        private BitmapSource _depthBitmapSource = null;

        public event BitmapSourceHandler DepthBitmapSourceReady;
        public event EventHandler<KinectSensor> SensorChanged;

        public delegate void BitmapSourceHandler( object sender, BitmapSourceEventArgs e );

        public BitmapSource DepthBitmapSource
        {
          get { return _depthBitmapSource; }
        }

        public bool DepthFrameIsReady
        {
            get { return _depthFrameIsReady; }
        }

        /// <summary>
        /// Active Kinect sensor. Can be null (No sensor connected).
        /// </summary>
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

        /// <summary>
        /// Kinect status text
        /// </summary>
        public string SensorStatus
        {
            get { return _kinectSensor != null ? _kinectSensor.Status.ToString() : "No Kinect detected."; }
        }

		public KinectSensorController()
        {
            int deviceCount = KinectSensor.KinectSensors.Count; // Blocking call.

            if ( deviceCount > 0 )
            {
                KinectSensor kinectSensor = KinectSensor.KinectSensors.Where( item => item.Status == KinectStatus.Connected ).FirstOrDefault();
                initializeKinectSensor( kinectSensor );
            }

            KinectSensor.KinectSensors.StatusChanged += sensors_StatusChanged;
		}

        /// <summary>
        /// Prepares a Kinect to be started. Enables streams, among other things.
        /// Call StartSensor(); from outside after this.
        /// </summary>
        /// <param name="sensor">Kinect to set as active</param>
        private void initializeKinectSensor( KinectSensor sensor )
        {
            if ( sensor == null )
            {
                return;
            }

            _kinectSensor = sensor;

            if ( !_kinectSensor.SkeletonStream.IsEnabled )
            {
                _kinectSensor.SkeletonStream.Enable();
                //connect event
                //_kinectSensor.SkeletonFrameReady += sensor_SkeletonFrameReady;
            }


            if ( !_kinectSensor.DepthStream.IsEnabled )
            {
                _kinectSensor.DepthStream.Range = DepthRange.Default; // Change to Near mode here
                _kinectSensor.DepthFrameReady += sensor_DepthFrameReady;
                _kinectSensor.DepthStream.Enable( DepthImageFormat.Resolution640x480Fps30 );
            }


            // Call StartSensor(); from outside.
        }

        /// <summary>
        /// Remove bindings on sensor disconnection
        /// </summary>
        private void cleanupKinectSensor()
        {
            if( _kinectSensor == null )
            {
                // Nothing to clean up.
                return;
            }
            StopSensor();
            
            // Throw last BitmapSource to blank picture
            _depthBitmapSource = null;
            if ( DepthBitmapSourceReady != null )
            {
                DepthBitmapSourceReady( this, new BitmapSourceEventArgs( null ) );
            }

            _kinectSensor.DepthFrameReady -= sensor_DepthFrameReady;
            _kinectSensor.SkeletonFrameReady -= sensor_SkeletonFrameReady;

            if ( _kinectSensor.DepthStream != null )
            {
                _kinectSensor.DepthStream.Disable();
            }
            if ( _kinectSensor.SkeletonStream != null )
            {
                _kinectSensor.SkeletonStream.Disable();
            }

            _kinectSensor = null;
        }

        /// <summary>
        /// Resets the sensor (clean up, stop, reinitialize and start)
        /// </summary>
        public void resetSensor()
        {
            KinectSensor sensor = _kinectSensor; // To keep the reference locally once nulled

            cleanupKinectSensor();
            initializeKinectSensor( sensor );

            StartSensor();
        }

        /// <summary>
        /// Attempts to start the detected Kinect sensor.
        /// </summary>
        public void StartSensor()
        {
            if ( !SensorIsRunning && HasSensor )
            {
                Sensor.Start(); // Blocking call.
                if ( SensorChanged != null )
                {
                    SensorChanged( this, Sensor );
                }
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
                if ( SensorChanged != null )
                {
                    SensorChanged( this, Sensor );
                }
            }

        }

        /// <summary>
        /// Converts a depth image frame to a Bitmap source.
        /// From http://www.i-programmer.info/ebooks/practical-windows-kinect-in-c/3802-using-the-kinect-depth-sensor.html?start=1
        /// </summary>
        /// <param name="imageFrame">Image frame to convert</param>
        /// <returns>Bitmap source</returns>
        private static BitmapSource DepthToBitmapSource( DepthImageFrame imageFrame )
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
            DepthImageFrame imageFrame = e.OpenDepthImageFrame();
            if ( imageFrame == null )
            {
                return; // Already opened
            }

            _depthFrameIsReady = true;
            _depthBitmapSource = DepthToBitmapSource( imageFrame );

            if ( DepthBitmapSourceReady != null )
            {
                DepthBitmapSourceReady( this, new BitmapSourceEventArgs(_depthBitmapSource) );
            }
        }

        private void sensors_StatusChanged(object sender, StatusChangedEventArgs e) {

            if ( ( e.Status == KinectStatus.Disconnected || e.Status == KinectStatus.NotPowered )
                && e.Sensor == _kinectSensor )
            {
                // Current sensor is gone, clean up
                cleanupKinectSensor();
            }
            else if ( e.Status == KinectStatus.Connected )
            {
                initializeKinectSensor( e.Sensor );
                // New sensor has appeared
                StartSensor();
            }

            // Attach Kinect sensor if it doesn't exist
            if ( _kinectSensor == null && e.Sensor != null )
            {
                _kinectSensor = e.Sensor;
            }

            // Throw event
            if ( SensorChanged != null )
            {
                SensorChanged( this, e.Sensor );
            }
        }
	}

    public class BitmapSourceEventArgs : EventArgs
    {
        private BitmapSource _bitmapSource;

        public BitmapSource BitmapSource
        {
            get { return _bitmapSource; }
        }

        public BitmapSourceEventArgs( BitmapSource source )
        {
            _bitmapSource = source;
        }
    }
	
}
