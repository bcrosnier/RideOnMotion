using Microsoft.Kinect;
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
using CK.Core;

namespace RideOnMotion.Inputs.Kinect
{
    /// <summary>
    /// Interaction logic for KinectDeviceSettings.xaml
    /// </summary>
    public partial class KinectDeviceSettings : Window
    {
        private KinectDeviceSettingsViewModel _viewModel;

        public KinectDeviceSettings(IActivityLogger parentLogger, Inputs.Kinect.KinectSensorController controller)
        {

            this._viewModel = new KinectDeviceSettingsViewModel( parentLogger, controller );
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
        private Inputs.Kinect.KinectSensorController _controller;
        private IActivityLogger _logger;

        private int _minimumElevationAngle;
        private int _maximumElevationAngle;
        private int _currentElevationAngle;
        private bool _nearModeIsEnabled;
        private bool _seatingModeIsEnabled;
        private bool _skeletonSmoothingIsEnabled;
        private bool _depthImageIsDisabled;

        public KinectDeviceSettingsViewModel( IActivityLogger parentLogger, Inputs.Kinect.KinectSensorController controller )
        {
            _logger = new DefaultActivityLogger();
            _logger.AutoTags = ActivityLogger.RegisteredTags.FindOrCreate( "Settings" );
            _logger.Output.BridgeTo( parentLogger );

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

            if ( this._controller.Sensor.DepthStream.Range == Microsoft.Kinect.DepthRange.Near )
            {
                this.NearModeIsEnabled = true;
            }
            else
            {
                this.NearModeIsEnabled = false;
            }

            if ( this._controller.Sensor.SkeletonStream.TrackingMode == Microsoft.Kinect.SkeletonTrackingMode.Seated )
            {
                this.SeatingModeIsEnabled = true;
            }
            else
            {
                this.SeatingModeIsEnabled = false;
            }

            if ( this._controller.Sensor.SkeletonStream.IsSmoothingEnabled )
            {
                this.SkeletonSmoothingIsEnabled = true;
            }
            else
            {
                this.SkeletonSmoothingIsEnabled = false;
            }

            if ( this._controller.DepthImageEnabled )
            {
                this.DepthImageIsDisabled = false;
            }
            else
            {
                this.DepthImageIsDisabled = true;
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

        public bool SeatingModeIsEnabled
        {
            get
            {
                return this._seatingModeIsEnabled;
            }

            set
            {
                if ( this._seatingModeIsEnabled != value )
                {
                    this._seatingModeIsEnabled = value;
                    this.OnNotifyPropertyChange( "SeatingModeIsEnabled" );
                }
            }
        }

        public bool SkeletonSmoothingIsEnabled
        {
            get
            {
                return this._skeletonSmoothingIsEnabled;
            }

            set
            {
                if ( this._skeletonSmoothingIsEnabled != value )
                {
                    this._skeletonSmoothingIsEnabled = value;
                    this.OnNotifyPropertyChange( "SkeletonSmoothingIsEnabled" );
                }
            }
        }

        public bool DepthImageIsDisabled
        {
            get
            {
                return this._depthImageIsDisabled;
            }

            set
            {
                if ( this._depthImageIsDisabled != value )
                {
                    this._depthImageIsDisabled = value;
                    this.OnNotifyPropertyChange( "DepthImageIsDisabled" );
                }
            }
        }

        public void applySettings() {

			bool settingChanged = false;
            if ( _controller.Sensor.Status != Microsoft.Kinect.KinectStatus.Connected )
            {
                return; // Do nothing if Kinect isn't ready
            }

            // Near mode
			if ( this.NearModeIsEnabled 
				&& _controller.Sensor.DepthStream.Range == Microsoft.Kinect.DepthRange.Default )
            {
                _controller.Sensor.DepthStream.Range = Microsoft.Kinect.DepthRange.Near;
				settingChanged = true;
            }
            else if ( !this.NearModeIsEnabled 
				&& _controller.Sensor.DepthStream.Range == Microsoft.Kinect.DepthRange.Near)
            {
				_controller.Sensor.DepthStream.Range = Microsoft.Kinect.DepthRange.Default;
				settingChanged = true;
            }

            // Seating mode
            if ( this.SeatingModeIsEnabled 
				&& _controller.Sensor.SkeletonStream.TrackingMode == Microsoft.Kinect.SkeletonTrackingMode.Default)
            {
				_controller.Sensor.SkeletonStream.TrackingMode = Microsoft.Kinect.SkeletonTrackingMode.Seated;
				settingChanged = true;
            }
			else if ( !this.SeatingModeIsEnabled 
				&& _controller.Sensor.SkeletonStream.TrackingMode == Microsoft.Kinect.SkeletonTrackingMode.Seated )
            {
				_controller.Sensor.SkeletonStream.TrackingMode = Microsoft.Kinect.SkeletonTrackingMode.Default;
				settingChanged = true;
            }

            // Smoothing
            if ( this.SkeletonSmoothingIsEnabled 
				&& _controller.SmoothingEnabled == false)
            {
				_controller.SetSkeletonSmoothingEnabled( true );
				settingChanged = true;
				
            }
			else if ( !this.SkeletonSmoothingIsEnabled 
				&& _controller.SmoothingEnabled == true )
            {
				_controller.SetSkeletonSmoothingEnabled( false );
				settingChanged = true;
            }

            // Disable depth rendering
            // Seating mode
            if ( this.DepthImageIsDisabled
                && _controller.DepthImageEnabled )
            {
                _controller.DepthImageEnabled = false;
                settingChanged = true;
            }
            else if ( !this.DepthImageIsDisabled
                && !_controller.DepthImageEnabled )
            {
                _controller.DepthImageEnabled = true;
                settingChanged = true;
            }

			if ( settingChanged )
			{
				_controller.resetSensor();
			}

			//we are checking for +1 and -1 since Kinect can't distinguish between 1° angles
			if ( _controller.Sensor.ElevationAngle < this.CurrentElevationAngle - 1 
				|| _controller.Sensor.ElevationAngle > this.CurrentElevationAngle + 1 )
			{
				Task.Factory.StartNew( () =>
				{
					try
					{
						_controller.Sensor.ElevationAngle = this.CurrentElevationAngle;
					}
					catch ( InvalidOperationException e )
					{
                        // Log ElevationAngle error here
                        _logger.Error( "Too much movement for the Kinect, please wait 20 sec" );
                        _logger.Error( e );
					}
				} );
			}
        }
    }
}
