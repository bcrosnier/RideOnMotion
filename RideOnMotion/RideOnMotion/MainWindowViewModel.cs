namespace KinectInfo
{
	using System.ComponentModel;

	/// <summary>
	/// Class MainWindowViewModel
	/// </summary>
	public class MainWindowViewModel : INotifyPropertyChanged
	{
		/// <summary>
		/// Sensor Status
		/// </summary>
		private string sensorStatusValue;

		/// <summary>
		/// Connection ID
		/// </summary>
		private string connectionIDValue;

		/// <summary>
		/// Unique Device ID
		/// </summary>
		private string deviceIdValue;

		/// <summary>
		/// Is Color Stream Enabled
		/// </summary>
		private bool isColorStreamEnabledValue;

		/// <summary>
		/// Is Depth Stream Enabled
		/// </summary>
		private bool isDepthStreamEnabledValue;

		/// <summary>
		/// Is Skeleton Stream Enabled
		/// </summary>
		private bool isSkeletonStreamEnabledValue;

		/// <summary>
		/// sensor elevation angle
		/// </summary>
		private int sensorAngleValue;

		/// <summary>
		/// Can Start the sensor
		/// </summary>
		private bool canStartValue;

		/// <summary>
		/// Can Stop the sensor
		/// </summary>
		private bool canStopValue;

		/// <summary>
		/// Occurs when [property changed].
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Gets or sets the connection ID.
		/// </summary>
		/// <value>The connection ID.</value>
		public string ConnectionID
		{
			get
			{
				return this.connectionIDValue;
			}

			set
			{
				if ( this.connectionIDValue != value )
				{
					this.connectionIDValue = value;
					this.OnNotifyPropertyChange( "ConnectionID" );
				}
			}
		}

		/// <summary>
		/// Gets or sets the device ID.
		/// </summary>
		/// <value>The device ID.</value>
		public string DeviceID
		{
			get
			{
				return this.deviceIdValue;
			}

			set
			{
				if ( this.deviceIdValue != value )
				{
					this.deviceIdValue = value;
					this.OnNotifyPropertyChange( "DeviceID" );
				}
			}
		}

		/// <summary>
		/// Gets or sets the sensor status.
		/// </summary>
		/// <value>The sensor status.</value>
		public string SensorStatus
		{
			get
			{
				return this.sensorStatusValue;
			}

			set
			{
				if ( this.sensorStatusValue != value )
				{
					this.sensorStatusValue = value;
					this.OnNotifyPropertyChange( "SensorStatus" );
				}
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance is color stream enabled.
		/// </summary>
		/// <value><c>true</c> if this instance is color stream enabled; otherwise, <c>false</c>.</value>
		public bool IsColorStreamEnabled
		{
			get
			{
				return this.isColorStreamEnabledValue;
			}

			set
			{
				if ( this.isColorStreamEnabledValue != value )
				{
					this.isColorStreamEnabledValue = value;
					this.OnNotifyPropertyChange( "IsColorStreamEnabled" );
				}
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance is depth stream enabled.
		/// </summary>
		/// <value><c>true</c> if this instance is depth stream enabled; otherwise, <c>false</c>.</value>
		public bool IsDepthStreamEnabled
		{
			get
			{
				return this.isDepthStreamEnabledValue;
			}

			set
			{
				if ( this.isDepthStreamEnabledValue != value )
				{
					this.isDepthStreamEnabledValue = value;
					this.OnNotifyPropertyChange( "IsDepthStreamEnabled" );
				}
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance is skeleton stream enabled.
		/// </summary>
		/// <value><c>true</c> if this instance is skeleton stream enabled; otherwise, <c>false</c>.</value>
		public bool IsSkeletonStreamEnabled
		{
			get
			{
				return this.isSkeletonStreamEnabledValue;
			}

			set
			{
				if ( this.isSkeletonStreamEnabledValue != value )
				{
					this.isSkeletonStreamEnabledValue = value;
					this.OnNotifyPropertyChange( "IsSkeletonStreamEnabled" );
				}
			}
		}

		/// <summary>
		/// Gets or sets the sensor angle.
		/// </summary>
		/// <value>The sensor angle.</value>
		public int SensorAngle
		{
			get
			{
				return this.sensorAngleValue;
			}

			set
			{
				if ( this.sensorAngleValue != value )
				{
					this.sensorAngleValue = value;
					this.OnNotifyPropertyChange( "SensorAngle" );
				}
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance can start.
		/// </summary>
		/// <value><c>true</c> if this instance can start; otherwise, <c>false</c>.</value>
		public bool CanStart
		{
			get
			{
				return this.canStartValue;
			}

			set
			{
				if ( this.canStartValue != value )
				{
					this.canStartValue = value;
					this.OnNotifyPropertyChange( "CanStart" );
				}
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance can stop.
		/// </summary>
		/// <value><c>true</c> if this instance can stop; otherwise, <c>false</c>.</value>
		public bool CanStop
		{
			get
			{
				return this.canStopValue;
			}

			set
			{
				if ( this.canStopValue != value )
				{
					this.canStopValue = value;
					this.OnNotifyPropertyChange( "CanStop" );
				}
			}
		}

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
