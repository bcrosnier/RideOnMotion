using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Drawing;
using RideOnMotion.KinectModule;
using System.Windows.Controls;

namespace RideOnMotion
{
    /// <summary>
    /// View model for main window. Contains displayed properties.
    /// </summary>
    class MainWindowViewModel : IViewModel, INotifyPropertyChanged
    {
        /// <summary>
        /// Kinect model : Handles data in and out of the Kinect
        /// </summary>
        private IDroneInputController _inputController;

        #region Values
        private BitmapSource _droneBitmapSource;
        private BitmapSource _inputBitmapSource;
        private Control _inputControl;

		private string _sensorStatusInfo = "No Kinect detected.";

        private String _logString;

        #endregion Values

        #region GettersSetters

        public BitmapSource InputBitmapSource
        {
            get
            {
                return this._inputBitmapSource;
            }

            set
            {
                if ( this._inputBitmapSource != value )
                {
                    this._inputBitmapSource = value;
                    this.OnNotifyPropertyChange( "InputBitmapSource" );
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
                return  _sensorStatusInfo;
            }
        }

        public Control InputControl
        {
            get
            {
                return _inputControl;
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
        /// <param name="inputController">Input model : Handles data in and out of the input</param>
        public MainWindowViewModel( IDroneInputController sensorController )
        {
            _inputController = sensorController;

            initializeBindings();

            _inputControl = _inputController.InputUIControl;
            this.OnNotifyPropertyChange( "InputControl" );
        }

        /// <summary>
        /// Creates the event bindings with the model.
        /// </summary>
        private void initializeBindings()
        {
            // Bind depth image changes
            _inputController.InputImageSourceChanged += OnInputBitmapSourceChanged;

            // Bind sensor status
            _inputController.InputStatusChanged += ( sender, e ) =>
            {

                _sensorStatusInfo = _inputController.InputStatusString;
                this.OnNotifyPropertyChange( "InputStatusInfo" );
            };

            Logger.Instance.NewLogStringReady += OnLogStringReceived;
        }

        private void OnInputBitmapSourceChanged( object sender, BitmapSource s )
        {
            InputBitmapSource = s;
        }

        private void OnLogStringReceived( object sender, String e )
        {
            this.LogString = e; // Will fire NotifyPropertyChanged
        }

        #endregion Contructor/initializers/event handlers
    }

    /// <summary>
    /// Trigger zone, reprensenting 4 overlayed TriggerButtons as sides of a square.
    /// Centered in a given area.
    /// </summary>
    public class TriggerArea
    {
        public enum Buttons { Up = 0, Down = 1, Left = 2, Right = 3 };

        public int AreaWidth { get; private set; }
        public int AreaHeight { get; private set; }
        public int OffsetX { get; private set; }
        public int OffsetY { get; private set; }
        public int ButtonWidth { get; private set; }
        public int ButtonHeight { get; private set; }

        public ICaptionArea LeftButton { get { return this.TriggerCaptionsCollection[Buttons.Left]; } }
        public ICaptionArea RightButton { get { return this.TriggerCaptionsCollection[Buttons.Right]; } }
        public ICaptionArea UpButton { get { return this.TriggerCaptionsCollection[Buttons.Up]; } }
        public ICaptionArea DownButton { get { return this.TriggerCaptionsCollection[Buttons.Down]; } }

        public Dictionary<Buttons, ICaptionArea> TriggerCaptionsCollection { get; private set; }
        private KinectSensorController.SkelPointToDepthPoint _converter;

        /// <summary>
        /// Create a trigger area, with 4 triggerable captions.
        /// </summary>
        /// <param name="areaWidth">Total width of the zone to use ; area will be centered in it.</param>
        /// <param name="areaHeight">Total height of the zone to use ; area will be centered in it.</param>
        /// <param name="offsetX">X offset to all buttons</param>
        /// <param name="offsetY">Y offset to all buttons</param>
        /// <param name="buttonWidth">Width of each button. Side size of the area.</param>
        /// <param name="buttonHeight">Height of each button; "thickness" of the area.</param>
        public TriggerArea( int areaWidth, int areaHeight, int offsetX, int offsetY, int buttonWidth, int buttonHeight, KinectSensorController.SkelPointToDepthPoint converter )
        {
            this.AreaHeight = areaHeight;
            this.AreaWidth = areaWidth;
            this.OffsetX = offsetX;
            this.OffsetY = offsetY;
            this.ButtonWidth = buttonWidth;
            this.ButtonHeight = buttonHeight;

            this._converter = converter;

            this.TriggerCaptionsCollection = new Dictionary<Buttons, ICaptionArea>();
            generateButtonCaptions();
        }

        public void generateButtonCaptions()
        {
            int horizontalMargin = ( AreaWidth - ButtonWidth ) / 2;
            int verticalMargin = ( AreaHeight - ButtonWidth ) / 2;

            ICaptionArea upCaption = createCaptionArea(
                    OffsetX + horizontalMargin,
                    OffsetY + verticalMargin,
                ButtonWidth,
                ButtonHeight
            );

            ICaptionArea leftCaption = createCaptionArea(
                    OffsetX + horizontalMargin,
                    OffsetY + verticalMargin,
                ButtonHeight,
                ButtonWidth
            );

            ICaptionArea downCaption = createCaptionArea(
                    OffsetX + horizontalMargin,
                    OffsetY + verticalMargin + ButtonWidth - ButtonHeight,
                ButtonWidth,
                ButtonHeight
            );

            ICaptionArea rightCaption = createCaptionArea(
                    OffsetX + horizontalMargin + ButtonWidth - ButtonHeight,
                    OffsetY + verticalMargin,
                ButtonHeight,
                ButtonWidth
            );

            TriggerCaptionsCollection.Add( Buttons.Up, upCaption );
            TriggerCaptionsCollection.Add( Buttons.Down, downCaption );
            TriggerCaptionsCollection.Add( Buttons.Left, leftCaption );
            TriggerCaptionsCollection.Add( Buttons.Right, rightCaption );

            // Fire stuff to update rectangles here. --BC
        }

        private ICaptionArea createCaptionArea( int depthX, int depthY, int width, int height )
        {
            //DepthImagePoint depthPoint = new DepthImagePoint() { X = depthX, Y = depthY };
            //SkeletonPoint skelPoint = _pointConverter( depthPoint );
            return new CaptionArea(
                   new KinectModule.Point(
                       depthX,
                       depthY
                   ),
                   width,
                   height,
                   _converter
               );
        }
    }
}
