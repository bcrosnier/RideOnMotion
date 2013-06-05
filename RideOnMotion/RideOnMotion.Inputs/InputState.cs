using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideOnMotion.Inputs
{
	public class InputState
	{
		public float Roll { get; set; }
		public float Pitch { get; set; }
		public float Yaw { get; set; }
		public float Gaz { get; set; }

		public bool CameraSwap { get; set; }
		public bool TakeOff { get; set; }
		public bool Land { get; set; }
		public bool Hover { get; set; }
		public bool Emergency { get; set; }
		public bool FlatTrim { get; set; }
		public bool SpecialAction { get; set; }

		public InputState()
		{
			Roll = 0.0f; Pitch = 0.0f; Gaz = 0.0f;
			TakeOff = false; Land = false; Emergency = false; FlatTrim = false; SpecialAction = false;
		}

		public InputState( float roll, float pitch, float yaw, float gaz, bool cameraSwapButton, bool takeOffButton, bool landButton, bool hoverButton, bool emergencyButton, bool flatTrimButton, bool specialActionButton )
		{
			Roll = roll; Pitch = pitch; Yaw = yaw; Gaz = gaz;
			CameraSwap = cameraSwapButton;
			TakeOff = takeOffButton; Land = landButton; Hover = hoverButton;
			Emergency = emergencyButton; FlatTrim = flatTrimButton;
			SpecialAction = specialActionButton;
		}

		public override String ToString()
		{
			String value = "Roll: " + Roll.ToString( "0.000" ) + ", Pitch: " + Pitch.ToString( "0.000" ) + ", Yaw: " + Yaw.ToString( "0.000" ) + ", Gaz: " + Gaz.ToString( "0.000" );
			if ( CameraSwap ) { value += ", Change Camera"; }
			if ( TakeOff ) { value += ", Take Off"; }
			if ( Land ) { value += ", Land"; }
			if ( Hover ) { value += ", Hover"; }
			if ( Emergency ) { value += ", Emergency"; }
			if ( FlatTrim ) { value += ", Flat Trim"; }
			if ( SpecialAction ) { value += ", Special Action"; }

			return value;
		}
	}
}
