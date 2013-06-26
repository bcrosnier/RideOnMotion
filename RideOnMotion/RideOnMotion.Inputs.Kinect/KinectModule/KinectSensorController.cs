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
using RideOnMotion.Inputs;
using RideOnMotion.Utilities;
using System.ComponentModel;
using Microsoft.Kinect.Toolkit.Interaction;
using System.Windows.Threading;
using System.Collections.Generic;
using CK.Core;

namespace RideOnMotion.Inputs.Kinect
{
    /// <summary>
    /// Main controller class for drone input through a Kinect sensor device.
    /// </summary>
    public class KinectSensorController : IDroneInputController
	{
		#region declarations
        float roll = 0;
        float pitch = 0;
        float yaw = 0;
        float gaz = 0;
        bool cameraSwap = false;
        bool takeOff = false;
        bool land = false;
        bool hover = false;
        bool emergency = false;
        bool flatTrim = false;
		bool specialActionButton = false;
        RideOnMotion.Inputs.InputState _lastInputState = new RideOnMotion.Inputs.InputState();
        Point _leftHand = new Point (-1, -1);
        Point _rightHand = new Point (-1, -1);
        DispatcherTimer ReleaseLeftHand = new DispatcherTimer();
        DispatcherTimer ReleaseRightHand = new DispatcherTimer();

        private IActivityLogger _logger;

		/// <summary>
        /// A user-friendly input name!
        /// </summary>
        public static readonly string INPUT_NAME = "Kinect";

        /// <summary>
        /// Default format used for the depth image stream.
        /// </summary>
        public static readonly DepthImageFormat DEPTH_IMAGE_FORMAT = DepthImageFormat.Resolution640x480Fps30;

        /// <summary>
        /// Width of the depth frame. Must match DEPTH_IMAGE_FORMAT.
        /// </summary>
        public static readonly int DEPTH_FRAME_WIDTH = 640;

        /// <summary>
        /// Height of the depth frame. Must match DEPTH_IMAGE_FORMAT.
        /// </summary>
		public static readonly int DEPTH_FRAME_HEIGHT = 480;

        /// <summary>
        /// Default trigger zone width (side size)
        /// </summary>
        public static readonly int TRIGGER_BUTTON_WIDTH = 80;

        /// <summary>
        /// Default trigger zone height (button thickness)
        /// </summary>
        public static readonly int TRIGGER_BUTTON_HEIGHT = (int)(TRIGGER_BUTTON_WIDTH * (4.0 / 3.0));

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
		private TriggerArea _leftTriggerArea;

        /// <summary>
        /// TriggerArea for the right hand.
        /// </summary>
		private TriggerArea _rightTriggerArea;

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

		private bool _skeletonFound = false;

		private bool _canTakeOff = false;

		private UserInfo[] _userInfo;

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
        /// 3 -> Take off
		/// </summary>
		public event EventHandler<int> SecurityModeChanged;
        
        /// <summary>
        /// A person was detected and motioned to control the drone. Drone can now take off.
        /// </summary>
		public event EventHandler ControlAcquired;

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
		/// Used to know if the safety has already forced the drone to land
		/// </summary>
		internal bool _safetyLandingtriggered = false;

		/// <summary>
		/// Contain the inputState
		/// </summary>
		private InputState _inputState;

		private InteractionStream _interactionStream;
        private InteractionClient _interactionClient;

		/// <summary>
		/// Manage the grip and grip release
        /// </summary>
        private bool _leftGrip = false;
        private bool _rightGrip = false;
        private bool _operatorLost = false;
        private bool _lastOperatorLost = false;

        /// <summary>
        /// Active settings Window. Closed on Stop(), can be null.
        /// </summary>
        private Window _deviceSettingsWindow;

		private WriteableBitmap _writeableBitmap;

        DepthImagePixel[] _depthPixels;
        byte[] _colorPixels;

		#endregion

		#region Interface implementation
		// Events
        public event EventHandler<BitmapSource> InputImageSourceChanged;
        public event EventHandler<DroneInputStatus> InputStatusChanged;
		public event EventHandler<bool> ControllerActivity;

        // Properties
        public MenuItem InputMenu
        {
            get;
            private set;
        }

        public DroneCommand ActiveDrone;

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

		#region Properties

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
			get { return _kinectSensor != null && _kinectSensor.Status != KinectStatus.Disconnected; }
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

		public bool DepthImageEnabled { get; set; }

		#endregion //Properties

		/// <summary>
        /// Kinect sensor input controller.
        /// Handles drone control through a Kinect sensor.
        /// </summary>
        public KinectSensorController(IActivityLogger parentLogger)
        {
            this._logger = new DefaultActivityLogger();
            _logger.AutoTags = ActivityLogger.RegisteredTags.FindOrCreate( "Kinect" );
            _logger.Output.BridgeTo( parentLogger );

            this.DepthImageEnabled = true;
            SetSkeletonSmoothingEnabled( false );

            int deviceCount = KinectSensor.KinectSensors.Count; // Blocking call (USB devices polling).

            TriggerButtons = new ObservableCollectionEx<ICaptionArea>();
            initTriggerZones( TRIGGER_BUTTON_WIDTH, TRIGGER_BUTTON_HEIGHT );

            this.InputMenu = PrepareInputMenuItem();
            this.InputStatusChanged += OnInputStatusChanged;

            _interactionClient = new InteractionClient();

            if ( deviceCount > 0 )
            {
                KinectSensor kinectSensor = KinectSensor.KinectSensors.Where( item => item.Status == KinectStatus.Connected ).FirstOrDefault();
                initializeKinectSensor( kinectSensor );
            }

            KinectSensor.KinectSensors.StatusChanged += sensors_StatusChanged;

			this._inputState = new InputState(_logger);

            this.InputUIControl = new KinectSensorControllerUI( _logger, this );
            ReleaseLeftHand.Interval = new TimeSpan( 0, 0, 2 );
            ReleaseLeftHand.Tick += new EventHandler( OnReleaseLeftHand );
            ReleaseRightHand.Interval = new TimeSpan( 0, 0, 2 );
            ReleaseRightHand.Tick += new EventHandler( OnReleaseRightHand );

        }
        private void OnReleaseLeftHand( object sender, EventArgs e )
        {
            _leftGrip = false;
            ReleaseLeftHand.Stop();
        }

        private void OnReleaseRightHand( object sender, EventArgs e )
        {
            _rightGrip = false;
            ReleaseRightHand.Stop();
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

			if( !_kinectSensor.SkeletonStream.IsEnabled && !_kinectSensor.DepthStream.IsEnabled )
			{
				_kinectSensor.AllFramesReady += sensor_AllFramesReady;
			}

            if ( _kinectSensor.SkeletonStream.IsEnabled )
			{
				_kinectSensor.SkeletonStream.Disable();
            }

            _totalSkeleton = new Skeleton[6];
			_userInfo = new UserInfo[6];

            if ( this._enableSmoothing )
            {
                System.Diagnostics.Debug.Assert( this._enableSmoothing == true && this._smoothingParam != null );
                _kinectSensor.SkeletonStream.Enable( _smoothingParam );
            }
            else
            {
                _kinectSensor.SkeletonStream.Enable();
            }


            if ( _kinectSensor.DepthStream.IsEnabled )
			{
				_kinectSensor.DepthStream.Disable();
            }

            _handsVisible = false;

            _kinectSensor.DepthStream.Enable( DEPTH_IMAGE_FORMAT );
			_kinectSensor.SkeletonStream.EnableTrackingInNearRange = true;

            _interactionStream = new InteractionStream( _kinectSensor, _interactionClient );
            _interactionStream.InteractionFrameReady += new EventHandler<InteractionFrameReadyEventArgs>( InteractiontStream_InteractionFrameReady );

            initializeSecurityTimer();

            initializePositionTrackerController();
            // Call Start(); from outside.
        }

		private void initializeSecurityTimer()
		{
            if ( _timerToLand != null )
            {
                _timerToLand.Stop();
            }
            
			_timerToLand = new DispatcherTimer();
			_timerToLand.Interval = new TimeSpan( 0, 0, 3 );
			_timerToLand.Tick += new EventHandler( timerToLand_Tick );
		}

        /// <summary>
        /// Initializes the isLeftArea tracker and its related trigger zones for the hands.
        /// </summary>
        private void initializePositionTrackerController()
        {
            _positionTrackerController = new PositionTrackerController();

            IPositionTracker<UserInfo> leftTracker = new LeftHandPositionTracker( _leftTriggerArea.DeadZoneCaptionsCollection.Values.ToList() );
            IPositionTracker<UserInfo> rightTracker = new RightHandPositionTracker( _rightTriggerArea.DeadZoneCaptionsCollection.Values.ToList() );

            this.PositionTrackerController.AttachPositionTracker( leftTracker );
            this.PositionTrackerController.AttachPositionTracker( rightTracker );
        }

        /// <summary>
        /// Prepare the right and left trigger zones.
        /// </summary>
        /// <param name="deadZoneWidth">Width of each button / side size of the zone</param>
        /// <param name="deadZoneHeight">Height of each button / thickness of the triggers</param>
        private void initTriggerZones( int buttonWidth, int buttonHeight )
        {
            int zoneWidth = DEPTH_FRAME_WIDTH / 2;
            int zoneHeight = DEPTH_FRAME_HEIGHT;

            // Create caption areas
            _leftTriggerArea = new TriggerArea( zoneWidth, zoneHeight, 0, 0, buttonWidth, buttonHeight, true);
            _rightTriggerArea = new TriggerArea( zoneWidth, zoneHeight, zoneWidth, 0, buttonWidth, buttonHeight, false);

            // Create a collection from both zones
                _leftTriggerArea.DeadZoneCaptionsCollection.Values.Union(
                        _rightTriggerArea.DeadZoneCaptionsCollection.Values
                ).ToList().ForEach((element) => TriggerButtons.Add(element));
			( (INotifyPropertyChanged)TriggerButtons ).PropertyChanged += ( x, y ) => ChangeCurrentInputState(x,y);
        }

		internal void ChangeCurrentInputState( object x, PropertyChangedEventArgs y )
		{
			bool[] inputs = new bool[2];
			for ( int i = 0; i < 2; i++)
			{
				inputs[i] = ( (ObservableCollectionEx<ICaptionArea>)x )[i].IsActive;
			}
			if ( _inputState.CheckInput( inputs ) )
			{
                MapInput();
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

            OnInputImageSourceChanged( null );

			_kinectSensor.AllFramesReady -= sensor_AllFramesReady;

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
					_kinectSensor.Start(); // Blocking call. May throw IOException on already used, or worse...!

					OnSensorChanged( _kinectSensor );

                }
                catch ( System.IO.IOException e )
                {
                    _logger.Error( "Kinect is already in use by another process" );
                    _logger.Error( e );
                }
                catch ( Exception e )
                {
                    _logger.Error( "Unexpected Kinect API error" );
                    _logger.Error( e );
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

            if ( this._deviceSettingsWindow != null )
            {
                this._deviceSettingsWindow.Close();
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

		public BitmapSource WriteToBitmap( DepthImageFrame frame )
		{
            // Mostly from: http://msdn.microsoft.com/en-us/library/jj131029.aspx

            if ( ( null == _depthPixels ) || ( _depthPixels.Length != frame.PixelDataLength ) )
			{
                this._depthPixels = new DepthImagePixel[_kinectSensor.DepthStream.FramePixelDataLength];
                this._colorPixels = new byte[_kinectSensor.DepthStream.FramePixelDataLength * sizeof( int )];
			}

            frame.CopyDepthImagePixelDataTo( _depthPixels );

			if( null == _writeableBitmap || _writeableBitmap.Format != PixelFormats.Bgra32 )
            {
                this._writeableBitmap = new WriteableBitmap(
                    _kinectSensor.DepthStream.FrameWidth,
                    _kinectSensor.DepthStream.FrameHeight,
                    96.0,
                    96.0,
                    PixelFormats.Bgr32,
                    null
                    );
			}

            // Get the min and max reliable depth for the current frame
            int minDepth = frame.MinDepth;
            int maxDepth = frame.MaxDepth;

            // Convert the depth to RGB
            int colorPixelIndex = 0;
            for ( int i = 0; i < this._depthPixels.Length; ++i )
            {
                // Get the depth for this pixel
                short depth = _depthPixels[i].Depth;

                // To convert to a byte, we're discarding the most-significant
                // rather than least-significant bits.
                // We're preserving detail, although the intensity will "wrap."
                // Values outside the reliable depth range are mapped to 0 (black).

                // Note: Using conditionals in this loop could degrade performance.
                // Consider using a lookup table instead when writing production code.
                // See the KinectDepthViewer class used by the KinectExplorer sample
                // for a lookup table example.
                byte intensity = (byte)( depth >= minDepth && depth <= maxDepth ? depth : 0 );

                // Write out blue byte
                this._colorPixels[colorPixelIndex++] = intensity;

                // Write out green byte
                this._colorPixels[colorPixelIndex++] = intensity;

                // Write out red byte                        
                this._colorPixels[colorPixelIndex++] = intensity;

                // We're outputting BGR, the last byte in the 32 bits is unused so skip it
                // If we were outputting BGRA, we would write alpha here.
                ++colorPixelIndex;
            }

            this._writeableBitmap.WritePixels(
                new Int32Rect( 0, 0, this._writeableBitmap.PixelWidth, this._writeableBitmap.PixelHeight ),
                this._colorPixels,
                this._writeableBitmap.PixelWidth * sizeof( int ),
            0 );

			return _writeableBitmap;
		}

		private void sensor_AllFramesReady( object sender, AllFramesReadyEventArgs e )
		{
			using( DepthImageFrame imageFrame = e.OpenDepthImageFrame() )
			{
				if( imageFrame == null )
				{
					return; // Clear frame
				}

				_interactionStream.ProcessDepth( imageFrame.GetRawPixelData(), imageFrame.Timestamp );

				_depthFrameIsReady = true; 
				if( this.DepthImageEnabled )
				{
					_depthBitmapSource = WriteToBitmap( imageFrame );
					OnInputImageSourceChanged( _depthBitmapSource );
				}

				imageFrame.Dispose();
			}

			using( SkeletonFrame skeletonFrame = e.OpenSkeletonFrame() )
			{
				if( skeletonFrame != null )
				{
					skeletonFrame.CopySkeletonDataTo( _totalSkeleton );
					_interactionStream.ProcessSkeleton( _totalSkeleton, _kinectSensor.AccelerometerGetCurrentReading(), skeletonFrame.Timestamp );
				}
			}
		}

        public static Point WindowPointFromHandPointer( InteractionHandPointer p )
        {
            double offset = 0.325;
            double min = 0;
            double max = 1.5;

            double pointX = p.X + offset;
            double pointY = p.Y + offset;

            if ( pointX < min ) pointX = min;
            else if ( pointX > max ) pointX = max;
            if ( pointY < min ) pointY = min;
            else if ( pointY > max ) pointY = max;

            float x = (float)( pointX / 1.5 * KinectSensorController.DEPTH_FRAME_WIDTH );
            float y = (float)( pointY / 1.5 * KinectSensorController.DEPTH_FRAME_HEIGHT );

            return new System.Windows.Point( x, y );
        }

		private void InteractiontStream_InteractionFrameReady( object sender, InteractionFrameReadyEventArgs e )
		{
			InteractionFrame iFrame = e.OpenInteractionFrame();
			if( iFrame == null ) return;
						
			iFrame.CopyInteractionDataTo( _userInfo );

			List<UserInfo> curUsers = _userInfo.Where( x => x.SkeletonTrackingId > 0 ).ToList<UserInfo>();

			if( curUsers.Count > 0 )
			{

                if ( !_skeletonFound )
                {
                    _skeletonFound = true;
                }
				UserInfo curUser = curUsers[0];

				_handsVisible = true;

				_positionTrackerController.NotifyPositionTrackers( curUser );

				if( HandsPointReady != null )
				{
					Point left = new System.Windows.Point( -1, -1 );
					Point right = new System.Windows.Point( -1, -1 );
					if ( curUser.HandPointers[0].IsActive
					|| curUser.HandPointers[1].IsActive )
					{
						left = WindowPointFromHandPointer( curUser.HandPointers[0] );
						right = WindowPointFromHandPointer( curUser.HandPointers[1] );
					}
					_leftHand = left;
					_rightHand = right;

					OnHandsPointReady( left, right );

					SafetyModeCheck( curUser );

					TestTakeOffCapabitility( curUser );

                    MapInput();
				}
			}
			else if( _handsVisible == true )
			{
				OnHandsPointReady( new System.Windows.Point( -1, -1 ), new System.Windows.Point( -1, -1 ) );
                _leftHand = new System.Windows.Point(-1, -1);
                _rightHand = new System.Windows.Point(-1, -1);
                MapInput();

				_handsVisible = false;
				if( _skeletonFound )
				{
					_skeletonFound = false;
                    SafetyModeCheck( null );
				}
				_leftGrip = false;
				_rightGrip = false;
			}
			else
			{
				if( _skeletonFound )
				{
					_skeletonFound = false;
				}
				_leftGrip = false;
				_rightGrip = false;
			}
		}

		private void TestTakeOffCapabitility( UserInfo curUser )
		{
			// HandEvent are a single occurence, so we must store the current state
			if( curUser.HandPointers[0].HandEventType == InteractionHandEventType.Grip )
			{
				_leftGrip = true;
                ReleaseLeftHand.Start();
			}
			else if( curUser.HandPointers[0].HandEventType == InteractionHandEventType.GripRelease )
			{
				_leftGrip = false;
			}
			if( curUser.HandPointers[1].HandEventType == InteractionHandEventType.Grip )
			{
                _rightGrip = true;
                ReleaseRightHand.Start();
			}
			else if( curUser.HandPointers[1].HandEventType == InteractionHandEventType.GripRelease )
			{
				_rightGrip = false;
			}


			if( curUser.HandPointers[0].HandEventType == InteractionHandEventType.Grip
				|| curUser.HandPointers[1].HandEventType == InteractionHandEventType.Grip )
			{
				_logger.Trace("Can Take Off : "+ _canTakeOff.ToString());
				if( !_canTakeOff && ( _leftGrip && _rightGrip ) )
				{
					_canTakeOff = true;
					if( ControlAcquired != null )
					{
						ControlAcquired( this, null );
					}
					_leftGrip = false;
					_rightGrip = false;
				}
				else if ( _canTakeOff && curUser.HandPointers[0].HandEventType == InteractionHandEventType.Grip )
				{
					SecurityModeChanged( this, 2 );
					_canTakeOff = false;
                    MapInput();
				}
				else if( _canTakeOff && curUser.HandPointers[1].HandEventType == InteractionHandEventType.Grip )
				{
					SecurityModeChanged( this, 3 );
					MapInput();
				}
			}
		}

		#region Security
		public void SafetyModeCheck( UserInfo curUser )
		{
			if( curUser != null & _skeletonFound && ( curUser.HandPointers[0].IsTracked
					&& curUser.HandPointers[1].IsTracked ) )
			{
				if( !curUser.HandPointers[0].IsActive
					&& !curUser.HandPointers[1].IsActive )
				{
					if( SecurityModeChanged != null && !_timerToLand.IsEnabled && !_safetyLandingtriggered )
					{
						SecurityModeChanged( this, 1 );
                        _operatorLost = true;
                        MapInput();
                        _logger.Warn( "Safety mode enabled due to hands no longer being tracked" );
					}

					if( !_timerToLand.IsEnabled && !_safetyLandingtriggered )
					{
						_timerToLand.Start();
                        _logger.Warn( "Safety mode : 3 seconds remaining before automatic landing" );
					}
				}
				else if( _timerToLand.IsEnabled &&
					 ( curUser.HandPointers[0].IsActive
					&& curUser.HandPointers[1].IsActive ) && !_safetyLandingtriggered == false )
				{
					_timerToLand.Stop();
					SecurityModeChanged( this, 0 );
					_operatorLost = false;
					_safetyLandingtriggered = false;
                    MapInput();
                    _logger.Warn( "Safety mode no longer required, restoring manual control" );
				}
				else if( ( curUser.HandPointers[0].IsInteractive
					&& curUser.HandPointers[1].IsInteractive )
					&& _safetyLandingtriggered == true )
				{
					_safetyLandingtriggered = false;
				}
			}
			else
			{
				if( !_timerToLand.IsEnabled && SecurityModeChanged != null )
				{
					SecurityModeChanged( this, 1 );
                    _operatorLost = true;
                    MapInput();
                    _logger.Warn( "Safety mode enabled due to hands no longer being tracked" );
				}

				if( !_timerToLand.IsEnabled )
				{
                    _timerToLand.Start();
                    _logger.Warn( "Safety mode : 3 seconds remaining before automatic landing" );
				}
			}
		}

		private void timerToLand_Tick( object sender, EventArgs e )
		{
			if( SecurityModeChanged != null )
			{
				SecurityModeChanged( this, 2 );
                _logger.Warn( "Security mode will now process to land the drone" );
				_timerToLand.Stop();
				_safetyLandingtriggered = true;
                MapInput();
			}
		}
		#endregion
        
        /// <summary>
        /// Fired every time any sensor on the system changes its status.
        /// </summary>
        private void sensors_StatusChanged( object sender, StatusChangedEventArgs e )
        {
            if ( e.Sensor != null )
            {
                _logger.Trace( "Status: " + e.Status.ToString() );
            }

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
			OnSensorChanged( e.Sensor );

            // Throw interface event
            OnInputStatusChanged( InputStatus );
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
            if ( this._deviceSettingsWindow != null )
            {
                this._deviceSettingsWindow.Activate();
            }
            else if ( this.InputStatus != DroneInputStatus.Ready )
            {
                return; // Command should be disabled anyway
            }
            else
            {
                // Prepare and open window
                this._deviceSettingsWindow = new KinectDeviceSettings( _logger, this );
                _deviceSettingsWindow.Closed += deviceSettingsWindow_Closed;
                _deviceSettingsWindow.Show();
            }

            e.Handled = true;
        }

        void deviceSettingsWindow_Closed( object sender, EventArgs e )
        {
            _deviceSettingsWindow = null;
        }

		private void OnInputImageSourceChanged( BitmapSource bitmapSource )
		{
			if( InputImageSourceChanged != null )
			{
				InputImageSourceChanged( this, bitmapSource );
			}
		}

		private void OnSensorChanged( KinectSensor sensor)
		{
			if( SensorChanged != null )
			{
				SensorChanged( this, Sensor );
			}
		}

		private void OnInputStatusChanged( DroneInputStatus droneInputStatus )
		{
			if( InputStatusChanged != null )
			{
				InputStatusChanged( this, this.InputStatus );
			}
		}

		private void OnHandsPointReady( Point left, Point right )
		{
			if( HandsPointReady != null )
			{
				HandsPointReady( this, new System.Windows.Point[2] { left, right } );
			}
		}

        public RideOnMotion.Inputs.InputState GetCurrentControlInput(RideOnMotion.Inputs.InputState _lastInputState)
        {

            // TODO test

            if (roll != _lastInputState.Roll || pitch != _lastInputState.Pitch || yaw != _lastInputState.Yaw || gaz != _lastInputState.Gaz || cameraSwap != _lastInputState.CameraSwap || takeOff != _lastInputState.TakeOff ||
                land != _lastInputState.Land || hover != _lastInputState.Hover || emergency != _lastInputState.Emergency || flatTrim != _lastInputState.FlatTrim || specialActionButton != _lastInputState.SpecialAction)
            {
                RideOnMotion.Inputs.InputState newInputState = new RideOnMotion.Inputs.InputState(roll, pitch, yaw, gaz, cameraSwap, takeOff, land, hover, emergency, flatTrim, specialActionButton);
                return newInputState;
            }
            else
            {
                return null;
            }
        }

        internal void MapInput()
        {
            roll = 0;
            pitch = 0;
            yaw = 0;
            gaz = 0;

            cameraSwap = false;
            takeOff = false;
            land = false;
            hover = false;
            emergency = false;

            flatTrim = false;
            specialActionButton = false;

            if (_handsVisible)
            {
                int offset;
                if (!InputState[0])
                {
                    offset = 0;
                    roll = HandsPositionX(_leftHand.X, offset);
                    pitch = HandsPositionY(_leftHand.Y, false);
                }
                if (!InputState[1])
                {
                    offset = - (DEPTH_FRAME_WIDTH / 2);
                    yaw = HandsPositionX(_rightHand.X, offset);
                    gaz = HandsPositionY(_rightHand.Y,true);
                }
            }
            if (_operatorLost && !_lastOperatorLost)
            {
                hover = true;
                _lastOperatorLost = true;
            }
            else if (!_operatorLost && _lastOperatorLost)
            {
                hover = true;
                _lastOperatorLost = false;
            }
			if ( ( _leftGrip && !_rightGrip && ActiveDrone.CanLand ) || _safetyLandingtriggered )
            {
                land = true;
            }
			if( _canTakeOff && _rightGrip && !_leftGrip && ActiveDrone.CanTakeoff )
            {
                takeOff = true;
			}
        }

        float HandsPositionX(double hand, int offset)
        {
            float xValue;
            if (hand != -1)
            {
                hand += offset;
            }
            if ((hand >= (DEPTH_FRAME_WIDTH / 4) - TRIGGER_BUTTON_WIDTH / 2 && hand <= (DEPTH_FRAME_WIDTH / 4) + TRIGGER_BUTTON_WIDTH / 2) || hand == -1)
            {
                xValue = 0;
            }
            else if (hand < (DEPTH_FRAME_WIDTH / 4) - TRIGGER_BUTTON_WIDTH / 2)
            {
                double transition = (double)((hand - (DEPTH_FRAME_WIDTH / 4 - TRIGGER_BUTTON_WIDTH / 2)) / (DEPTH_FRAME_WIDTH / 4 - TRIGGER_BUTTON_WIDTH / 2));
                if (transition < 0)
                {
                    xValue = (float)-(Math.Pow(-transition, 1.7) * 1.1);
                }
                else
                {
                    xValue = (float)(Math.Pow(transition, 1.7) * 1.1);
                }
                if (xValue < -1)
                {
                    xValue = -1f;
                }
                if (xValue > 1)
                {
                    xValue = 1f;
                }
            }
            else if (hand > (DEPTH_FRAME_WIDTH / 4) + TRIGGER_BUTTON_WIDTH / 2)
            {
                xValue = (float)(Math.Pow((double)((hand - (DEPTH_FRAME_WIDTH / 4 + TRIGGER_BUTTON_WIDTH / 2)) / (DEPTH_FRAME_WIDTH / 4 - TRIGGER_BUTTON_WIDTH / 2)), 1.7) * 1.1);
                if (xValue > 1)
                {
                    xValue = 1f;
                }
            }
            else
            {
                xValue = 0;
            }
            return xValue;
        }

        float HandsPositionY(double hand, bool up)
        {
            float yValue;
            if ((hand >= (DEPTH_FRAME_HEIGHT / 2) - TRIGGER_BUTTON_HEIGHT / 2 && hand <= (DEPTH_FRAME_HEIGHT / 2) + TRIGGER_BUTTON_HEIGHT / 2) || hand == -1)
            {
                yValue = 0;
            }
            else if (hand < (DEPTH_FRAME_HEIGHT / 2) - TRIGGER_BUTTON_HEIGHT / 2)
            {
                double transition = (double)((hand - (DEPTH_FRAME_HEIGHT / 2 - TRIGGER_BUTTON_HEIGHT / 2)) / (DEPTH_FRAME_HEIGHT / 2 - TRIGGER_BUTTON_HEIGHT / 2));
                if (transition < 0)
                {
                    yValue = (float)(Math.Pow(-transition, 1.7) * 1.1);
                }
                else
                {
                    yValue = (float)(Math.Pow(transition, 1.7) * 1.1);
                }
                if (yValue < -1)
                {
                    yValue = -1f;
                }
                if (yValue > 1)
                {
                    yValue = 1f;
                }
                if (!up)
                {
                    yValue = -yValue;
                }
            }
            else if (hand > (DEPTH_FRAME_HEIGHT / 2) + TRIGGER_BUTTON_HEIGHT / 2)
            {
                yValue = (float)-(Math.Pow((double)((hand - (DEPTH_FRAME_HEIGHT / 2 + TRIGGER_BUTTON_HEIGHT / 2)) / (DEPTH_FRAME_HEIGHT / 2 - TRIGGER_BUTTON_HEIGHT / 2)), 1.7) * 1.1);
                if (yValue < -1)
                {
                    yValue = -1f;
                }
                if (yValue > 1)
                {
                    yValue = 1f;
                }
                if (!up)
                {
                    yValue = -yValue;
                }
            }
            else
            {
                yValue = 0;
            }
            return yValue;
        }

	}

	public class InteractionClient : IInteractionClient
	{
		public InteractionClient()
		{
		}

		public InteractionInfo GetInteractionInfoAtLocation( int skeletonTrackingId, InteractionHandType handType, double x, double y )
		{
			InteractionInfo interactionInfo = new InteractionInfo();

			return interactionInfo;
		}
	}
}
