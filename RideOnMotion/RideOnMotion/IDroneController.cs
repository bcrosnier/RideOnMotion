using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideOnMotion
{
    /// <summary>
    /// Class controlling the standard moves of a three-axis quadricopter drone.
    /// Should handle the connection with the drone, process and send commands to it,
    /// and eventually report back the status of the drone through events or something.
    /// </summary>
    public interface IDroneController
    {
        // What a beautiful stub !

        /// <summary>
        /// Status of the drone that should be connected to the controller.
        /// </summary>
        DroneStatus DroneStatus { get; }
        
        /// <summary>
        /// Fired when the status of the drone changed (<see cref="RideOnMotion.IDroneController.DroneStatus"/>).
        /// </summary>
        event EventHandler<DroneStatus> DroneStatusChanged;

        /// <summary>
        /// Commands the drone to move on a axis at a given speed.
        /// </summary>
        /// <param name="axis">Axis to move on.</param>
        /// <param name="speed">Speed AND direction to move the drone on:
        /// Ranges from -1.0 to 1.0.
        /// -1.0 / 1.0 being full speed on either direction (implementation and hardware decides what full speed and direction mean),
        /// 0 being a command stop moving on this axis.</param>
        /// <remarks>
        /// Drone should move on the axis at the determined speed/direction until asked otherwise; but the implementation
        /// can determine timeout and safety measures as it sees fit.
        /// If the drone is disconnected or not ready, this should do nothing.
        /// </remarks>
        /// <example>
        /// IDroneController.Move( DroneAxes.Pitch, 1.0 ); : Command drone to increase pitch angle : move forward at full speed.
        /// IDroneController.Move( DroneAxes.Elevation, -1.0 ); : Command drone to reduce elevation at full speed. Use sparingly (and preferably not for too long, crashes and all that).
        /// </example>
        void Move( DroneAxes axis, double speed );

        /// <summary>
        /// Sends a drone a single command.
        /// </summary>
        /// <remarks>
        /// If the drone is disconnected or not ready, this should do nothing.
        /// </remarks>
        /// <param name="command">Command to send: <see cref="RideOnMotion.DroneSingleCommands"/></param>
        void Command( DroneSingleCommands command );
    }

    /// <summary>
    /// Standard single commands without values (ie. button push) for a connected drone.
    /// </summary>
    public enum DroneSingleCommands
    {
        /// <summary>
        /// Command the drone to leave the ground and take off to the skies.
        /// Or the roof, whichever is closer.
        /// </summary>
        TakeOff,
        /// <summary>
        /// Command to safely land the drone on the ground, preferably at a speed
        /// that doesn't cause it to kill whoever's below (and break the drone).
        /// </summary>
        Land,
        /// <summary>
        /// Shut down everything! Stop all movable motors.
        /// This can and will obviously lead to a very unsafe landing (eg. a crash).
        /// </summary>
        Emergency
    }

    /// <summary>
    /// Standard drone movable axes, used to make the drone move. <see cref="RideOnMotion.IDroneController.Move(DroneAxes axis, double speed)"/>
    /// </summary>
    public enum DroneAxes
    {
        /// <summary>
        /// Pitch axis. Drone rotates on the X axis.
        /// </summary>
        /// <remarks>
        /// On a quadricopter, this causes the drone to move forward -- or backwards -- on the Z axis.
        /// </remarks>
        Pitch,
        /// <summary>
        /// Yaw axis. Drone rotates on the Y axis.
        /// </summary>
        /// <remarks>
        /// On a quadricopter, this causes the drone to turn on itself, clockwise or counter-clockwise.
        /// </remarks>
        Yaw,
        /// <summary>
        /// Roll axis. Drone rotates on the Z axis.
        /// </summary>
        /// <remarks>
        /// On a quadricopter, this causes the drone to move left or right, on the X axis.
        /// </remarks>
        Roll,
        /// <summary>
        /// Drone elevation.
        /// </summary>
        /// <remarks>
        /// On a quadricopter, controls the motors to set the elevation on the Y axis.
        /// Causes the drone to elevate to the skies - or to eat the dirt. If you want to land safely, <see cref="RideOnMotion.DroneSingleCommands.Land"/>.
        /// </remarks>
        Elevation
    }

    /// <summary>
    /// Status of a drone connected to a drone controller.
    /// </summary>
    public enum DroneStatus
    {
        /// <summary>
        /// Drone is disconnected, and controller cannot receive data from, or send data to it.
        /// </summary>
        Disconnected,
        /// <summary>
        /// Controller is establishing or has established a connection with the drone, but isn't ready to process commands yet.
        /// </summary>
        NotReady,
        /// <summary>
        /// Controller is connected with the drone, which is fully operational. Drone can move around freely until it dies a painful death (or is disconnected).
        /// </summary>
        Ready
    }
}
