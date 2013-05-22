using Microsoft.Kinect;
using System;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RideOnMotion;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Collections;
using RideOnMotion.Utilities;
using System.ComponentModel;
using System.Windows.Threading;

namespace RideOnMotion.Inputs.Kinect
{
    /// <summary>
    /// Main controller class for drone input through a Kinect sensor device.
    /// </summary>
    public class KinectSensorController : IDroneInputController
    {
        /// <summary>
        /// A user-friendly input name!
        /// </summary>
        private static readonly string INPUT_NAME = "Kinect";

        /// <summary>
        /// Default format used for the depth image stream.
        /// </summary>
        private static readonly DepthImageFormat DEPTH_IMAGE_FORMAT = DepthImageFormat.Resolution640x480Fps30;

        /// <summary>
        /// Width of the depth frame. Must match DEPTH_IMAGE_FORMAT.
        /// </summary>
        private static readonly int DEPTH_FRAME_WIDTH = 640;

        /// <summary>
        /// Height of the depth frame. Must match DEPTH_IMAGE_FORMAT.
        /// </summary>
        private static readonly int DEPTH_FRAME_HEIGHT = 480;

        /// <summary>
        /// Default trigger zone width (side size)
        /// </summary>
        private static readonly int TRIGGER_BUTTON_WIDTH = 300;

        /// <summary>
        /// Default trigger zone height (button thickness)
        /// </summary>
        private static readonly int TRIGGER_BUTTON_HEIGHT = 100;

        /// <summary>
        /// The Kinect sensor used by the controller. Can be null.
        /// </summary>
        private KinectSensor _kinectSensor = null;

        /// <summary>
        /// The last BitmapSource given by the depth camera.
        /// </summary>
        private BitmapSource _depthBitmapSource = null;

        /// <summary>
        /// Default smoothing parameters for the skeleton.
        /// </summary>
        private TransformSmoothParameters _smoothingParam;

        /// <summary>
        /// Handler for the hands' zone detection and activation.
        /// </summary>
        private PositionTrackerController _positionTrackerController;

        /// <summary>
        /// TriggerArea for the left hand.
        /// </summary>
        private TriggerArea LeftTriggerArea { get; set; }

        /// <summary>
        /// TriggerArea for the right hand.
        /// </summary>
        private TriggerArea RightTriggerArea { get; set; }

        /// <summary>
        /// Whether a depth frame is ready to use.
        /// </summary>
        private bool _depthFrameIsReady;

        /// <summary>
        /// Whether hands are visible, and tracked.
        /// </summary>
        private bool _handsVisible;

        /// <summary>
        /// Whether to enable skeleton smoothing. <see cref="_smoothingParam"/>
        /// </summary>
        private bool _enableSmoothing;

        /// <summary>
        /// Array of all skeletons that could be detected.
        /// </summary>
        private Skeleton[] _totalSkeleton;

		private DispatcherTimer _timerToLand;

        /// <summary>
        /// Collection of all ICaptionAreas for the left and right hands.
        /// </summary>
        internal ObservableCollectionEx<ICaptionArea> TriggerButtons { get; private set; }

        /// <summary>
        /// Fired when the active Kinect sensor changes.
        /// </summary>
        private event EventHandler<KinectSensor> SensorChanged;

        /// <summary>
        /// Fired when new points are available for the hands.
        /// </summary>
        public event EventHandler<System.Windows.Point[]> HandsPointReady;

		/// <summary>
		/// Fired when security recquire hover mode.
		/// 0 -> No problem
		/// 1 -> Hover mode activation
		/// 2 -> Land the drone
		/// </summary>
		public event EventHandler<int> SecurityModeNeeded;

        /// <summary>
        /// Converts a DepthImagePoint to a SkeletonPoint, using this controller's depth tracking data.
        /// </summary>
        /// <param name="p">DepthImagePoint to convert.</param>
        /// <returns>Equivalent SkeletonPoint</returns>
        public delegate SkeletonPoint DepthPointToSkelPoint( DepthImagePoint p );

        /// <summary>
        /// Converts a SkeletonPoint to a DepthImagePoint, using this controller's depth tracking data.
        /// </summary>
        /// <param name="p">SkeletonPoint to convert.</param>
        /// <returns>Equivalent DepthImagePoint</returns>
        public delegate DepthImagePoint SkelPointToDepthPoint( SkeletonPoint p );

		/// <summary>
		/// Determine the number of active triggerButtons
		/// </summary>
		internal int _triggerButtonsActive;

		/// <summary>
		/// Contain the inputState
		/// </summary>
		private InputState _inputState;


        #region Interface implementation
        // Events
        public event EventHandler<BitmapSource> InputImageSourceChanged;
        public event EventHandler<DroneInputStatus> InputStatusChanged;
		public event EventHandler<bool[]> InputsStateChanged;
		public event EventHandler<bool> ControllerActivity;

        // Properties
        public MenuItem InputMenu
        {
            get;
            private set;
        }

        /// <summary>
        /// Kinect status text
        /// </summary>
        public string InputStatusString
        {
            get
            {
                if ( _kinectSensor != null )
                {
                    return _kinectSensor.Status.ToString();
                }
                else
                {
                    return "No Kinect detected.";
                }
            }
        }

        public string Name
        {
            get { return INPUT_NAME; }
        }

        public BitmapSource InputImageSource
        {
            get { return _depthBitmapSource; }
        }

        public Control InputUIControl
        {
            get;
            private set;
        }

        public DroneInputStatus InputStatus
        {
            get
            {
                if ( _kinectSensor == null || _kinectSensor.Status == KinectStatus.Disconnected )
                {
                    return DroneInputStatus.Disconnected;
                }
                else if ( _kinectSensor.Status == KinectStatus.Connected )
                {
                    return DroneInputStatus.Ready;
                }
                else
                {
                    return DroneInputStatus.NotReady;
                }
            }
        }

        public IDroneController ActiveDrone
        {
            set { throw new NotImplementedException(); }
        }

		public bool IsActive
		{
			get
			{
				int activesButtons = 0;
				System.Collections.Generic.List<ICaptionArea> triggerActive = TriggerButtons.Where( ( button ) => button.IsActive == true ).ToList();
				foreach( ICaptionArea flag in triggerActive)
				{
					activesButtons += flag.Id;
				}

				if ( activesButtons != _triggerButtonsActive )
				{
					_triggerButtonsActive = activesButtons;
					ControllerActivity( this, true );
					return true;
				}
				else
				{
					ControllerActivity( this, false );
					return false;
				}
			}
		}

        #endregion Interface implementation

        private bool DepthFrameIsReady
        {
            get { return _depthFrameIsReady; }
        }

        /// <summary>
        /// Active Kinect sensor. Can be null (No sensor connected).
        /// </summary>
        internal KinectSensor Sensor
        {
            get { return _kinectSensor; }
        }

        /// <summary>
        /// Indicates whether the sensor has been started. Returns false when no sensor was detected.
        /// </summary>
        private bool SensorIsRunning
        {
            // If _kinectSensor exists, return IsRunning, else return false
            get { return _kinectSensor != null ? _kinectSensor.IsRunning : false; }
        }

        /// <summary>
        /// Indicates whether the Kinect sensor has been detected.
        /// </summary>
        private bool HasSensor
        {
            // If _kinectSensor exists, return true, else return false
            get { return ( _kinectSensor != null && _kinectSensor.Status != KinectStatus.Disconnected ) ? true : false; }
        }

        /// <summary>
        ///
        /// </summary>
        private PositionTrackerController PositionTrackerController
        {
            get { return _positionTrackerController; }
        }

        /// <summary>
        /// Gets whether skeleton smoothing is enabled.
        /// </summary>
		public bool SmoothingEnabled
		{
			get { return this._enableSmoothing; }
		}

		/// <summary>
		/// Return the current input state 
		/// </summary>
		public bool[] InputState
		{
			get
			{
				return _inputState.CurrentInputState;
			}
		}

        /// <summary>
        /// Kinect sensor input controller.
        /// Handles drone control through a Kinect sensor.
        /// </summary>
        public KinectSensorController()
        {
            int deviceCount = KinectSensor.KinectSensors.Count; // Blocking call (USB devices polling).

            TriggerButtons = new ObservableCollectionEx<ICaptionArea>();
            initTriggerZones( TRIGGER_BUTTON_WIDTH, TRIGGER_BUTTON_HEIGHT );

            this.InputMenu = PrepareInputMenuItem();
            this.InputStatusChanged += OnInputStatusChanged;

            if ( deviceCount > 0 )
            {
                KinectSensor kinectSensor = KinectSensor.KinectSensors.Where( item => item.Status == KinectStatus.Connected ).FirstOrDefault();
                SetSkeletonSmoothingEnabled( false );
                initializeKinectSensor( kinectSensor );
				initializeSecurityTimer();
            }

            KinectSensor.KinectSensors.StatusChanged += sensors_StatusChanged;

			this._inputState = new InputState();

            this.InputUIControl = new KinectSensorControllerUI( this );

        }

        /// <summary>
        /// Fired on InputStatusChanged to enable/disable the menus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnInputStatusChanged( object sender, DroneInputStatus e )
        {
            this.updateMenusStatus();
        }

        /// <summary>
        /// Prepares a Kinect to be started. Enables streams, among other things.
        /// Call Start(); from outside after this.
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

            _kinectSensor.DepthStream.Enable( DEPTH_IMAGE_FORMAT );
			_kinectSensor.SkeletonStream.EnableTrackingInNearRange = true;

            initializePositionTrackerController();
            // Call Start(); from outside.
        }

		private void initializeSecurityTimer()
		{
			_timerToLand = new DispatcherTimer();
			_timerToLand.Interval = new TimeSpan( 0, 0, 3 );
			_timerToLand.Tick += new EventHandler( timerToLand_Tick );
		}

        /// <summary>
        /// Initializes the position tracker and its related trigger zones for the hands.
        /// </summary>
        private void initializePositionTrackerController()
        {
            _positionTrackerController = new PositionTrackerController();

            IPositionTracker leftTracker = new LeftHandPositionTracker( LeftTriggerArea.TriggerCaptionsCollection.Values.ToList() );
            IPositionTracker rightTracker = new RightHandPositionTracker( RightTriggerArea.TriggerCaptionsCollection.Values.ToList() );

            this.PositionTrackerController.AttachPositionTracker( leftTracker );
            this.PositionTrackerController.AttachPositionTracker( rightTracker );
        }

        /// <summary>
        /// Prepare the right and left trigger zones.
        /// </summary>
        /// <param name="buttonWidth">Width of each button / side size of the zone</param>
        /// <param name="buttonHeight">Height of each button / thickness of the triggers</param>
        private void initTriggerZones( int buttonWidth, int buttonHeight )
        {
            int zoneWidth = DEPTH_FRAME_WIDTH / 2;
            int zoneHeight = DEPTH_FRAME_HEIGHT;

            // Create caption areas
            LeftTriggerArea = new TriggerArea( zoneWidth, zoneHeight, 0, 0, buttonWidth, buttonHeight, this.SkelPointToDepthImagePoint, true);
            RightTriggerArea = new TriggerArea( zoneWidth, zoneHeight, zoneWidth, 0, buttonWidth, buttonHeight, this.SkelPointToDepthImagePoint, false);

            // Create a collection from both zones
                LeftTriggerArea.TriggerCaptionsCollection.Values.Union(
                        RightTriggerArea.TriggerCaptionsCollection.Values
                ).ToList().ForEach((element) => TriggerButtons.Add(element));
			( (INotifyPropertyChanged)TriggerButtons ).PropertyChanged += ( x, y ) => ChangeCurrentInputState(x,y);
        }

		internal void ChangeCurrentInputState( object x, PropertyChangedEventArgs y )
		{
			bool[] inputs = new bool[8];
			for ( int i = 0; i < 8; i++)
			{
				inputs[i] = ( (ObservableCollectionEx<ICaptionArea>)x )[i].IsActive;
			}
			if ( _inputState.CheckInput( inputs ) )
			{
				if ( InputsStateChanged != null )
				{
					InputsStateChanged( this, InputState );
				}
			}
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
            Stop(); // HAMMER TIME.

            // Throw last BitmapSource to blank picture
            _depthBitmapSource = null;
            if ( InputImageSourceChanged != null )
            {
                InputImageSourceChanged( this, null );
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

            Start();
        }

        /// <summary>
        /// Attempts to start the detected Kinect sensor.
        /// </summary>
        public void Start()
        {
            if ( !SensorIsRunning && HasSensor )
            {
                try
                {
                    Sensor.Start(); // Blocking call. May throw IOException on already used, or worse...!

                    if ( SensorChanged != null )
                    {
                        SensorChanged( this, Sensor );
                    }

                }
                catch ( System.IO.IOException e )
                {
                    Logger.Instance.NewEntry( CK.Core.LogLevel.Fatal, CKTraitTags.Kinect, "Kinect is already in use by another process. Error:" );
                    Logger.Instance.NewEntry( CK.Core.LogLevel.Fatal, CKTraitTags.Kinect, e.Message );
                }
                catch ( Exception e )
                {
                    Logger.Instance.NewEntry( CK.Core.LogLevel.Fatal, CKTraitTags.Kinect, "Unexpected Kinect API error:" );
                    Logger.Instance.NewEntry( CK.Core.LogLevel.Fatal, CKTraitTags.Kinect, e.Message );
                }
            }

            updateMenusStatus();
        }

        /// <summary>
        /// Attempts to stop the sensor.
        /// </summary>
        public void Stop()
        {
            if ( SensorIsRunning )
            {
                _kinectSensor.Stop();
            }
        }

        /// <summary>
        /// Enable smoothing on skeleton tracking.
        /// Kinect needs to be reinitialized from the outside after this.
        /// </summary>
        /// <param name="enabled">True to enable, false to disable smoothing.</param>
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
        /// Converts a depth image frame to a Bitmap source using a simple 16-bit Grayscale mapping.
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
        
        /// <summary>
        /// Method fired when a new skeleton frame is ready.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
						securityHoverMode( firstSkeleton );
                        if ( HandsPointReady != null )
                        {
                            HandsPointReady( this,
                                new System.Windows.Point[2] {
                                    SkelPointTo2DDepthPoint( firstSkeleton.Joints[JointType.HandLeft].Position ),
                                    SkelPointTo2DDepthPoint( firstSkeleton.Joints[JointType.HandRight].Position )
                                }
                             );
                        }
                    }
                    else if ( _handsVisible == true )
                    {
                        if ( HandsPointReady != null )
                        {
                            HandsPointReady( this, new System.Windows.Point[2] { new System.Windows.Point( -1, -1 ), new System.Windows.Point( -1, -1 ) } );
                        }
                        _handsVisible = false;
                    }
                }
            }
        }

		public void securityHoverMode(Skeleton skeleton)
		{
			SkeletonTrackingState skeletonState = skeleton.TrackingState;

			if( skeleton.TrackingState == SkeletonTrackingState.Tracked )
			{
				if( skeleton.Joints[JointType.HandLeft].TrackingState != JointTrackingState.Tracked
					|| skeleton.Joints[JointType.HandRight].TrackingState != JointTrackingState.Tracked )
				{
					
					SecurityModeNeeded( this, 1 );

					if(_timerToLand.IsEnabled == false )
					{
						_timerToLand.Start();
					}
				}
				else if( _timerToLand.IsEnabled == true )
					_timerToLand.Stop();
			}
			else
			{
				SecurityModeNeeded( this, 1 );

				if( _timerToLand.IsEnabled == false )
				{
					_timerToLand.Start();
				}
			}
		}

		private void timerToLand_Tick( object sender, EventArgs e )
		{
			SecurityModeNeeded( this, 2 );
		}

        /// <summary>
        /// Converts a SkeletonPoint to a DepthImagePoint, taking only the X and Y values.
        /// </summary>
        /// <param name="skeletonPoint">SkeletonPoint to convert</param>
        /// <returns>Point in 2D space of its DepthImagePoint equivalent</returns>
        private System.Windows.Point SkelPointTo2DDepthPoint( SkeletonPoint skeletonPoint )
        {
            DepthImagePoint depthPoint = SkelPointToDepthImagePoint( skeletonPoint );
            return new System.Windows.Point( depthPoint.X, depthPoint.Y );
        }

        /// <summary>
        /// Converts a DepthImagePoint to a SkeletonPoint.
        /// </summary>
        /// <param name="p">DepthImagePoint to convert</param>
        /// <returns>SkeletonPoint equivalent</returns>
        private SkeletonPoint DepthImagePointToSkelPoint( DepthImagePoint p )
        {
            SkeletonPoint skelPoint = this.Sensor.CoordinateMapper.MapDepthPointToSkeletonPoint(
                DEPTH_IMAGE_FORMAT,
                p );
            return skelPoint;
        }

        /// <summary>
        /// Converts a SkeletonPoint to a DepthImagePoint.
        /// </summary>
        /// <param name="p">SkeletonPoint to convert</param>
        /// <returns>DepthImagePoint equivalent</returns>
        private DepthImagePoint SkelPointToDepthImagePoint( SkeletonPoint p )
        {
            DepthImagePoint depthPoint = this.Sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(
                 p, DEPTH_IMAGE_FORMAT );
            return depthPoint;
        }

        /// <summary>
        /// Fired on every depth frame.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sensor_DepthFrameReady( object sender, DepthImageFrameReadyEventArgs e )
        {
            DepthImageFrame imageFrame = e.OpenDepthImageFrame();
            if ( imageFrame == null )
            {
                return; // Clear frame
            }

            _depthFrameIsReady = true;
            _depthBitmapSource = DepthToBitmapSource( imageFrame );

            if ( InputImageSourceChanged != null )
            {
                InputImageSourceChanged( this, _depthBitmapSource );
            }
            imageFrame.Dispose();
        }
        
        /// <summary>
        /// Fired every time any sensor on the system changes its status.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                Start();
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

            // Throw interface event
            if ( InputStatusChanged != null )
            {
                InputStatusChanged( this, this.InputStatus );
            }
        }

        /// <summary>
        /// Create the menu items for the UI.
        /// </summary>
        /// <returns>New MenuItem for the active Kinect device.</returns>
        private MenuItem PrepareInputMenuItem()
        {
            MenuItem mainMenuItem = new MenuItem();
            mainMenuItem.Header = "Kinect";

            MenuItem settingsMenuItem = new MenuItem();
            settingsMenuItem.Header = "Kinect settings";
            settingsMenuItem.Click += settingsMenuItem_Click;
            mainMenuItem.Items.Add( settingsMenuItem );

            mainMenuItem.Items.Add( new Separator() );

            MenuItem resetMenuItem = new MenuItem();
            resetMenuItem.Header = "Reset device";
            resetMenuItem.Click += resetMenuItem_Click;
            mainMenuItem.Items.Add( resetMenuItem );

            return mainMenuItem;
        }

        /// <summary>
        /// Enable or disable menu availability, depending on whether Kinect is ready
        /// </summary>
        void updateMenusStatus()
        {
            if( this.InputMenu != null )
            {
                if ( this.InputStatus == DroneInputStatus.Ready )
                {
                    this.InputMenu.IsEnabled = true;
                }
                else
                {
                    this.InputMenu.IsEnabled = false;
                }
            }

        }

        /// <summary>
        /// Fired when the "Reset device" menu item is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void resetMenuItem_Click( object sender, RoutedEventArgs e )
        {
            if ( this.InputStatus != DroneInputStatus.Ready )
            {
                return; // Command should be disabled anyway
            }
            this.resetSensor();
        }

        /// <summary>
        /// Fired when the "Kinect settings" menu item is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void settingsMenuItem_Click( object sender, RoutedEventArgs e )
        {
            if ( this.InputStatus != DroneInputStatus.Ready )
            {
                return; // Command should be disabled anyway
            }

            Window deviceSettingsWindow = new KinectDeviceSettings( this );
            deviceSettingsWindow.Show();

            e.Handled = true;
        }
    }
}
