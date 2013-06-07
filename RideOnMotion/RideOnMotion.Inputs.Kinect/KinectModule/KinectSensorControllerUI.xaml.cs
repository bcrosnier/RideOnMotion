using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RideOnMotion.Inputs.Kinect
{
    /// <summary>
    /// Kinect UI overlay, with a Canvas, hand points, buttons, et cetera, et cetera.
    /// Interaction logic for KinectSensorControllerUI.xaml
    /// </summary>
    public partial class KinectSensorControllerUI : UserControl, INotifyPropertyChanged
    {
        private KinectSensorController _controller;

        private System.Windows.Point _leftHandPoint = new System.Windows.Point( -1, -1 );
        private System.Windows.Point _rightHandPoint = new System.Windows.Point( -1, -1 );

        private Visibility _handsVisibility = Visibility.Collapsed;

        private string _alertMessage;
        private Visibility _alertVisibility = Visibility.Collapsed;
        private Task _alertTask;
        private bool _alertIsPersistent;

        private static readonly int ALERT_BLINK_TIME = 500; // Time between alert visibility toggles, in ms.
        private static readonly int ALERT_BLINK_COUNT = 5; // Number of times the alert goes on and off.

        private CancellationTokenSource _alertCancellationTokenSource = new CancellationTokenSource();

        public int LeftHandX
        {
            get { return (int)(this._leftHandPoint.X - 7.5); }
        }

        public int LeftHandY
        {
            get { return (int)(this._leftHandPoint.Y - 7.5); }
        }

        public int RightHandX
        {
            get { return (int)(this._rightHandPoint.X - 7.5); }
        }

        public int RightHandY
        {
            get { return (int)(this._rightHandPoint.Y - 7.5); }
        }

        public Visibility HandsVisibility
        {
			get
			{
				return _handsVisibility;
			}
        }

		public KinectSensor KinectSensor
		{
			get
			{
				return _controller.Sensor;
			}
		}

        public ObservableCollection<ICaptionArea> TriggerButtons
        {
            get
            {
                return this._controller.TriggerButtons;
            }
        }

        public KinectSensorControllerUI(KinectSensorController controller)
        {
            this._controller = controller;
            this.DataContext = this;

			this._controller.HandsPointReady += OnHandsPoint;

			InitializeComponent();
        }

        public string AlertMessage
        {
            get
            {
                return _alertMessage;
            }

            set
            {
                this._alertMessage = value;
                this.OnNotifyPropertyChange( "AlertMessage" );
            }
        }

        public Visibility AlertVisibility
        {
            get
            {
                return _alertVisibility;
            }

            set
            {
                this._alertVisibility = value;
                this.OnNotifyPropertyChange( "AlertVisibility" );
            }
        }

        public void ClearAlert()
        {
            if ( _alertTask != null && !_alertTask.IsCompleted )
            {
                _alertCancellationTokenSource.Cancel();
                _alertCancellationTokenSource = new CancellationTokenSource();
            }
        }

        public void FireAlert(string message, bool isPersistent)
        {
            ClearAlert();

            _alertIsPersistent = isPersistent;
            this.AlertMessage = message;

            CancellationToken ct = _alertCancellationTokenSource.Token;

            _alertTask = Task.Factory.StartNew( () =>
            {
                bool persistent = _alertIsPersistent;
                int i = ALERT_BLINK_COUNT;

                while( true )
                {
                    if ( ct.IsCancellationRequested )
                    {
                        this.AlertVisibility = System.Windows.Visibility.Hidden;
                        break;
                    }
                    this.AlertVisibility = System.Windows.Visibility.Visible;
                    System.Threading.Thread.Sleep( ALERT_BLINK_TIME );

                    if ( ct.IsCancellationRequested )
                    {
                        this.AlertVisibility = System.Windows.Visibility.Hidden;
                        break;
                    }
                    this.AlertVisibility = System.Windows.Visibility.Hidden;
                    System.Threading.Thread.Sleep( ALERT_BLINK_TIME );

                    if ( !persistent && --i == 0 )
                    {
                        break;
                    }

                }
            }, _alertCancellationTokenSource.Token );
        }

        public void FireAlert( string message )
        {
            FireAlert( message, false );
        }

        private void OnHandsPoint( object sender, System.Windows.Point[] e )
        {
            this._rightHandPoint = e[0];
            this._leftHandPoint = e[1];

			if ( this._rightHandPoint.Y != -1.0 && _handsVisibility == Visibility.Collapsed )
			{
				_handsVisibility = Visibility.Visible;
				Logger.Instance.NewEntry( CKLogLevel.Trace, CKTraitTags.User, "Hands visible" );
			}
			else if ( this._rightHandPoint.Y == -1.0 && _handsVisibility == Visibility.Visible )
			{
				_handsVisibility = Visibility.Collapsed;
				Logger.Instance.NewEntry( CKLogLevel.Trace, CKTraitTags.User, "Hands not visible" );
			}

            this.OnNotifyPropertyChange( "LeftHandX" );
            this.OnNotifyPropertyChange( "LeftHandY" );
            this.OnNotifyPropertyChange( "RightHandX" );
            this.OnNotifyPropertyChange( "RightHandY" );
            this.OnNotifyPropertyChange( "HandsVisibility" );
        }
        
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
    }
}
