using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideOnMotion.Inputs.Xbox360Gamepad
{
    public class XboxControllerStateChangedEventArgs: EventArgs
    {
        public XInputState CurrentInputState { get; set; }
        public XInputState PreviousInputState { get; set; }
    }
}
