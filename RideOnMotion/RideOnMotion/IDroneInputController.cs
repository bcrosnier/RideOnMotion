using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace RideOnMotion
{
    /// <summary>
    /// Interface representing an input controller for a drone (<see cref="RideOnMotion.IDroneController"/>).
    /// </summary>
    /// <remarks>
    /// This can represent ie. Kinect input, joystick, gamepad, keyboard, Leap Motion (nope, not this semester) and stuff.
    /// This controller should handle input by itself.
    /// Once given the reference to a drone controller, this input controller should be able to use it to send commands to the drone.
    /// </remarks>
    public interface IDroneInputController
    {
        /// <summary>
        /// A friendly name for this Controller.
        /// </summary>
        /// <remarks>Usually, the name of the input device.</remarks>
        /// <example>"Kinect device", "Keyboard", "Xbox 360 Gamepad", "Leap Motion", etc. </example>
        string Name { get; }

        /// <summary>
        /// Bitmap property, if the input has a bitmap to send. Can and should be null if the input does not have any Bitmap to show, or if the Bitmap is no longer available.
        /// </summary>
        /// <remarks>
        /// If the input has a camera, or any other imaging source, the input controller can set its image here.
        /// </remarks>
        BitmapSource InputImageSource { get; }

        /// <summary>
        /// An observable collection of UI elements, relative to the input, that should be displayed by the user interface. Can be empty.
        /// </summary>
        /// <remarks>
        /// In the current implementation, the controller should assume its UI elements are displayed as children of a 640x480 canvas.
        /// </remarks>
        /// <example>
        /// On a Kinect device, display the two points of a hand, and/or trigger areas.
        /// Display key information on a keyboard or on a joystick, etc.
        /// </example>
        ObservableCollection<UIElement> InputUIElements { get; }

        /// <summary>
        /// Status of the input that should be connected to the controller.
        /// </summary>
        DroneInputStatus InputStatus { get; }

        /// <summary>
        /// Set the drone controller linked to this input.
        /// The IDroneController set here should be the one the commands are sent to.
        /// </summary>
        IDroneController ActiveDrone { set; }

        /// <summary>
        /// If the InputImageSource is set to a new one, this event should be fired.
        ///  (<see cref="RideOnMotion.IDroneInputController.InputImageSource"/>)
        /// </summary>
        event EventHandler<BitmapSource> InputImageSourceChanged;

        /// <summary>
        /// Fired when the status of the input device changed (<see cref="RideOnMotion.IDroneInputController.InputStatus"/>).
        /// </summary>
        event EventHandler<DroneInputStatus> InputStatusChanged;
    }

    /// <summary>
    /// Status of an input connected to an input controller.
    /// </summary>
    public enum DroneInputStatus
    {
        /// <summary>
        /// Input is disconnected, and controller cannot receive data from, or send data to it.
        /// </summary>
        Disconnected,
        /// <summary>
        /// Controller is establishing or has established a connection with the input, but isn't ready to process commands yet.
        /// </summary>
        NotReady,
        /// <summary>
        /// Controller is connected with input, input is fully operational and both can process commands.
        /// </summary>
        Ready
    }
}
