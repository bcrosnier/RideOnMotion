using System;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace RideOnMotion.UI
{
    /// <summary>
    /// View model for main window. Contains displayed properties.
    /// </summary>
    class MainWindowViewModel : IViewModel, INotifyPropertyChanged
    {
        private readonly int MAX_LOG_ENTRIES = 50; // Maximum number of log entries in the collection

        /// <summary>
        /// Kinect model : Handles data in and out of the Kinect
        /// </summary>
        private IDroneInputController _inputController;

        private DroneInitializer _droneInit;

        #region Values
        private ImageSource _droneImageSource;
        private ImageSource _inputImageSource;
        private Control _inputControlUI;
        private MenuItem _inputMenu;

        private List<Type> InputTypes { get; set; }

		private string _inputStatusInfo = String.Empty;
        
        private ObservableCollection<String> _logStrings;

		internal bool Konami = false;

		private System.Windows.Media.MediaPlayer mp1 = new System.Windows.Media.MediaPlayer();
		private System.Windows.Media.MediaPlayer mp2 = new System.Windows.Media.MediaPlayer();
		private System.Windows.Media.MediaPlayer mp3 = new System.Windows.Media.MediaPlayer();
		private System.Windows.Media.MediaPlayer mp4 = new System.Windows.Media.MediaPlayer();

        internal event EventHandler<MenuItem> InputMenuChanged;

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

        public String InputStatusInfo
        {
            get
            {
                return  _inputController.Name + ": " + _inputStatusInfo;
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

            // Bind front drone camera
            _droneInit.DroneFrameReady += OnDroneFrameReady;
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
            if ( _logStrings.Count >= MAX_LOG_ENTRIES )
            {
                _logStrings.RemoveAt( 0 );
            }
            _logStrings.Add( e );
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
        }

		public void OnControllerActivity(object sender, bool e)
		{
			this.OnNotifyPropertyChange( "IsActive" );
		}

        internal void Stop()
        {
            this._inputController.Stop();
        }

        #endregion Contructor/initializers/event handlers
    }


}
