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


}
