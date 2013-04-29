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

namespace RideOnMotion
{
    /// <summary>
    /// Interaction logic for KinectDeviceSettings.xaml
    /// </summary>
    public partial class KinectDeviceSettings : Window
    {
        private KinectDeviceSettingsViewModel _viewModel;

        public KinectDeviceSettings(KinectModule.KinectSensorController controller)
        {
            this._viewModel = new KinectDeviceSettingsViewModel(controller);
            this.DataContext = _viewModel;
            InitializeComponent();
        }

        private void ButtonCancel_Click( object sender, RoutedEventArgs e )
        {
            this.Close();
        }

        private void ButtonOK_Click( object sender, RoutedEventArgs e )
        {
            this.applySettings();
            this.Close();
        }

        private void ButtonApply_Click( object sender, RoutedEventArgs e )
        {
            this.applySettings();
        }

        private void applySettings()
        {
            this._viewModel.applySettings();
        }

    }

    public class KinectDeviceSettingsViewModel : INotifyPropertyChanged
    {
        private KinectModule.KinectSensorController _controller;

        private int _minimumElevationAngle;
        private int _maximumElevationAngle;
        private int _currentElevationAngle;
        private bool _nearModeIsEnabled;

        public KinectDeviceSettingsViewModel( KinectModule.KinectSensorController controller )
        {
            this._controller = controller;
            this.MinimumElevationAngle = this._controller.Sensor.MinElevationAngle;
            this.MaximumElevationAngle = this._controller.Sensor.MaxElevationAngle;

            int angle = this._controller.Sensor.ElevationAngle;

            if ( angle > MaximumElevationAngle )
            {
                this.CurrentElevationAngle = MaximumElevationAngle;
            }
            else if ( angle < MinimumElevationAngle )
            {
                this.CurrentElevationAngle = MinimumElevationAngle;
            }
            else
            {
                this.CurrentElevationAngle = this._controller.Sensor.ElevationAngle;
            }

            if( this._controller.Sensor.DepthStream.Range == Microsoft.Kinect.DepthRange.Near )
            {
                this.NearModeIsEnabled = true;
            } else {
                this.NearModeIsEnabled = false;
            }
        }

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

        public int MinimumElevationAngle
        {
            get
            {
                return this._minimumElevationAngle;
            }

            set
            {
                if ( this._minimumElevationAngle != value )
                {
                    this._minimumElevationAngle = value;
                    this.OnNotifyPropertyChange( "MinimumElevationAngle" );
                }
            }
        }

        public int MaximumElevationAngle
        {
            get
            {
                return this._maximumElevationAngle;
            }

            set
            {
                if ( this._maximumElevationAngle != value )
                {
                    this._maximumElevationAngle = value;
                    this.OnNotifyPropertyChange( "MaximumElevationAngle" );
                }
            }
        }

        public int CurrentElevationAngle
        {
            get
            {
                return this._currentElevationAngle;
            }

            set
            {
                if ( this._currentElevationAngle != value )
                {
                    this._currentElevationAngle = value;
                    this.OnNotifyPropertyChange( "CurrentElevationAngle" );
                }
            }
        }

        public bool NearModeIsEnabled
        {
            get
            {
                return this._nearModeIsEnabled;
            }

            set
            {
                if ( this._nearModeIsEnabled != value )
                {
                    this._nearModeIsEnabled = value;
                    this.OnNotifyPropertyChange( "NearModeIsEnabled" );
                }
            }
        }

        public void applySettings() {
            if ( this.NearModeIsEnabled )
            {
                _controller.Sensor.DepthStream.Range = Microsoft.Kinect.DepthRange.Near;
            }
            else
            {
                _controller.Sensor.DepthStream.Range = Microsoft.Kinect.DepthRange.Default;
            }

            Task.Factory.StartNew( () =>
            {
                try
                {
                    _controller.Sensor.ElevationAngle = this.CurrentElevationAngle;
                }
                catch ( InvalidOperationException e )
                {
                    // Log ElevationAngle error here
                    System.Console.WriteLine( e.Message );
                }
            } );
        }
    }
}
