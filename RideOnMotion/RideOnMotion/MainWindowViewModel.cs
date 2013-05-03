﻿using Microsoft.Kinect;
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
        private KinectModule.KinectSensorController _sensorController;

        #region Values
        public CommandBinding ResetSensorCommandBinding { get; private set; }
        public RoutedCommand ResetSensorCommand { get; private set; }
        public CommandBinding OpenKinectSettingsCommandBinding { get; private set; }
        public RoutedCommand OpenKinectSettingsCommand { get; private set; }

        private BitmapSource _droneBitmapSource;
        private BitmapSource _depthBitmapSource;

        private System.Windows.Point _leftHandPoint = new System.Windows.Point( 0, 0 );
        private System.Windows.Point _rightHandPoint = new System.Windows.Point( 0, 0 );

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
            get
            {
                bool exists = ( this._leftHandPoint.Y == 0.0 && this._leftHandPoint.X == 0.0 ) ? false : true;
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
        private Visibility _rightHandVisibility;
        public Visibility RightHandVisibility
        {
            get
            {
                bool exists = this._rightHandPoint.Y != 0.0 && this._rightHandPoint.X != 0.0;
                if ( exists )
                {
                    if ( _rightHandVisibility != Visibility.Visible )
                    {
                        Logger.Instance.NewEntry( CK.Core.LogLevel.Trace, CKTraitTags.User, "Right hand visible" );
                    }
                    _rightHandVisibility = Visibility.Visible;
                    return _rightHandVisibility;
                }
                else
                {
                    if ( _rightHandVisibility != Visibility.Collapsed
                        )
                    {
                        Logger.Instance.NewEntry( CK.Core.LogLevel.Trace, CKTraitTags.User, "Right hand not visible" );
                    }
                    _rightHandVisibility = Visibility.Collapsed;
                    return _rightHandVisibility;
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

        public ObservableCollection<ICaptionArea> TriggerButtons
        {
            get
            {
                return this._sensorController.TriggerButtons;
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
        }

        /// <summary>
        /// Creates the event bindings with the model.
        /// </summary>
        private void initializeBindings()
        {
            // Bind depth image changes
            _sensorController.DepthBitmapSourceReady += OnDepthBitmapSourceChanged;

            // Bind sensor status
            _sensorController.SensorChanged += ( sender, e ) =>
            {
                this.OnNotifyPropertyChange( "SensorStatusInfo" );
                this.OnNotifyPropertyChange( "CanUseSensor" );
                this.OnNotifyPropertyChange( "TriggerButtons" );
            };

            _sensorController.LeftHandPointReady += OnLeftHandPoint;
            _sensorController.RightHandPointReady += OnRightHandPoint;

            Logger.Instance.NewLogStringReady += OnLogStringReceived;
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

        private void OnLogStringReceived( object sender, String e )
        {
            this.LogString = e; // Will fire NotifyPropertyChanged
        }

        #endregion Contructor/initializers/event handlers

        #region Commands/command helpers
        public void CanExecuteKinectCommands( object sender, CanExecuteRoutedEventArgs e )
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
