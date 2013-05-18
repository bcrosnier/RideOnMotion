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
            //RideOnMotion.Logger.Instance.NewEntry( CK.Core.LogLevel.Trace, RideOnMotion.CKTraitTags.User, "Key : " + e.Key.ToString() );

            if ( this._drone != null )
            {
                // Drone control.
                switch( e.Key.ToString() ) {
                    // O or Enter: Take off
                    case "O":
                    case "Return":
                        this._drone.Takeoff();
                        e.Handled = true; // Halt further propagation.
                        break;

                    // Landing
                    case "P":
                    case "Space":
                        this._drone.Land();
                        e.Handled = true;
                        break;
                        
                    // LED testing
                    case "L":
                        this._drone.PlayLED();
                        e.Handled = true;
                        break;
                       
                    // Calibrate flat trim
                    case "F":
                        this._drone.FlatTrim();
                        e.Handled = true;
                        break;
                    
                    /* Navigation commands:
                     * Navigate( roll, pitch, yaw, gaz )
                     * Reminder:
                     * Roll: Translate left or right
                     * Pitch Translate ahead or backwards
                     * Yaw: Turn left or right
                     * Gaz: Propelling power (ie. elevation)
                     * */

                    /* Arranged like the number pad:
                     * A Z E  ↖ ↑ ↗
                     * Q S D  ← ⇎ →
                     * W X C  ↙ ↓ ↘
                     * */
                    case "NumPad7":
                    case "A": // Fly ahead and to the left?
                        this._drone.Navigate(-1, 1, 0, 0);
                        e.Handled = true;
                        break;
                    case "NumPad8":
                    case "Z": // Fly ahead?
				        this._drone.Navigate( 0, 1, 0, 0 );
                        e.Handled = true;
                        break;
                    case "NumPad9":
                    case "E": // Fly ahead and to the right?
				        this._drone.Navigate( 1, 1, 0, 0 );
                        e.Handled = true;
                        break;
                    case "NumPad4":
                    case "Q": // Fly left?
				        this._drone.Navigate( -1, 0, 0, 0 );
                        e.Handled = true;
                        break;
                    case "NumPad5":
                    case "S":// Stop
				        this._drone.Navigate( 0, 0, 0, 0 );
                        e.Handled = true;
                        break;
                    case "NumPad6":
                    case "D": // Fly right?
				        this._drone.Navigate( 1, 0, 0, 0 );
                        e.Handled = true;
                        break;
                    case "NumPad1":
                    case "W": // Fly backwards and to the left?
				        this._drone.Navigate( -1, -1, 0, 0 );
                        e.Handled = true;
                        break;
                    case "NumPad2":
                    case "X": // Fly backwards
				        this._drone.Navigate( 0, -1, 0, 0 );
                        e.Handled = true;
                        break;
                    case "NumPad3":
                    case "C": // Fly backwards and to the right?
				        this._drone.Navigate( 1, -1, 0, 0 );
                        e.Handled = true;
                        break;
                    default:
                        // Unknown key.
                        break;
                }
            }
        }
    }
}
