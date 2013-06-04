﻿using ARDrone.Control;
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

namespace RideOnMotion.UI
{
    /// <summary>
    /// Interaction logic for DroneSettingsWindow.xaml
    /// </summary>
    public partial class DroneSettingsWindow : Window
    {
        private DroneSettingsWindowViewModel _viewModel;
        public event EventHandler<DroneConfig> DroneConfigAvailable;

        public DroneSettingsWindow(DroneConfig config)
        {
            if ( config == null )
            {
                throw new ArgumentNullException( "Config cannot be null" );
            }

            _viewModel = new DroneSettingsWindowViewModel( config );

            this.DataContext = _viewModel;

            InitializeComponent();
        }

        private void ButtonOK_Click( object sender, RoutedEventArgs e )
        {
            _viewModel.SaveSettings();
            RaiseDroneConfigAvailable( _viewModel.DroneConfig );
            this.Close();
        }

        private void ButtonCancel_Click( object sender, RoutedEventArgs e )
        {
            this.Close();
        }

        private void ButtonApply_Click( object sender, RoutedEventArgs e )
        {
            _viewModel.SaveSettings();
            RaiseDroneConfigAvailable( _viewModel.DroneConfig );
        }

        private void RaiseDroneConfigAvailable(DroneConfig config)
        {
            if ( DroneConfigAvailable != null )
            {
                DroneConfigAvailable( this, config );
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

        private String _droneIPAddress, _clientIPAddress, _droneSSID;
        private DroneConfig _droneConfig;
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
        #endregion Properties

        public DroneSettingsWindowViewModel( DroneConfig config )
        {
            if ( config == null )
            {
                throw new ArgumentNullException( "Config cannot be null." );
            }
            this._droneConfig = config;
        }

        internal void SaveSettings()
        {
            Settings.Default.DroneIPAddress = this.DroneIPAddress;
            Settings.Default.ClientIPAddress = this.ClientIPAddress;
            Settings.Default.DroneSSID = this.DroneSSID;

            Settings.Default.Save();
        }
    }
}
