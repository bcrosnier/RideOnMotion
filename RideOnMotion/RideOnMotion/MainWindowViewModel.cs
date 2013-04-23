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
        #region Values

        private BitmapSource _droneBitmapSource;
        private BitmapSource _depthBitmapSource;

        private Dictionary<String, String> _sensorStatusInfo;
        private Dictionary<String, String> _droneStatusInfo;

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

        public Dictionary<String, String> SensorStatusInfo
        {
            get
            {
                return this._sensorStatusInfo;
            }

            set
            {
                if ( this._sensorStatusInfo != value )
                {
                    this._sensorStatusInfo = value;
                    this.OnNotifyPropertyChange( "SensorStatusInfo" );
                }
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
    }
}
