using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        private BitmapSource _droneBitmapSource;
        private BitmapSource _depthBitmapSource;

        private Dictionary<String, String> _droneStatusInfo;

        private List<String> _logList;

        public enum KinectStatusInfoKeys { STATUS, ELEVATION_ANGLE }

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

        public Dictionary<String, String> DroneStatusInfo
        {
            get
            {
                return this._droneStatusInfo;
            }

            set
            {
                if ( this._droneStatusInfo != value )
                {
                    this._droneStatusInfo = value;
                    this.OnNotifyPropertyChange( "DroneStatusInfo" );
                }
            }
        }

        public Dictionary<KinectStatusInfoKeys, string> SensorStatusInfo
        {
            get
            {
                return this.generateKinectStatusInfo();
            }
        }

        public string SensorStatusInfoString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach ( KeyValuePair<KinectStatusInfoKeys, String> entry in SensorStatusInfo )
                {
                    sb.Append( entry.Key.ToString() );
                    sb.Append( " : " );
                    sb.Append( entry.Value );

                    sb.Append( '\n' );
                }

                return sb.ToString().TrimEnd( '\n' );
            }
        }

        #endregion GettersSetters

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

        /// <summary>
        /// Initializes the ViewModel with the given KinectSensorController.
        /// </summary>
        /// <param name="sensorController">Kinect model : Handles data in and out of the Kinect</param>
        public MainWindowViewModel( KinectModule.KinectSensorController sensorController )
        {
            _sensorController = sensorController;

            initializeBindings();
        }

        /// <summary>
        /// Creates the event bindings with the model.
        /// </summary>
        private void initializeBindings() {
            // Bind depth image changes
            _sensorController.DepthBitmapSourceReady += OnDepthBitmapSourceChanged;

            // Bind sensor status
            _sensorController.SensorChanged += ( sender, e ) => { this.OnNotifyPropertyChange( "SensorStatusInfo" ); this.OnNotifyPropertyChange( "SensorStatusInfoString" ); };
        }

        /// <summary>
        /// Create a dictionary with the Sensor's status, elevation angle, etc.
        /// </summary>
        /// <returns></returns>
        private Dictionary<KinectStatusInfoKeys, string> generateKinectStatusInfo()
        {
            Dictionary<KinectStatusInfoKeys, string> dict = new Dictionary<KinectStatusInfoKeys, string>();

            string statusString = !this._sensorController.HasSensor ? KinectStatus.Disconnected.ToString() : _sensorController.Sensor.Status.ToString();

            dict.Add( KinectStatusInfoKeys.STATUS, statusString );

            if ( _sensorController.SensorIsRunning )
            {
                dict.Add( KinectStatusInfoKeys.ELEVATION_ANGLE, _sensorController.Sensor.ElevationAngle.ToString() );
            }

            return dict;
        }

        private void OnDepthBitmapSourceChanged( object sender, KinectModule.BitmapSourceEventArgs e )
        {
            DepthBitmapSource = e.BitmapSource;
        }
    } 
}
