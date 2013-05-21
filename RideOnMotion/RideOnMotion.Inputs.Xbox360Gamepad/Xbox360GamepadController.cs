using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideOnMotion.Inputs.Xbox360Gamepad
{
	class Xbox360GamepadController
	{
		volatile bool _keepRunning;
		XboxController _selectedController;

		public void start()
		{
			_selectedController = XboxController.RetrieveController( 0 );
			_selectedController.StateChanged += _selectedController_StateChanged;
			XboxController.StartPolling();
		}

		private void _selectedController_StateChanged( object sender, XboxControllerStateChangedEventArgs e )
		{
			SendCommand();
		}

		private void SendCommand()
		{
			RideOnMotion.Logger.Instance.NewEntry( CKLogLevel.Trace, CKTraitTags.User, "X:" + _selectedController.RightThumbStick.X.ToString() + " Y:" + _selectedController.RightThumbStick.Y.ToString() );
		}
		public void Close()
		{
			XboxController.StopPolling();
		}
	}
}
