using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARDrone.Control.Data;

namespace ARDrone.Control.Commands
{
	public enum DronePairingMode
	{
		Pair,
		Unpair
	}

	public class PairingCommand : Command
	{
		private DronePairingMode mode;

		private String _mac;

		public PairingCommand( DronePairingMode mode, String Mac )
			: base()
		{
			this.mode = mode;
			this._mac = Mac;

			SetPrerequisitesAndOutcome();
		}

		private void SetPrerequisitesAndOutcome()
		{
			switch ( mode )
			{
				case DronePairingMode.Pair:
					prerequisites.Add( CommandStatusPrerequisite.Connected );
					prerequisites.Add( CommandStatusPrerequisite.NotFlying );
					outcome.Add( CommandStatusOutcome.ClearFlying );
					break;
				case DronePairingMode.Unpair:
					prerequisites.Add( CommandStatusPrerequisite.Connected );
					prerequisites.Add( CommandStatusPrerequisite.NotFlying );
					outcome.Add( CommandStatusOutcome.ClearFlying );
					break;
			}
		}

		public override String CreateCommand( SupportedFirmwareVersion firmwareVersion )
		{
			CheckSequenceNumber();
			// unpairing is : "00:00:00:00:00:00"
			return String.Format( "AT*CONFIG={0},{1},{2}\r", sequenceNumber, "network:owner_mac", _mac );
		}
	}
}
