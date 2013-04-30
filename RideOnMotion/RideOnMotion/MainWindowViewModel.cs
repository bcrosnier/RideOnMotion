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
using System.Drawing;

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

        public TriggerArea LeftTriggerArea { get; private set; }
        public TriggerArea RightTriggerArea { get; private set; }
        public List<Rectangle> TriggerZoneCollection {
            get {
                return LeftTriggerArea.TriggerZoneCollection.Values
                    .Union(
                        RightTriggerArea.TriggerZoneCollection.Values
                    ).ToList();
            }
        }

        private System.Windows.Point _leftHandPoint = new System.Windows.Point( 0, 0 );
        private System.Windows.Point _rightHandPoint = new System.Windows.Point( 0, 0 );

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

        #region Contructor/initializers/event handlers/methods
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
            initTriggerZones( 300, 100 );
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

        private void OnRightHandPoint( object sender, System.Windows.Point e )
        {
            this._rightHandPoint = e;
            this.OnNotifyPropertyChange( "LeftHandX" );
            this.OnNotifyPropertyChange( "LeftHandY" );
            this.OnNotifyPropertyChange( "LeftHandVisibility" );
        }

        private void OnLeftHandPoint( object sender, System.Windows.Point e )
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

        private void initTriggerZones( int buttonWidth, int buttonHeight )
        {
            int zoneWidth = 640 / 2;
            int zoneHeight = 480;

            LeftTriggerArea = new TriggerArea( zoneWidth, zoneHeight, 0, 0, buttonWidth, buttonHeight );
            RightTriggerArea = new TriggerArea( zoneWidth, zoneHeight, zoneWidth, 0, buttonWidth, buttonHeight );
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

    /**
     * Trigger zone, reprensenting 4 overlayed rectangles (here, 'buttons') as sides of a square.
     * Centered in a given area.
     **/
    public class TriggerArea
    {
        public enum Buttons { Up = 0, Down = 1, Left = 2, Right = 3 };

        public int AreaWidth { get; private set; }
        public int AreaHeight { get; private set; }
        public int OffsetX { get; private set; }
        public int OffsetY { get; private set; }
        public int ButtonWidth { get; private set; }
        public int ButtonHeight { get; private set; }

        public Rectangle LeftButton { get { return this.TriggerZoneCollection[Buttons.Left]; } }
        public Rectangle RightButton { get { return this.TriggerZoneCollection[Buttons.Right]; } }
        public Rectangle UpButton { get { return this.TriggerZoneCollection[Buttons.Up]; } }
        public Rectangle DownButton { get { return this.TriggerZoneCollection[Buttons.Down]; } }

        public Dictionary<Buttons, Rectangle> TriggerZoneCollection { get; private set; }

        public TriggerArea(int areaWidth, int areaHeight, int offsetX, int offsetY, int buttonWidth, int buttonHeight)
        {
            this.AreaHeight = areaHeight;
            this.AreaWidth = areaWidth;
            this.OffsetX = offsetX;
            this.OffsetY = offsetY;
            this.ButtonWidth = buttonWidth;
            this.ButtonHeight = buttonHeight;

            TriggerZoneCollection = new Dictionary<Buttons, Rectangle>();
            generateButtonRectangles();
        }

        public void generateButtonRectangles()
        {
            int horizontalMargin = ( AreaWidth - ButtonWidth ) / 2;
            int verticalMargin = ( AreaHeight - ButtonWidth ) / 2;

            Rectangle upZone = new Rectangle
            {
                Height = ButtonHeight,
                Width = ButtonWidth,
                X = OffsetX + horizontalMargin,
                Y = OffsetY + verticalMargin
            };

            Rectangle leftZone = new Rectangle
            {
                Height = ButtonWidth,
                Width = ButtonHeight,
                X = OffsetX + horizontalMargin,
                Y = OffsetY + verticalMargin
            };

            Rectangle downZone = new Rectangle
            {
                Height = ButtonHeight,
                Width = ButtonWidth,
                X = OffsetX + horizontalMargin,
                Y = OffsetY + verticalMargin + ButtonWidth - ButtonHeight
            };

            Rectangle rightZone = new Rectangle
            {
                Height = ButtonWidth,
                Width = ButtonHeight,
                X = OffsetX + horizontalMargin + ButtonWidth - ButtonHeight,
                Y = OffsetY + verticalMargin
            };

            TriggerZoneCollection.Add( Buttons.Up, upZone );
            TriggerZoneCollection.Add( Buttons.Down, downZone );
            TriggerZoneCollection.Add( Buttons.Left, leftZone );
            TriggerZoneCollection.Add( Buttons.Right, rightZone );

            // Fire stuff to update rectangles here. --BC
        }
    }
}
