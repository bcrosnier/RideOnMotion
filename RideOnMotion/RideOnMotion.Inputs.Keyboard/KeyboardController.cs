using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RideOnMotion.Inputs.Keyboard
{
    public class KeyboardController // : IDroneInputController // Fired until interface is fixed and/or valid.
    {
        DroneCommand _drone;

        public KeyboardController()
        {
        }

        public DroneCommand ActiveDrone
        {
            set
            {
                this._drone = value;
            }
        }

        /// <summary>
        /// Event triggered on key press.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// Currently, it's only valid for an AZERTY keyboard, at the character level.
        /// To use virtual keys or scancodes instead, we might want to check KeyInterop.VirtualKeyFromKey.
        /// </remarks>
        public void ProcessKey( KeyEventArgs e )
        {
            //RideOnMotion.Logger.Instance.NewEntry( CK.Core.LogLevel.Trace, RideOnMotion.CKTraitTags.User, "Coin : " + e.Key.ToString() );

            if ( this._drone != null )
            {
                // Drone control.
                switch( e.Key.ToString() ) {
                    case "O":
                    case "Return":
                        this._drone.Takeoff();
                        break;

                    case "P":
                    case "Space":
                        this._drone.Land();
                        break;
                        
                    case "L":
                        this._drone.PlayLED();
                        break;
                        
                    case "F":
                        this._drone.FlatTrim();
                        break;
                        
                    case "A":
                        this._drone.Navigate(-1, 1, 0, 0);
                        break;
                    case "Z":
				        this._drone.Navigate( 0, 1, 0, 0 );
                        break;
                    case "E":
				        this._drone.Navigate( 1, 1, 0, 0 );
                        break;
                    case "Q":
				        this._drone.Navigate( -1, 0, 0, 0 );
                        break;
                    case "S":
				        this._drone.Navigate( 0, 0, 0, 0 );
                        break;
                    case "D":
				        this._drone.Navigate( 1, 0, 0, 0 );
                        break;
                    case "W":
				        this._drone.Navigate( -1, -1, 0, 0 );
                        break;
                    case "X":
				        this._drone.Navigate( 0, -1, 0, 0 );
                        break;
                    case "C":
				        this._drone.Navigate( 1, -1, 0, 0 );
                        break;
                    default:
                        // Unknown key.
                        break;
                }
            }
        }
    }
}
