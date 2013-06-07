using RideOnMotion.Inputs.Keyboard;
using RideOnMotion.Inputs.Xbox360Gamepad;
using RideOnMotion.UI.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using RideOnMotion.Inputs;

namespace RideOnMotion.UI
{
    /// <summary>
    /// View model for main window. Contains displayed properties.
    /// </summary>
    class MainWindowViewModel : IViewModel, INotifyPropertyChanged
    {
        private readonly int MAX_LOG_ENTRIES = 50; // Maximum number of log entries in the collection

        private static readonly float DEFAULT_MAXIMUM_DRONE_TRANSLATION_SPEED = 0.15f;
        private static readonly float DEFAULT_MAXIMUM_DRONE_ROTATION_SPEED = 0.25f;
        private static readonly float DEFAULT_MAXIMUM_DRONE_ELEVATION_SPEED = 0.25f;
        public static ARDrone.Control.DroneConfig DefaultDroneConfig = new ARDrone.Control.DroneConfig()
        {
            DroneIpAddress = Settings.Default.DroneIPAddress,
            StandardOwnIpAddress = Settings.Default.ClientIPAddress,
            DroneNetworkIdentifierStart = Settings.Default.DroneSSID,
            NavigationPort = 5554,
            VideoPort = 5555,
            CommandPort = 5556,
            ControlInfoPort = 5559,
            UseSpecificFirmwareVersion = false,
            FirmwareVersion = ARDrone.Control.DroneConfig.DefaultSupportedFirmwareVersion,
            TimeoutValue = 500
        };

        /// <summary>
        /// Kinect model : Handles data in and out of the Kinect
        /// </summary>
        private IDroneInputController _inputController;
        private KeyboardController _keyboardController;
		private Xbox360GamepadController _Xbox360Gamepad;
        private DroneInitializer _droneInit;

        private float _droneTranslationSpeed = DEFAULT_MAXIMUM_DRONE_TRANSLATION_SPEED;
        private float _droneRotationSpeed = DEFAULT_MAXIMUM_DRONE_ROTATION_SPEED;
        private float _droneElevationSpeed = DEFAULT_MAXIMUM_DRONE_ELEVATION_SPEED;

        #region Values
        private ImageSource _droneImageSource;
        private ImageSource _inputImageSource;
        private Control _inputControlUI;
        private MenuItem _inputMenu;

        private Window _droneSettingsWindow;

        private String _droneNetworkStatusText;
        private bool _droneConnectionStatus;
        private ARDrone.Control.DroneConfig _currentDroneConfig = DefaultDroneConfig;
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

		InputState _lastKeyboardInput;
		InputState _lastGamepadInput;

		SendDroneCommand _sendDroneCommand;

        #endregion Values

        #region GettersSetters

        public ICommand OpenDroneSettingsCommand
        {
            get;
            internal set;
        }

        public ICommand ReconnectDroneCommand
        {
            get;
            internal set;
        }

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

        public Visibility InputControlVisibility
        {
            get
            {
                if ( this._inputController.InputStatus == DroneInputStatus.Ready )
                {
                    return Visibility.Visible;
                } else {
                    return Visibility.Collapsed;
                }
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

		DispatcherTimer DroneOrderTimer = new DispatcherTimer();
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
            CreateDroneCommands();

            InputTypes = new List<Type>();
			InputTypes.Add( typeof( RideOnMotion.Inputs.Kinect.KinectSensorController ) );

            _logStrings = new ObservableCollection<string>();

			loadInputType( InputTypes[0] );

			//mp1.Open( new Uri( "..\\..\\Resources\\Quack.wav", UriKind.Relative ) );
			//mp2.Open( new Uri( "..\\..\\Resources\\Quack2.wav", UriKind.Relative ) );
			//mp3.Open( new Uri( "..\\..\\Resources\\Quack3.wav", UriKind.Relative ) );
			//mp4.Open( new Uri( "..\\..\\Resources\\Quack4.mp3", UriKind.Relative ) );

            initializeBindings();

			_lastKeyboardInput = new InputState();
			_lastGamepadInput = new InputState();
            _keyboardController = new KeyboardController();
            ConnectDrone(this._currentDroneConfig); // At this point, should be default config.
			_Xbox360Gamepad = new Xbox360GamepadController();
			_Xbox360Gamepad.ActiveDrone = _droneInit.DroneCommand;
			_Xbox360Gamepad.Start();
			DroneOrderTimer.Interval = new TimeSpan( 0, 0, 0,0,30 );
			DroneOrderTimer.Tick += new EventHandler( OrderTheMotherfuckingDrone );
			DroneOrderTimer.Start();
			_sendDroneCommand = new SendDroneCommand();

        }

		private void OrderTheMotherfuckingDrone( object sender, EventArgs e )
		{
			InputState newKeyboardInput = _keyboardController.GetCurrentControlInput(_lastKeyboardInput);
			InputState newGamepadInput = _Xbox360Gamepad.GetCurrentControlInput( _lastGamepadInput );
			if ( newGamepadInput != null || newKeyboardInput != null)
			{
				if ( newKeyboardInput != null )
				{
					_lastKeyboardInput = newKeyboardInput;
				}
				if ( newGamepadInput != null )
				{
					_lastGamepadInput = newGamepadInput;
				}
				InputState MixedInput = SendDroneCommand.MixInput( _lastKeyboardInput, _lastGamepadInput );
				if ( MixedInput != null )
				{
					_sendDroneCommand.Process( MixedInput );
				}
			}
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
            this.OnNotifyPropertyChange( "InputControlVisibility" );
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
			switch( e )
			{
				case 0:
					_droneInit.DroneCommand.LeaveHoverMode();
					break;
				case 1:
					_droneInit.DroneCommand.EnterHoverMode();
					break;
				case 2:
					_droneInit.DroneCommand.Land();
					break;
                case 3:
                    _droneInit.DroneCommand.Takeoff();
                    break;
				default:
					_droneInit.DroneCommand.Land();
					break;
			}
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

            if ( CurrentInputState[2] ) { roll = -_droneTranslationSpeed; } else if ( CurrentInputState[3] ) { roll = _droneTranslationSpeed; }
            if ( CurrentInputState[0] ) { pitch = -_droneTranslationSpeed; } else if ( CurrentInputState[1] ) { pitch = _droneTranslationSpeed; }
            if ( CurrentInputState[6] ) { yaw = -_droneRotationSpeed; } else if ( CurrentInputState[7] ) { yaw = _droneRotationSpeed; }
            if ( CurrentInputState[4] ) { gaz = _droneElevationSpeed; } else if ( CurrentInputState[5] ) { gaz = -_droneElevationSpeed; }

			_droneInit.DroneCommand.Navigate( roll, pitch, yaw, gaz );

		}

		public void OnControllerActivity(object sender, bool e)
		{
			this.OnNotifyPropertyChange( "IsActive" );
		}

        internal void Stop()
        {
            if ( _droneSettingsWindow != null )
            {
                _droneSettingsWindow.Close();
            }
            this._inputController.Stop();
			this._Xbox360Gamepad.Stop();

            DisconnectDrone(this._droneInit);
        }

        private void ConnectDrone( ARDrone.Control.DroneConfig config )
        {
            _droneInit = new DroneInitializer( config );

            _droneInit.NetworkConnectionStateChanged += OnNetworkConnectionStateChanged;
            _droneInit.ConnectionStateChanged += OnConnectionStateChanged;
            _droneInit.DroneDataReady += OnDroneDataReady;
            // Bind front drone camera
            _droneInit.DroneFrameReady += OnDroneFrameReady;

            // Keyboard controller is specially handled.
            _keyboardController.ActiveDrone = _droneInit.DroneCommand;

            _droneInit.StartDrone();
            _droneInit.DroneCommand.EnterHoverMode();
        }

        private void DisconnectDrone( DroneInitializer init )
        {

            init.NetworkConnectionStateChanged -= OnNetworkConnectionStateChanged;
            init.ConnectionStateChanged -= OnConnectionStateChanged;
            init.DroneDataReady -= OnDroneDataReady;
            // Bind front drone camera
            init.DroneFrameReady -= OnDroneFrameReady;

            // Keyboard controller is specially handled.
            _keyboardController.ActiveDrone = init.DroneCommand;

            init.EndDrone();
        }

        private void ReconnectDrone()
        {
            DisconnectDrone( this._droneInit );
            ConnectDrone( this._currentDroneConfig );
        }

        /// <summary>
        /// Sets new maximum drone speeds the drone can reach when moving.
        /// </summary>
        /// <param name="translationSpeed">(Between 0 and 1) Speed to move on the pitch and yaw axis. 0: No change.</param>
        /// <param name="rotationSpeed">(Between 0 and 1) Speed to move on the roll axis. 0: No change.</param>
        /// <param name="elevationSpeed">(Between 0 and 1) Speed to raise or lower at. 0: No change.</param>
        internal void SetDroneSpeeds( float translationSpeed, float rotationSpeed, float elevationSpeed )
        {
            if ( translationSpeed > 0.0 && translationSpeed <= 1.0 )
            {
                this._droneTranslationSpeed = translationSpeed;
            }

            if ( rotationSpeed > 0.0 && rotationSpeed <= 1.0 )
            {
                this._droneRotationSpeed = rotationSpeed;
            }

            if ( rotationSpeed > 0.0 && rotationSpeed <= 1.0 )
            {
                this._droneElevationSpeed = elevationSpeed;
            }
        }

        /// <summary>
        /// Reset drone speeds to their defaults.
        /// </summary>
        internal void SetDroneSpeeds()
        {
            this._droneTranslationSpeed = DEFAULT_MAXIMUM_DRONE_TRANSLATION_SPEED;
            this._droneRotationSpeed = DEFAULT_MAXIMUM_DRONE_ROTATION_SPEED;
            this._droneElevationSpeed = DEFAULT_MAXIMUM_DRONE_ELEVATION_SPEED;
        }

        #endregion Contructor/initializers/event handlers

        #region Commands

        private void CreateDroneCommands()
        {
            OpenDroneSettingsCommand = new RelayCommand( OpenDroneSettingsExecute, CanExecuteOpenDroneSettingsCommand );
            ReconnectDroneCommand = new RelayCommand( ReconnectDroneExecute, CanExecuteReconnectDroneCommand );
        }

        private bool CanExecuteOpenDroneSettingsCommand( object param )
        {
            return true;
        }

        private void OpenDroneSettingsExecute( object param )
        {
            if ( _droneSettingsWindow != null )
            {
                _droneSettingsWindow.Activate();
            }
            else
            {
                EventHandler<ARDrone.Control.DroneConfig> newDroneConfigDelegate = (sender, e) => {
                    this._currentDroneConfig = e;
                    ReconnectDrone();
                };
                DroneSettingsWindow window = new DroneSettingsWindow( this._currentDroneConfig );
                window.DroneConfigAvailable += newDroneConfigDelegate;

                _droneSettingsWindow = window;

                _droneSettingsWindow.Closed += ( object sender, EventArgs args ) => {
                    _droneSettingsWindow = null;
                    window.DroneConfigAvailable -= newDroneConfigDelegate;
                };

                _droneSettingsWindow.Show();
            }
        }

        private bool CanExecuteReconnectDroneCommand( object param )
        {
            return true;
        }

        private void ReconnectDroneExecute( object param )
        {
            ReconnectDrone();
        }

        #endregion Commands

        #region Utilities

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

        public class RelayCommand : ICommand
        {
            #region Fields

            readonly Action<object> _execute;
            readonly Predicate<object> _canExecute;        

            #endregion // Fields

            #region Constructors

            public RelayCommand(Action<object> execute)
            : this(execute, null)
            {
            }

            public RelayCommand(Action<object> execute, Predicate<object> canExecute)
            {
                if (execute == null)
                    throw new ArgumentNullException("execute");

                _execute = execute;
                _canExecute = canExecute;           
            }
            #endregion // Constructors

            #region ICommand Members

            [System.Diagnostics.DebuggerStepThrough]
            public bool CanExecute(object parameter)
            {
                return _canExecute == null ? true : _canExecute(parameter);
            }

            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }

            public void Execute(object parameter)
            {
                _execute(parameter);
            }

            #endregion // ICommand Members
        }
        #endregion Utilities

    }


}
