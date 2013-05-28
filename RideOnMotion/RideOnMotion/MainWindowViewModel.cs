using System;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Input;
using RideOnMotion.Inputs.Keyboard;
using System.Text;
using System.Windows.Threading;

namespace RideOnMotion.UI
{
    /// <summary>
    /// View model for main window. Contains displayed properties.
    /// </summary>
    class MainWindowViewModel : IViewModel, INotifyPropertyChanged
    {
        private readonly int MAX_LOG_ENTRIES = 50; // Maximum number of log entries in the collection
        private readonly float DRONE_PITCH_YAW_SPEED = 0.15f;
        private readonly float DRONE_ROTATION_SPEED = 0.25f;
        private readonly float DRONE_ELEVATION_SPEED = 0.25f;

        /// <summary>
        /// Kinect model : Handles data in and out of the Kinect
        /// </summary>
        private IDroneInputController _inputController;
        private KeyboardController _keyboardController;
        private DroneInitializer _droneInit;

        #region Values
        private ImageSource _droneImageSource;
        private ImageSource _inputImageSource;
        private Control _inputControlUI;
        private MenuItem _inputMenu;

        private String _droneNetworkStatusText;
        private bool _droneConnectionStatus;
        private ARDrone.Control.Data.DroneData _lastDroneData;

        private List<Type> InputTypes { get; set; }

		private string _inputStatusInfo = String.Empty;
        
        private ObservableCollection<String> _logStrings;

		internal bool Konami = false;

		private System.Windows.Media.MediaPlayer mp1 = new System.Windows.Media.MediaPlayer();
		private System.Windows.Media.MediaPlayer mp2 = new System.Windows.Media.MediaPlayer();
		private System.Windows.Media.MediaPlayer mp3 = new System.Windows.Media.MediaPlayer();
		private System.Windows.Media.MediaPlayer mp4 = new System.Windows.Media.MediaPlayer();

        internal event EventHandler<MenuItem> InputMenuChanged;

        private delegate void AddLogStringDelegate( String s );

        #endregion Values

        #region GettersSetters

        public ImageSource InputImageSource
        {
            get
            {
                return this._inputImageSource;
            }

            set
            {
                if ( this._inputImageSource != value )
                {
                    this._inputImageSource = value;
                    this.OnNotifyPropertyChange( "InputImageSource" );
					if ( IsActive == true  && Konami == true)
					{
						TimeSpan timeZero =  new TimeSpan( 0 );
						if ( mp1.Position == mp1.NaturalDuration.TimeSpan || mp1.Position == timeZero)
						{
						mp1.Position = new TimeSpan( 0 );
						mp1.Play();
						}
						else if ( mp2.Position == mp2.NaturalDuration.TimeSpan || mp2.Position == timeZero )
						{
							mp2.Position = new TimeSpan( 0 );
							mp2.Play();
						}
						else if ( mp3.Position == mp3.NaturalDuration.TimeSpan || mp3.Position == timeZero )
						{
							mp3.Position = new TimeSpan( 0 );
							mp3.Play();
						}
						else if ( mp4.Position == mp4.NaturalDuration.TimeSpan || mp4.Position == timeZero )
						{
							mp4.Position = new TimeSpan( 0 );
							mp4.Play();
						}
					}
                }
            }
        }

        public ImageSource DroneImageSource
        {
            get
            {
                return this._droneImageSource;
            }

            set
            {
                if ( this._droneImageSource != value )
                {
                    this._droneImageSource = value;
                    this.OnNotifyPropertyChange( "DroneImageSource" );
                }
            }
        }

        public ObservableCollection<String> LogData
        {
            get
            {
                return this._logStrings;
            }

            set
            {
                if ( this._logStrings != value )
                {
                    this._logStrings = value;
                    this.OnNotifyPropertyChange( "LogData" );
                }
            }
        }

        public String DroneStatus
        {
            get
            {
                if ( this._droneConnectionStatus )
                {
                    return "AR Drone: Connected";
                }
                else
                {
                    return "AR Drone: Disconnected";
                }
            }
        }

        public String DroneStatusInfo
        {
            get
            {
                if ( this._droneConnectionStatus )
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append( "AR Drone: Connected\n" );
                    if ( this._droneNetworkStatusText != null )
                    {
                        sb.Append( this._droneNetworkStatusText );
                        sb.Append( '\n' );
                    }
                    if ( this._lastDroneData != null )
                    {
                        sb.Append( "Battery: " );
                        sb.Append( this._lastDroneData.BatteryLevel );
                        sb.Append( '\n' );
                        sb.Append( "Altitude: " );
                        sb.Append( this._lastDroneData.Altitude );
                        sb.Append( '\n' );

                        sb.Append( "φ: " );
                        sb.Append( this._lastDroneData.Phi );
                        sb.Append( '\n' );
                        sb.Append( "ψ: " );
                        sb.Append( this._lastDroneData.Psi );
                        sb.Append( '\n' );
                        sb.Append( "θ: " );
                        sb.Append( this._lastDroneData.Theta );
                        sb.Append( '\n' );

                        sb.Append( "vX: " );
                        sb.Append( this._lastDroneData.VX );
                        sb.Append( '\n' );
                        sb.Append( "vY: " );
                        sb.Append( this._lastDroneData.VY );
                        sb.Append( '\n' );
                        sb.Append( "vZ: " );
                        sb.Append( this._lastDroneData.VZ );
                        sb.Append( '\n' );
                    }

                    return sb.ToString();
                }
                else
                {
                    return "AR Drone: Disconnected"; 
                }
            }
        }

        public String InputStatusInfo
        {
            get
            {
                return _inputController.Name + ": " + _inputStatusInfo;
            }
        }

        public Control InputControl
        {
            get
            {
                return _inputControlUI;
            }
        }

        public MenuItem InputMenu
        {
            get
            {
                return _inputMenu;
            }
        }

		public bool IsActive
		{
			get
			{
				return _inputController.IsActive;
			}
		}

        #endregion GettersSetters

        #region INotifyPropertyChanged utilities
        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Called when [notify property change].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public void OnNotifyPropertyChange( string propertyName )
        {
            if ( this.PropertyChanged != null )
            {
                this.PropertyChanged.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }

        #endregion INotifyPropertyChanged utilities

        #region Contructor/initializers/event handlers/methods
        /// <summary>
        /// Initializes the ViewModel with the given IDroneInputController.
        /// </summary>
        public MainWindowViewModel()
        {
            InputTypes = new List<Type>();
			InputTypes.Add( typeof( RideOnMotion.Inputs.Kinect.KinectSensorController ) );

            _logStrings = new ObservableCollection<string>();

			loadInputType( InputTypes[0] );

			mp1.Open( new Uri( "..\\..\\Resources\\Quack.wav", UriKind.Relative ) );
			mp2.Open( new Uri( "..\\..\\Resources\\Quack2.wav", UriKind.Relative ) );
			mp3.Open( new Uri( "..\\..\\Resources\\Quack3.wav", UriKind.Relative ) );
			mp4.Open( new Uri( "..\\..\\Resources\\Quack4.mp3", UriKind.Relative ) );

            initializeBindings();

            _droneInit = new DroneInitializer();
            _droneInit.StartDrone();

			_droneInit.DroneCommand.EnterHoverMode();

            // Keyboard controller is specially handled.
            _keyboardController = new KeyboardController();
            _keyboardController.ActiveDrone = _droneInit.DroneCommand;

            // Bind front drone camera
            _droneInit.DroneFrameReady += OnDroneFrameReady;
            _droneInit.NetworkConnectionStateChanged += OnNetworkConnectionStateChanged;
            _droneInit.ConnectionStateChanged += OnConnectionStateChanged;
            _droneInit.DroneDataReady += OnDroneDataReady;
        }

        void OnDroneDataReady( object sender, DroneDataReadyEventArgs e )
        {
            this._lastDroneData = e.Data;
            this.OnNotifyPropertyChange( "DroneStatusInfo" );
        }

        private void OnNetworkConnectionStateChanged( object sender, string e )
        {
            this._droneNetworkStatusText = e;
            this.OnNotifyPropertyChange( "DroneStatusInfo" );
        }

        private void OnConnectionStateChanged( object sender, bool e )
        {
            this._droneConnectionStatus = e;
            this.OnNotifyPropertyChange( "DroneStatus" );
            this.OnNotifyPropertyChange( "DroneStatusInfo" );
        }

        void OnDroneFrameReady( object sender, DroneFrameReadyEventArgs e )
        {
            this.DroneImageSource = e.Frame;
        }

        /// <summary>
        /// Creates the event bindings with the model.
        /// </summary>
        private void initializeBindings()
        {
            Logger.Instance.NewLogStringReady += OnLogStringReceived;
        }

        private void OnInputStatusChanged( object sender, DroneInputStatus e )
        {
            _inputStatusInfo = _inputController.InputStatusString;
            this.OnNotifyPropertyChange( "InputStatusInfo" );
        }

        private void OnInputBitmapSourceChanged( object sender, BitmapSource s )
        {
            InputImageSource = s;
        }

        private void OnLogStringReceived( object sender, String e )
        {
            Invoke( () =>
            {
                if ( _logStrings.Count >= MAX_LOG_ENTRIES )
                {
                    _logStrings.RemoveAt( 0 );
                }
                _logStrings.Add( e );
            } );
        }

        private void loadInputType(Type t)
        {
            if (t != null && t.GetInterface( "RideOnMotion.IDroneInputController", true ) != null )
            {

                IDroneInputController controller = (IDroneInputController) Activator.CreateInstance(t);

                clearInputController();

                _inputController = controller;

                bindWithInputController();

                _inputControlUI = _inputController.InputUIControl;
                this.OnNotifyPropertyChange( "InputControl" );

                _inputMenu = _inputController.InputMenu;
                if ( InputMenuChanged != null )
                {
                    InputMenuChanged( this, _inputController.InputMenu );
                }

                _inputController.Start();
            }
            else
            {
                // Can't load, type doesn't implement interface
            }
        }

        private static void Invoke( Action action )
        {
            Dispatcher dispatchObject = System.Windows.Application.Current.Dispatcher;
            if ( dispatchObject == null || dispatchObject.CheckAccess() )
            {
                action();
            }
            else
            {
                dispatchObject.Invoke( action );
            }
        }

        private void clearInputController()
        {
            if ( this._inputController != null )
            {
                _inputController.InputImageSourceChanged -= OnInputBitmapSourceChanged;
                _inputController.InputStatusChanged -= OnInputStatusChanged;
				_inputController.ControllerActivity -= OnControllerActivity;
				_inputController.InputsStateChanged -= OnInputsStateChanged;
                _inputStatusInfo = "";
                this.OnNotifyPropertyChange( "InputStatusInfo" );
                this._inputController = null;
            }
        }

        private void bindWithInputController()
        {
            // Bind depth image changes
            _inputController.InputImageSourceChanged += OnInputBitmapSourceChanged;

            // Bind sensor status and set once
            _inputController.InputStatusChanged += OnInputStatusChanged;
			_inputStatusInfo = _inputController.InputStatusString;

			// Bind activity
			_inputController.ControllerActivity += OnControllerActivity;
			_inputController.InputsStateChanged += OnInputsStateChanged;
			_inputController.SecurityModeNeeded += OnSecurityModeNeeded;
        }

		private void OnSecurityModeNeeded( object sender, int e )
		{
			throw new NotImplementedException();
		}

		void OnInputsStateChanged( object sender, bool[] e )
		{
			DroneCommandProcessing( e );
		}
		/// <summary>
		/// Control Navigation of the drone
		/// </summary>
		/// <param name="CurrentInputState">Must contains 8 bool value for pitch[0-1], roll[2-3], gaz[4-5], yaw[6-7] (for the AR Drone) </param>
		public void DroneCommandProcessing( bool[] CurrentInputState )
		{

			float roll = 0; // = CurrentInputState[2-3];
			float pitch = 0; // = CurrentInputState[0-1];
			float yaw = 0; // = CurrentInputState[6-7];
			float gaz = 0; // = CurrentInputState[4-5];

            if ( CurrentInputState[2] ) { roll = -DRONE_PITCH_YAW_SPEED; } else if ( CurrentInputState[3] ) { roll = DRONE_PITCH_YAW_SPEED; }
            if ( CurrentInputState[0] ) { pitch = -DRONE_PITCH_YAW_SPEED; } else if ( CurrentInputState[1] ) { pitch = DRONE_PITCH_YAW_SPEED; }
            if ( CurrentInputState[6] ) { yaw = -DRONE_ROTATION_SPEED; } else if ( CurrentInputState[7] ) { yaw = DRONE_ROTATION_SPEED; }
            if ( CurrentInputState[4] ) { gaz = DRONE_ELEVATION_SPEED; } else if ( CurrentInputState[5] ) { gaz = -DRONE_ELEVATION_SPEED; }

			_droneInit.DroneCommand.Navigate( roll, pitch, yaw, gaz );

		}

		public void OnControllerActivity(object sender, bool e)
		{
			this.OnNotifyPropertyChange( "IsActive" );
		}

        internal void Stop()
        {
            this._inputController.Stop();
            this._droneInit.EndDrone();
        }

        #endregion Contructor/initializers/event handlers

		internal void OnPreviewKeyDown( KeyEventArgs e )
		{
			if ( this._keyboardController != null )
			{
				this._keyboardController.ProcessKeyDown( e );
			}
		}
		internal void OnPreviewKeyUp( KeyEventArgs e )
		{
			if ( this._keyboardController != null )
			{
				this._keyboardController.ProcessKeyUp( e );
			}
		}
    }


}
