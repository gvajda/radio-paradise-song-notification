/* 
 * Player API
 *
 * No description provided (generated by Swagger Codegen https://github.com/swagger-api/swagger-codegen)
 *
 * OpenAPI spec version: 0.1
 * 
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */

using System.ComponentModel;

namespace Foobar2000.RESTClient.Model
{
    /// <summary>
    /// Defines PlaybackState
    /// </summary>

    public enum PlaybackState
    {

        /// <summary>
        /// Enum Stopped for value: stopped
        /// </summary>

        [Description("Stopped")]
        stopped,

        /// <summary>
        /// Enum Playing for value: playing
        /// </summary>

        [Description("Playing")]
        playing,

        /// <summary>
        /// Enum Paused for value: paused
        /// </summary>

        [Description("Paused")]
        paused
    }

}
