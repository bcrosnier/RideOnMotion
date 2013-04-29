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
using System.Collections.ObjectModel;

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
        public ObservableCollection<TriggerZone> TriggerZoneCollection { get; private set; }

        private Point _leftHandPoint = new Point( 0, 0 );
        private Point _rightHandPoint = new Point( 0, 0 );

        private List<String> _logList;

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

        public List<String> LogList
        {
            get
            {
                return this._logList;
            }

            set
            {
                if ( this._logList != value )
                {
                    this._logList = value;
                    this.OnNotifyPropertyChange( "LogList" );
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
            initTriggerZones( 320, 75 );
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

        private void initTriggerZones( int zoneWidth, int zoneHeight )
        {
            int baseWidth = 640;
            int baseHeight = 480;
            int horizontalMargin = (baseWidth / 2 - zoneWidth) / 2;
            int verticalMargin = (baseHeight - zoneWidth) / 2;

            TriggerZoneCollection = new ObservableCollection<TriggerZone>();

            TriggerZone leftUpZone = new TriggerZone
            {
                Height = zoneHeight,
                Width = zoneWidth,
                X = horizontalMargin,
                Y = verticalMargin
            };

            TriggerZone leftLeftZone = new TriggerZone
            {
                Height = zoneWidth,
                Width = zoneHeight,
                X = horizontalMargin,
                Y = verticalMargin
            };

            TriggerZone leftDownZone = new TriggerZone
            {
                Height = zoneHeight,
                Width = zoneWidth,
                X = horizontalMargin,
                Y = verticalMargin + zoneWidth - zoneHeight
            };

            TriggerZone leftRightZone = new TriggerZone
            {
                Height = zoneWidth,
                Width = zoneHeight,
                X = baseWidth / 2 + horizontalMargin + zoneWidth - zoneHeight,
                Y = verticalMargin
            };

            TriggerZone rightUpZone = new TriggerZone
            {
                Height = zoneHeight,
                Width = zoneWidth,
                X = baseWidth / 2 + horizontalMargin,
                Y = verticalMargin
            };

            TriggerZone rightLeftZone = new TriggerZone
            {
                Height = zoneWidth,
                Width = zoneHeight,
                X = baseWidth / 2 + horizontalMargin,
                Y = verticalMargin
            };

            TriggerZone rightDownZone = new TriggerZone
            {
                Height = zoneHeight,
                Width = zoneWidth,
                X = baseWidth / 2 + horizontalMargin,
                Y = verticalMargin + zoneWidth - zoneHeight
            };

            TriggerZone rightRightZone = new TriggerZone
            {
                Height = zoneWidth,
                Width = zoneHeight,
                X = horizontalMargin + zoneWidth - zoneHeight,
                Y = verticalMargin
            };

            TriggerZoneCollection.Add( leftLeftZone );
            TriggerZoneCollection.Add( leftUpZone );
            TriggerZoneCollection.Add( leftDownZone );
            TriggerZoneCollection.Add( leftRightZone );
            TriggerZoneCollection.Add( rightLeftZone );
            TriggerZoneCollection.Add( rightUpZone );
            TriggerZoneCollection.Add( rightDownZone );
            TriggerZoneCollection.Add( rightRightZone );
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

    public class TriggerZone
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
    }
}
