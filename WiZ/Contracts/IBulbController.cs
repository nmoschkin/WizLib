using System.Drawing;
using System.Threading.Tasks;

using WiZ.Command;
using WiZ.Observable;

namespace WiZ.Contracts
{
    /// <summary>
    /// An interface that describes an object capable of controlling WiZ Connected Smart Bulbs.
    /// </summary>
    public interface IBulbController : IBulb
    {
        /// <summary>
        /// Gets or sets the brightness setting of the bulb.
        /// </summary>
        byte? Brightness { get; set; }

        /// <summary>
        /// Gets or sets the color of the light.
        /// </summary>
        Color? Color { get; set; }

        /// <summary>
        /// Gets or sets the bulb's HomeId.
        /// </summary>
        /// <remarks>
        /// This is an identifying value.
        /// </remarks>
        new int? HomeId { get; set; }

        /// <summary>
        /// Gets a value indicating the power state of the bulb.
        /// </summary>
        bool? IsPoweredOn { get; }

        /// <summary>
        /// Gets the current light mode for the bulb.
        /// </summary>
        LightMode LightMode { get; }

        /// <summary>
        /// Gets or sets the bulb's RoomId.
        /// </summary>
        /// <remarks>
        /// This is an identifying value.
        /// </remarks>
        new int? RoomId { get; set; }

        /// <summary>
        /// Gets or sets the speed of an animated light mode.
        /// </summary>
        byte? Speed { get; set; }

        /// <summary>
        /// Gets or sets the white light temperature in degress Kelvin.
        /// </summary>
        int? Temperature { get; set; }

        /// <summary>
        /// Gets or sets the timeout for connectivity.
        /// </summary>
        int Timeout { get; set; }

        /// <summary>
        /// Pulse the light with the specified settings.
        /// </summary>
        /// <param name="delta">The amount to dim.</param>
        /// <param name="pulseTime">The time of a single pulse.</param>
        void Pulse(int delta = -50, int pulseTime = 250);

        /// <summary>
        /// Set the light mode of the bulb to a solid color and brightness.
        /// </summary>
        /// <param name="c">The color to set.</param>
        /// <param name="brightness">The brightness level (10-100).</param>
        /// <returns></returns>
        Task<bool> SetLightMode(Color c, byte brightness);

        /// <summary>
        /// Set the light mode of the bulb to the specified light mode and brightness.
        /// </summary>
        /// <param name="scene">The light mode to set the bulb to.</param>
        /// <param name="brightness">The brightness level (10-100).</param>
        /// <returns></returns>
        Task<bool> SetLightMode(ILightMode scene, byte brightness);

        /// <summary>
        /// Turn the light off.
        /// </summary>
        /// <returns></returns>
        Task<bool> TurnOff();

        /// <summary>
        /// Turn the light on.
        /// </summary>
        /// <returns></returns>
        Task<bool> TurnOn();
    }
}