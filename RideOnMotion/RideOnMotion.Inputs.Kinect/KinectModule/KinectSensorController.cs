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
using Microsoft.Kinect.Toolkit.Interaction;
using System.Windows.Threading;
using System.Collections.Generic;

namespace RideOnMotion.Inputs.Kinect
{
    /// <summary>
    /// Main controller class for drone input through a Kinect sensor device.
    /// </summary>
    public class KinectSensorController : IDroneInputController
	{
		#region declarations
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
        public static readonly int TRIGGER_BUTTON_WIDTH = 300;

        /// <summary>
        /// Default trigger zone height (button thickness)
        /// </summary>
        public static readonly int TRIGGER_BUTTON_HEIGHT = 100;

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
		/// </summary>
		public event EventHandler<int> SecurityModeNeeded;

		public event EventHandler CanTakeOffMotherFucker;

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

		/// <summary>
		/// Manage the grip and grip release
		/// </summary>
		private bool _leftGrip = false;
		private bool _rightGrip = false;

        /// <summary>
        /// Active settings Window. Closed on Stop(), can be null.
        /// </summary>
        private Window _deviceSettingsWindow;

		public bool DepthImageEnabled { get; set; }
		#endregion

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
				InteractionClient interactionClient = new InteractionClient();
				_interactionStream = new InteractionStream( kinectSensor, interactionClient );
				_interactionStream.InteractionFrameReady += new EventHandler<InteractionFrameReadyEventArgs>( InteractiontStream_InteractionFrameReady );
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

            IPositionTracker<UserInfo> leftTracker = new LeftHandPositionTracker( _leftTriggerArea.TriggerCaptionsCollection.Values.ToList() );
            IPositionTracker<UserInfo> rightTracker = new RightHandPositionTracker( _rightTriggerArea.TriggerCaptionsCollection.Values.ToList() );

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
            _leftTriggerArea = new TriggerArea( zoneWidth, zoneHeight, 0, 0, buttonWidth, buttonHeight, true);
            _rightTriggerArea = new TriggerArea( zoneWidth, zoneHeight, zoneWidth, 0, buttonWidth, buttonHeight, false);

            // Create a collection from both zones
                _leftTriggerArea.TriggerCaptionsCollection.Values.Union(
                        _rightTriggerArea.TriggerCaptionsCollection.Values
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
                    Logger.Instance.NewEntry( CKLogLevel.Fatal, CKTraitTags.Kinect, "Kinect is already in use by another process. Error:" );
                    Logger.Instance.NewEntry( CKLogLevel.Fatal, CKTraitTags.Kinect, e.Message );
                }
                catch ( Exception e )
                {
                    Logger.Instance.NewEntry( CKLogLevel.Fatal, CKTraitTags.Kinect, "Unexpected Kinect API error:" );
                    Logger.Instance.NewEntry( CKLogLevel.Fatal, CKTraitTags.Kinect, e.Message );
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

		private void sensor_AllFramesReady( object sender, AllFramesReadyEventArgs e )
		{
			short[] depthPix;
			using( DepthImageFrame imageFrame = e.OpenDepthImageFrame() )
			{
				if( imageFrame == null )
				{
					return; // Clear frame
				}

				depthPix = new short[imageFrame.PixelDataLength];

				imageFrame.CopyPixelDataTo( depthPix );

				_interactionStream.ProcessDepth( imageFrame.GetRawPixelData(), imageFrame.Timestamp );

				_depthFrameIsReady = true; 
				if( this.DepthImageEnabled )
				{
					_depthBitmapSource = DepthToBitmapSource( imageFrame );
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
				UserInfo curUser = curUsers[0];

				_handsVisible = true;

				_positionTrackerController.NotifyPositionTrackers( curUser );

				if( HandsPointReady != null )
                {
                    Point left = WindowPointFromHandPointer( curUser.HandPointers[0] );
                    Point right = WindowPointFromHandPointer( curUser.HandPointers[1] );

					HandsPointReady( this, new System.Windows.Point[2] { left, right } );

					SafetyModeCheck( curUser );

					TestTakeOffCapabitility( curUser );
					
                    /*// TODO - Merge pending
							if( !_skeletonFound )
							{
								_skeletonFound = true;
							}
							if( _timerToLand.IsEnabled )
							{
								_timerToLand.Stop();
							}
							SecurityModeNeeded( this, 0 );
                     */
				}
			}
			else if( _handsVisible == true )
			{
				if( HandsPointReady != null )
				{
					HandsPointReady( this, new System.Windows.Point[2] { new System.Windows.Point( -1, -1 ), new System.Windows.Point( -1, -1 ) } );
				}
				_handsVisible = false;
				if( _skeletonFound )
				{
					_skeletonFound = false;
				}
				_handsVisible = false;
			}
			else
			{
				if( _skeletonFound )
				{
					_skeletonFound = false;
				}
			}
		}

		private void TestTakeOffCapabitility( UserInfo curUser )
		{
			// HandEvent are a single occurence, so we must store the current state
			if( curUser.HandPointers[0].HandEventType == InteractionHandEventType.Grip )
			{
				_leftGrip = true;
			}
			else if( curUser.HandPointers[0].HandEventType == InteractionHandEventType.GripRelease )
			{
				_leftGrip = false;
			}
			if( curUser.HandPointers[1].HandEventType == InteractionHandEventType.Grip )
			{
				_rightGrip = true;
			}
			else if( curUser.HandPointers[1].HandEventType == InteractionHandEventType.GripRelease )
			{
				_rightGrip = false;
			}


			if( curUser.HandPointers[0].HandEventType == InteractionHandEventType.Grip
				|| curUser.HandPointers[1].HandEventType == InteractionHandEventType.Grip )
			{
				if( !_canTakeOff && ( _leftGrip && _rightGrip ) )
				{
					_canTakeOff = true;
					if( CanTakeOffMotherFucker != null )
					{
						CanTakeOffMotherFucker( this, null );
					}
				}
				else if( curUser.HandPointers[0].HandEventType == InteractionHandEventType.Grip )
				{
					SecurityModeNeeded( this, 2 );
				}
				else if( _canTakeOff && curUser.HandPointers[1].HandEventType == InteractionHandEventType.Grip )
				{
					SecurityModeNeeded( this, 3 );
					_canTakeOff = false;
				}
			}
		}

		#region Security
		public void SafetyModeCheck( UserInfo curUser )
		{
			if( _skeletonFound && ( curUser.HandPointers[0].IsTracked
					&& curUser.HandPointers[1].IsTracked ) )
			{
				if( !curUser.HandPointers[0].IsActive
					&& !curUser.HandPointers[1].IsActive )
				{
					if( SecurityModeNeeded != null && _timerToLand.IsEnabled == false && _safetyLandingtriggered == false )
					{
						SecurityModeNeeded( this, 1 );
						Logger.Instance.NewEntry( CKLogLevel.Warn, CKTraitTags.Kinect,
							"Safety mode enabled due to hands no longer being tracked" );
					}

					if( _timerToLand.IsEnabled == false && _safetyLandingtriggered == false )
					{
						_timerToLand.Start();
						Logger.Instance.NewEntry( CKLogLevel.Warn, CKTraitTags.Kinect,
							"Safety mode : 3 seconds remaining before automatic landing" );
					}
				}
				else if( _timerToLand.IsEnabled == true &&
					 ( curUser.HandPointers[0].IsActive
					&& curUser.HandPointers[1].IsActive ) && _safetyLandingtriggered == false )
				{
					_timerToLand.Stop();
					SecurityModeNeeded( this, 0 );
					Logger.Instance.NewEntry( CKLogLevel.Warn, CKTraitTags.Kinect,
						"Safety mode no longer required, restoring manual control" );
					_safetyLandingtriggered = false;
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
				if( _timerToLand.IsEnabled == false && SecurityModeNeeded != null )
				{
					SecurityModeNeeded( this, 1 );
					Logger.Instance.NewEntry( CKLogLevel.Warn, CKTraitTags.Kinect,
						"Safety mode enabled due to hands no longer being tracked" );
				}

				if( _timerToLand.IsEnabled == false )
				{
					_timerToLand.Start();
					Logger.Instance.NewEntry( CKLogLevel.Warn, CKTraitTags.Kinect,
						"Safety mode : 3 seconds remaining before automatic landing" );
				}
			}
		}

		private void timerToLand_Tick( object sender, EventArgs e )
		{
			if( SecurityModeNeeded != null )
			{
				SecurityModeNeeded( this, 2 );
				Logger.Instance.NewEntry( CKLogLevel.Warn, CKTraitTags.Kinect,
					"Security mode will now process to land the drone" );
				_timerToLand.Stop();
				_safetyLandingtriggered = true;
			}
		}
		#endregion
        
        /// <summary>
        /// Fired every time any sensor on the system changes its status.
        /// </summary>
        private void sensors_StatusChanged( object sender, StatusChangedEventArgs e )
        {
            Logger.Instance.NewEntry( CKLogLevel.Trace, CKTraitTags.Kinect, "Status changed to : " + e.Status.ToString() );
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
                this._deviceSettingsWindow = new KinectDeviceSettings( this );
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

		private void OnSecurityModeNeeded( int arg )
		{
			if( SecurityModeNeeded != null )
			{
				SecurityModeNeeded( this, arg );
			}
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
