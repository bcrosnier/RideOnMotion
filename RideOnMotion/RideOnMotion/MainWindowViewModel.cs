using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace RideOnMotion
{
    /// <summary>
    /// View model for main window. Contains displayed properties.
    /// </summary>
    class MainWindowViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Kinect model : Handles data in and out of the Kinect
        /// </summary>
        private KinectModule.KinectSensorController _sensorController;

        #region Values
        public CommandBinding ResetSensorCommandBinding { get; private set; }
        public RoutedCommand ResetSensorCommand { get; private set; }
        public CommandBinding OpenKinectSettingsCommandBinding { get; private set; }
        public RoutedCommand OpenKinectSettingsCommand { get; private set; }

        private BitmapSource _droneBitmapSource;
        private BitmapSource _depthBitmapSource;

        private Point _leftHandPoint = new Point( 0, 0 );
        private Point _rightHandPoint = new Point( 0, 0 );

        private String _logString;

        #endregion Values

        #region GettersSetters

        public BitmapSource DepthBitmapSource
        {
            get
            {
                return this._depthBitmapSource;
            }

            set
            {
                if ( this._depthBitmapSource != value )
                {
                    this._depthBitmapSource = value;
                    this.OnNotifyPropertyChange( "DepthBitmapSource" );
                }
            }
        }

        public BitmapSource DroneBitmapSource
        {
            get
            {
                return this._droneBitmapSource;
            }

            set
            {
                if ( this._droneBitmapSource != value )
                {
                    this._droneBitmapSource = value;
                    this.OnNotifyPropertyChange( "DroneBitmapSource" );
                }
            }
        }

        public int LeftHandX
        {
            get { return (int)this._leftHandPoint.X; }
        }

        public int LeftHandY
        {
            get { return (int)this._leftHandPoint.Y; }
        }

        public Visibility LeftHandVisibility
        {
            get {
                bool exists = ( this._leftHandPoint.Y == 0.0 && this._leftHandPoint.X == 0.0)  ? false : true;
                if ( exists )
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }

        public int RightHandX
        {
            get { return (int)this._rightHandPoint.X; }
        }

        public int RightHandY
        {
            get { return (int)this._rightHandPoint.Y; }
        }

        public Visibility RightHandVisibility
        {
            get
            {
                bool exists = ( this._rightHandPoint.Y == 0.0 && this._rightHandPoint.X == 0.0 ) ? false : true;
                if ( exists )
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }

        public String LogString
        {
            get
            {
                return this._logString;
            }

            set
            {
                if ( this._logString != value )
                {
                    this._logString = value;
                    this.OnNotifyPropertyChange( "LogString" );
                }
            }
        }

        public String SensorStatusInfo
        {
            get
            {
                string statusString = !this._sensorController.HasSensor ? KinectStatus.Disconnected.ToString() : _sensorController.Sensor.Status.ToString();
                return "Kinect device: " + statusString;
            }
        }

        public bool CanUseSensor
        {
            get
            {
                return ( this._sensorController.HasSensor && this._sensorController.SensorIsRunning );
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

        #region Contructor/initializers/event handlers
        /// <summary>
        /// Initializes the ViewModel with the given KinectSensorController.
        /// </summary>
        /// <param name="sensorController">Kinect model : Handles data in and out of the Kinect</param>
        public MainWindowViewModel( KinectModule.KinectSensorController sensorController )
        {
            _sensorController = sensorController;
            ResetSensorCommand = new RoutedCommand();
            OpenKinectSettingsCommand = new RoutedCommand();

            this.ResetSensorCommandBinding = new CommandBinding( ResetSensorCommand,
                this.ResetSensorExecuted,
                this.CanExecuteKinectCommands );

            this.OpenKinectSettingsCommandBinding = new CommandBinding( OpenKinectSettingsCommand,
                this.OpenKinectSettingsExecuted,
                this.CanExecuteKinectCommands );

            initializeBindings();
        }

        /// <summary>
        /// Creates the event bindings with the model.
        /// </summary>
        private void initializeBindings() {
            // Bind depth image changes
            _sensorController.DepthBitmapSourceReady += OnDepthBitmapSourceChanged;

            // Bind sensor status
            _sensorController.SensorChanged += ( sender, e ) => {
                this.OnNotifyPropertyChange( "SensorStatusInfo" );
                this.OnNotifyPropertyChange( "CanUseSensor" );
            };

            _sensorController.LeftHandPointReady += OnLeftHandPoint;
            _sensorController.RightHandPointReady += OnRightHandPoint;

            // TODO : DEMO
            //Logger myLogger = new Logger();
            //myLogger.NewLogStringReady += OnLogStringReceived; // Event binding
            //myLogger.NewEntry( CK.Core.LogLevel.Fatal, MyTag, "Test message" ); 
        }

        private void OnRightHandPoint( object sender, Point e )
        {
            this._rightHandPoint = e;
            this.OnNotifyPropertyChange( "LeftHandX" );
            this.OnNotifyPropertyChange( "LeftHandY" );
            this.OnNotifyPropertyChange( "LeftHandVisibility" );
        }

        private void OnLeftHandPoint( object sender, Point e )
        {
            this._leftHandPoint = e;
            this.OnNotifyPropertyChange( "RightHandX" );
            this.OnNotifyPropertyChange( "RightHandY" );
            this.OnNotifyPropertyChange( "RightHandVisibility" );
        }

        private void OnDepthBitmapSourceChanged( object sender, KinectModule.BitmapSourceEventArgs e )
        {
            DepthBitmapSource = e.BitmapSource;
        }

        private void OnLogStringReceived( object sender, String e )
        {
            this.LogString = e; // Will fire NotifyPropertyChanged
        }

        #endregion Contructor/initializers/event handlers

        #region Commands/command helpers
        public void CanExecuteKinectCommands(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.CanUseSensor;
            e.Handled = true;
        }

        public void ResetSensorExecuted( object sender, ExecutedRoutedEventArgs e )
        {
            if ( !this.CanUseSensor )
            {
                return; // Command should be disabled anyway
            }
            this._sensorController.resetSensor();

            e.Handled = true;
        }

        public void OpenKinectSettingsExecuted( object sender, ExecutedRoutedEventArgs e )
        {
            if ( !this.CanUseSensor )
            {
                return; // Command should be disabled anyway
            }

            Window deviceSettingsWindow = new KinectDeviceSettings( this._sensorController );
            deviceSettingsWindow.Show();

            e.Handled = true;
        }
        #endregion Commands/command helpers
    }
}
