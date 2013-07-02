using ARDrone.Control;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using RideOnMotion.UI.Properties;
using CK.Core;

namespace RideOnMotion.UI
{
    /// <summary>
    /// Interaction logic for DroneSettingsWindow.xaml
    /// </summary>
    public partial class DroneSettingsWindow : Window
    {
        private DroneSettingsWindowViewModel _viewModel;
        public event EventHandler<DroneSettingsEventArgs> DroneConfigAvailable;

        public DroneSettingsWindow(IActivityLogger parentLogger, DroneConfig config, bool droneIsPaired, DroneSpeeds DroneSpeeds, bool absoluteControlMode)
        {
            if ( config == null )
            {
                throw new ArgumentNullException( "Config cannot be null" );
            }

            // Clone config. Active one cannot be modified when used.
            DroneConfig newConfig = new DroneConfig()
            {
                DroneIpAddress = config.DroneIpAddress,
                StandardOwnIpAddress = config.StandardOwnIpAddress,
                DroneNetworkIdentifierStart = config.DroneNetworkIdentifierStart,
                NavigationPort = config.NavigationPort,
                VideoPort = config.VideoPort,
                CommandPort = config.CommandPort,
                ControlInfoPort = config.ControlInfoPort,
                UseSpecificFirmwareVersion = config.UseSpecificFirmwareVersion,
                FirmwareVersion = config.FirmwareVersion,
                TimeoutValue = config.TimeoutValue
            };

            _viewModel = new DroneSettingsWindowViewModel( parentLogger, newConfig, DroneSpeeds );

			_viewModel.DroneIsPaired = droneIsPaired;
			_viewModel.AbsoluteControlMode = absoluteControlMode;
            this.DataContext = _viewModel;

            InitializeComponent();
        }

        private void ButtonOK_Click( object sender, RoutedEventArgs e )
        {
            _viewModel.SaveSettings();
            RaiseDroneConfigAvailable();
            this.Close();
        }

        private void ButtonCancel_Click( object sender, RoutedEventArgs e )
        {
            this.Close();
        }

        private void ButtonApply_Click( object sender, RoutedEventArgs e )
        {
            _viewModel.SaveSettings();
            RaiseDroneConfigAvailable();
        }

        private void RaiseDroneConfigAvailable()
        {
            if ( DroneConfigAvailable != null )
            {
                DroneConfigAvailable( this, new DroneSettingsEventArgs(_viewModel.DroneConfig, _viewModel.DroneIsPaired, _viewModel.DroneSpeeds, _viewModel.AbsoluteControlMode) );
            }
        }
    }

    public class DroneSettingsWindowViewModel : INotifyPropertyChanged
    {
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

        /// <summary>
        /// Throw new PropertyChanged.
        /// </summary>
        /// <param name="caller">Auto-filled with Member name, when called from a property.</param>
        private void RaisePropertyChanged( [System.Runtime.CompilerServices.CallerMemberName] string caller = "" )
        {
            if ( PropertyChanged != null )
            {
                PropertyChanged( this, new PropertyChangedEventArgs( caller ) );
            }
        }
        #endregion INotifyPropertyChanged utilities

        #region Members

		DroneConfig _droneConfig;
		bool _droneIsPaired;
		bool _absoluteControlMode;
		DroneSpeeds _droneSpeeds;
        IActivityLogger _logger;
		String _elevation;
		String _rotation;
		String _translation;

        #endregion Members

        #region Properties
        public string DroneIPAddress
        {
            get { return _droneConfig.DroneIpAddress; }
            set
            {
                if ( value != _droneConfig.DroneIpAddress )
                {
                    _droneConfig.DroneIpAddress = value;
                    RaisePropertyChanged();
                }
            }
        }

        public DroneConfig DroneConfig
        {
            get { return this._droneConfig; }
        }

		public DroneSpeeds DroneSpeeds
		{
			get { return this._droneSpeeds; }
		}


		public bool DroneIsPaired
		{
			get { return this._droneIsPaired; }
			set
			{
				_droneIsPaired = value;
				RaisePropertyChanged();
			}
		}

		public bool AbsoluteControlMode
		{
			get { return this._absoluteControlMode; }
			set
			{
				_absoluteControlMode = value;
				RaisePropertyChanged();
			}
		}

        public string ClientIPAddress
        {
            get { return _droneConfig.StandardOwnIpAddress; }
            set
            {
                if ( value != _droneConfig.StandardOwnIpAddress )
                {
                    _droneConfig.StandardOwnIpAddress = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string DroneSSID
        {
            get { return _droneConfig.DroneNetworkIdentifierStart; }
            set
            {
                if ( value != _droneConfig.DroneNetworkIdentifierStart )
                {
                    _droneConfig.DroneNetworkIdentifierStart = value;
                    RaisePropertyChanged();
                }
            }
        }

		public String TranslationSpeedString
		{
			get
			{
				return _translation;
			}
			set
			{
				if ( value != _translation )
				{
					_translation = value + " %";
					RaisePropertyChanged();
				}
			}
		}
		public String ElevationSpeedString
		{
			get
			{
				return _elevation;
			}
			set
			{
				if ( value != _elevation )
				{
					_elevation = value + " %";
					RaisePropertyChanged();
				}
			}
		}
		public String RotationSpeedString
		{
			get
			{
				return _rotation;
			}
			set
			{
				if ( value != _rotation )
				{
					_rotation = value + " %";
					RaisePropertyChanged();
				}
			}
		}

		public float TranslationSpeed
		{
			get
			{
				return _droneSpeeds.DroneTranslationSpeed;
			}
			set
			{
				if ( value != _droneSpeeds.DroneTranslationSpeed )
				{
					_droneSpeeds.DroneTranslationSpeed = value;
					TranslationSpeedString = (_droneSpeeds.DroneTranslationSpeed*100).ToString();
					RaisePropertyChanged();
				}
			}
		}
		public float RotationSpeed
		{
			get
			{
				return _droneSpeeds.DroneRotationSpeed;
			}
			set
			{
				if ( value != _droneSpeeds.DroneRotationSpeed )
				{
					_droneSpeeds.DroneRotationSpeed = value;
					RotationSpeedString = (_droneSpeeds.DroneRotationSpeed*100).ToString();
					RaisePropertyChanged();
				}
			}
		}
		public float ElevationSpeed
		{
			get
			{
				return _droneSpeeds.DroneElevationSpeed;
			}
			set
			{
				if ( value != _droneSpeeds.DroneElevationSpeed )
				{
					_droneSpeeds.DroneElevationSpeed = value;
					ElevationSpeedString = (_droneSpeeds.DroneElevationSpeed*100).ToString();
					RaisePropertyChanged();
				}
			}
		}
        #endregion Properties

        public DroneSettingsWindowViewModel( IActivityLogger parentLogger, DroneConfig config, DroneSpeeds DroneSpeeds )
        {
            if ( config == null )
            {
                throw new ArgumentNullException( "Config cannot be null." );
            }

            _logger = new DefaultActivityLogger();
            _logger.AutoTags = ActivityLogger.RegisteredTags.FindOrCreate( "Settings" );
            _logger.Output.BridgeTo( parentLogger );

            this._droneConfig = config;
			this._droneSpeeds = DroneSpeeds;

			TranslationSpeedString = (_droneSpeeds.DroneTranslationSpeed*100).ToString();
			RotationSpeedString = (_droneSpeeds.DroneRotationSpeed*100).ToString();
			ElevationSpeedString = (_droneSpeeds.DroneElevationSpeed*100).ToString();
        }

        internal void SaveSettings()
        {
            Settings.Default.DroneIPAddress = this.DroneIPAddress;
            Settings.Default.ClientIPAddress = this.ClientIPAddress;
            Settings.Default.DroneSSID = this.DroneSSID;

            Settings.Default.Save();
            _logger.Info("Saved settings");
        }
    }

    public class DroneSettingsEventArgs : EventArgs
    {
        public DroneConfig DroneConfig
        {
            get;
            private set;
        }
        public bool IsPaired
        {
            get;
            private set;
        }

		public DroneSpeeds DroneSpeeds
		{
			get;
			private set;
		}

		public bool AbsoluteControlMode
		{
			get;
			private set;
		}

        public DroneSettingsEventArgs( DroneConfig newConfig, bool isPaired , DroneSpeeds droneSpeeds, bool absoluteControlMode)
        {
            this.DroneConfig = newConfig;
            this.IsPaired = isPaired;
			this.DroneSpeeds = droneSpeeds;
			this.AbsoluteControlMode = absoluteControlMode;
        }

	}
}
