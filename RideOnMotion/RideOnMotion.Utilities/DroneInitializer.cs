using ARDrone.Capture;
using ARDrone.Control;
using ARDrone.Control.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideOnMotion.Utilities
{
	public class DroneInitializer
	{
		private DroneConfig _currentDroneConfig;
		private DroneControl _droneControl;

		private DroneCommand _droneCommand;
		private VideoRecorder _videoRecorder;
		private SnapshotRecorder _snapshotRecorder;

		public DroneCommand DroneCommand { get { return _droneCommand; } }
		public VideoRecorder VideoRecorder { get { return _videoRecorder; } }
		public SnapshotRecorder SnapshotRecorder { get { return _snapshotRecorder; } }

		public DroneInitializer()
		{
			_currentDroneConfig = new DroneConfig();

			_currentDroneConfig.StandardOwnIpAddress = "192.168.1.2";
			_currentDroneConfig.DroneIpAddress = "192.168.1.1";
			_currentDroneConfig.DroneNetworkIdentifierStart = "ardrone_006431";

			_droneControl = new DroneControl( _currentDroneConfig );

			_videoRecorder = new VideoRecorder();
			_snapshotRecorder = new SnapshotRecorder();

			_droneCommand = new DroneCommand( _droneControl, _videoRecorder );

			_droneControl.ConnectionStateChanged += droneControl_ConnectionStateChanged_Sync;
			_droneCommand.Connect();

		}

		private void droneControl_ConnectionStateChanged_Sync( object sender, DroneConnectionStateChangedEventArgs e )
		{
			if( e.Connected )
			{
				
			}
			else
			{
				int b = 0;
			}

			//_droneCommand.FlatTrim();
			_droneCommand.Takeoff();
			_droneCommand.Land();
		}
	}
}
