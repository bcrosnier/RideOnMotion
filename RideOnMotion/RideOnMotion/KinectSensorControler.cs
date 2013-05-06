using Microsoft.Kinect;
using System;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RideOnMotion;
using System.Collections.ObjectModel;

namespace RideOnMotion.KinectModule
{
    public class KinectSensorController
    {
        private KinectSensor _kinectSensor = null;
        private bool _depthFrameIsReady = false;
        private BitmapSource _depthBitmapSource = null;
        private TransformSmoothParameters _smoothingParam;
        private bool _enableSmoothing;
        Skeleton[] _totalSkeleton;

        public TriggerArea LeftTriggerArea { get; private set; }
        public TriggerArea RightTriggerArea { get; private set; }

        public ObservableCollection<ICaptionArea> TriggerButtons { get; private set; }

        PositionTrackerController _positionTrackerController;

        public event BitmapSourceHandler DepthBitmapSourceReady;
        public event EventHandler<KinectSensor> SensorChanged;

        public event EventHandler<System.Windows.Point[]> HandsPointReady;
        private bool _handsVisible;

        public delegate void BitmapSourceHandler( object sender, BitmapSourceEventArgs e );
        public delegate SkeletonPoint DepthPointToSkelPoint( DepthImagePoint p );
        public delegate DepthImagePoint SkelPointToDepthPoint( SkeletonPoint p );

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
            get { return ( _kinectSensor != null && _kinectSensor.Status != KinectStatus.Disconnected ) ? true : false; }
        }

        /// <summary>
        /// Kinect status text
        /// </summary>
        public string SensorStatus
        {
            get { return _kinectSensor != null ? _kinectSensor.Status.ToString() : "No Kinect detected."; }
        }

        /// <summary>
        ///
        /// </summary>
        public PositionTrackerController PositionTrackerController
        {
            get { return _positionTrackerController; }
        }

        public KinectSensorController()
        {
            int deviceCount = KinectSensor.KinectSensors.Count; // Blocking call.
            TriggerButtons = new ObservableCollection<ICaptionArea>();

            if ( deviceCount > 0 )
            {
                KinectSensor kinectSensor = KinectSensor.KinectSensors.Where( item => item.Status == KinectStatus.Connected ).FirstOrDefault();
                SetSkeletonSmoothingEnabled( false );
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
                _kinectSensor.SkeletonFrameReady += sensor_SkeletonFrameReady;
            }
            else
            {
                _kinectSensor.SkeletonStream.Disable();
            }

            _totalSkeleton = new Skeleton[6];

            if ( this._enableSmoothing == true )
            {
                System.Diagnostics.Debug.Assert( this._enableSmoothing == true && this._smoothingParam != null );
                _kinectSensor.SkeletonStream.Enable( _smoothingParam );
            }
            else
            {
                _kinectSensor.SkeletonStream.Enable();
            }


            if ( !_kinectSensor.DepthStream.IsEnabled )
            {
                _kinectSensor.DepthFrameReady += sensor_DepthFrameReady;
            }
            else
            {
                _kinectSensor.DepthStream.Disable();
            }
            _handsVisible = false;

            _kinectSensor.DepthStream.Range = DepthRange.Default; // Change to Near mode here
            _kinectSensor.DepthStream.Enable( DepthImageFormat.Resolution640x480Fps30 );


            initializePositionTrackerController();
            // Call StartSensor(); from outside.
        }

        /// <summary>
        /// Créer les PositionTracker et les CaptionArea associe
        /// </summary>
        private void initializePositionTrackerController()
        {
            _positionTrackerController = new PositionTrackerController();

            initTriggerZones( 300, 100 );
        }

        /// <summary>
        /// Prepare the right and left trigger zones.
        /// </summary>
        /// <param name="buttonWidth">Width of each button / side size of the zone</param>
        /// <param name="buttonHeight">Height of each button / thickness of the triggers</param>
        private void initTriggerZones( int buttonWidth, int buttonHeight )
        {
            int zoneWidth = 640 / 2;
            int zoneHeight = 480;

            LeftTriggerArea = new TriggerArea( zoneWidth, zoneHeight, 0, 0, buttonWidth, buttonHeight, this.SkelPointToDepthImagePoint );
            RightTriggerArea = new TriggerArea( zoneWidth, zoneHeight, zoneWidth, 0, buttonWidth, buttonHeight, this.SkelPointToDepthImagePoint );

            // Create a collection from both zones
            TriggerButtons = new ObservableCollection<ICaptionArea>(
                LeftTriggerArea.TriggerCaptionsCollection.Values.Union(
                        RightTriggerArea.TriggerCaptionsCollection.Values
                ).ToList()
            );

            // Create caption areas

            IPositionTracker leftTracker = new LeftHandPositionTracker( LeftTriggerArea.TriggerCaptionsCollection.Values.ToList() );
            IPositionTracker rightTracker = new RightHandPositionTracker( RightTriggerArea.TriggerCaptionsCollection.Values.ToList() );

            this.PositionTrackerController.AttachPositionTracker( leftTracker );
            this.PositionTrackerController.AttachPositionTracker( rightTracker );

        }

        /// <summary>
        /// Remove bindings on sensor disconnection
        /// </summary>
        private void cleanupKinectSensor()
        {
            if ( _kinectSensor == null )
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
                try
                {
                    Sensor.Start(); // Blocking call.
                    SensorChanged( this, Sensor ); //
                }
                catch
                {
                    Logger.Instance.NewEntry( CK.Core.LogLevel.Fatal, CKTraitTags.Kinect, "Unexpected API error, replug the Kinect." );
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
            }
        }

        public void SetSkeletonSmoothingEnabled( bool enabled )
        {
            if ( !enabled )
            {
                this._enableSmoothing = false;
            }
            else if ( enabled )
            {
                this._enableSmoothing = true;
                // Smoothed with some latency.
                // Filters out medium jitters.
                // Good for a menu system that needs to be smooth but
                // doesn't need the reduced latency as much as gesture recognition does.
                // From: http://msdn.microsoft.com/en-us/library/jj131024.aspx
                this._smoothingParam = new TransformSmoothParameters();
                {
                    _smoothingParam.Smoothing = 0.5f;
                    _smoothingParam.Correction = 0.1f;
                    _smoothingParam.Prediction = 0.5f;
                    _smoothingParam.JitterRadius = 0.1f;
                    _smoothingParam.MaxDeviationRadius = 0.1f;
                };
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
            imageFrame.CopyPixelDataTo( pixelData );

            BitmapSource bmap = BitmapSource.Create(
                imageFrame.Width,
                imageFrame.Height,
                96, 96,
                PixelFormats.Gray16,
                null,
                pixelData,
                imageFrame.Width * imageFrame.BytesPerPixel );
            return bmap;
        }

        private void sensor_SkeletonFrameReady( object sender, SkeletonFrameReadyEventArgs e )
        {

            using ( SkeletonFrame skeletonFrame = e.OpenSkeletonFrame() )
            {
                if ( skeletonFrame != null )
                {
                    // copy the frame data in to the collection
                    skeletonFrame.CopySkeletonDataTo( _totalSkeleton );

                    var trackedSkeletons = ( from trackskeleton in _totalSkeleton
                                             where trackskeleton.TrackingState == SkeletonTrackingState.Tracked
                                             select trackskeleton );

                    int skeletonCount = trackedSkeletons.Count();

                    Skeleton firstSkeleton = trackedSkeletons.FirstOrDefault();
                    if ( firstSkeleton != null )
                    {
                        _handsVisible = true;
                        _positionTrackerController.NotifyPositionTrackers( firstSkeleton );

                        if ( HandsPointReady != null )
                        {
                            HandsPointReady( this, new System.Windows.Point[2] { ScalePosition( firstSkeleton.Joints[JointType.HandLeft].Position ), ScalePosition( firstSkeleton.Joints[JointType.HandRight].Position ) } );
                        }
                    }
                    else if ( _handsVisible == true )
                    {
                        HandsPointReady( this, new System.Windows.Point[2] { new System.Windows.Point( -1, -1 ), new System.Windows.Point( -1, -1 ) } );
                        _handsVisible = false;
                    }
                }
            }
        }

        private System.Windows.Point ScalePosition( SkeletonPoint skeletonPoint )
        {
            DepthImagePoint depthPoint = this.Sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(
                skeletonPoint, DepthImageFormat.Resolution640x480Fps30 );
            return new System.Windows.Point( depthPoint.X, depthPoint.Y );
        }

        private SkeletonPoint DepthImagePointToSkelPoint( DepthImagePoint p )
        {
            SkeletonPoint skelPoint = this.Sensor.CoordinateMapper.MapDepthPointToSkeletonPoint(
                DepthImageFormat.Resolution640x480Fps30,
                p );
            return skelPoint;
        }

        private DepthImagePoint SkelPointToDepthImagePoint( SkeletonPoint p )
        {
            DepthImagePoint depthPoint = this.Sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(
                 p, DepthImageFormat.Resolution640x480Fps30 );
            return depthPoint;
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
                DepthBitmapSourceReady( this, new BitmapSourceEventArgs( _depthBitmapSource ) );
            }
            imageFrame.Dispose();
        }

        private void sensors_StatusChanged( object sender, StatusChangedEventArgs e )
        {

            Logger.Instance.NewEntry( CK.Core.LogLevel.Trace, CKTraitTags.Kinect, "Status changed to : " + e.Status.ToString() );
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
            if ( _kinectSensor == null && e.Sensor != null && e.Status != KinectStatus.Disconnected )
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
