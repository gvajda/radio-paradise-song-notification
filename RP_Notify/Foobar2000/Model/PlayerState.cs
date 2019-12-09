/* 
 * Player API
 *
 * No description provided (generated by Swagger Codegen https://github.com/swagger-api/swagger-codegen)
 *
 * OpenAPI spec version: 0.1
 * 
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Foobar2000.RESTClient.Model
{
    /// <summary>
    /// PlayerState
    /// </summary>
    [DataContract]
    public partial class PlayerState :  IEquatable<PlayerState>, IValidatableObject
    {
        /// <summary>
        /// Gets or Sets PlaybackState
        /// </summary>
        [DataMember(Name="playbackState", EmitDefaultValue=false)]
        public PlaybackState? PlaybackState { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerState" /> class.
        /// </summary>
        /// <param name="info">info.</param>
        /// <param name="activeItem">activeItem.</param>
        /// <param name="playbackState">playbackState.</param>
        /// <param name="playbackMode">playbackMode.</param>
        /// <param name="playbackModes">playbackModes.</param>
        /// <param name="volume">volume.</param>
        public PlayerState(PlayerInfo info = default(PlayerInfo), PlayerStateActiveItem activeItem = default(PlayerStateActiveItem), PlaybackState? playbackState = default(PlaybackState?), decimal? playbackMode = default(decimal?), List<string> playbackModes = default(List<string>), PlayerStateVolume volume = default(PlayerStateVolume))
        {
            this.Info = info;
            this.ActiveItem = activeItem;
            this.PlaybackState = playbackState;
            this.PlaybackMode = playbackMode;
            this.PlaybackModes = playbackModes;
            this.Volume = volume;
        }
        
        /// <summary>
        /// Gets or Sets Info
        /// </summary>
        [DataMember(Name="info", EmitDefaultValue=false)]
        public PlayerInfo Info { get; set; }

        /// <summary>
        /// Gets or Sets ActiveItem
        /// </summary>
        [DataMember(Name="activeItem", EmitDefaultValue=false)]
        public PlayerStateActiveItem ActiveItem { get; set; }


        /// <summary>
        /// Gets or Sets PlaybackMode
        /// </summary>
        [DataMember(Name="playbackMode", EmitDefaultValue=false)]
        public decimal? PlaybackMode { get; set; }

        /// <summary>
        /// Gets or Sets PlaybackModes
        /// </summary>
        [DataMember(Name="playbackModes", EmitDefaultValue=false)]
        public List<string> PlaybackModes { get; set; }

        /// <summary>
        /// Gets or Sets Volume
        /// </summary>
        [DataMember(Name="volume", EmitDefaultValue=false)]
        public PlayerStateVolume Volume { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class PlayerState {\n");
            sb.Append("  Info: ").Append(Info).Append("\n");
            sb.Append("  ActiveItem: ").Append(ActiveItem).Append("\n");
            sb.Append("  PlaybackState: ").Append(PlaybackState).Append("\n");
            sb.Append("  PlaybackMode: ").Append(PlaybackMode).Append("\n");
            sb.Append("  PlaybackModes: ").Append(PlaybackModes).Append("\n");
            sb.Append("  Volume: ").Append(Volume).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
  
        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public virtual string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="input">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object input)
        {
            return this.Equals(input as PlayerState);
        }

        /// <summary>
        /// Returns true if PlayerState instances are equal
        /// </summary>
        /// <param name="input">Instance of PlayerState to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(PlayerState input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.Info == input.Info ||
                    (this.Info != null &&
                    this.Info.Equals(input.Info))
                ) && 
                (
                    this.ActiveItem == input.ActiveItem ||
                    (this.ActiveItem != null &&
                    this.ActiveItem.Equals(input.ActiveItem))
                ) && 
                (
                    this.PlaybackState == input.PlaybackState ||
                    (this.PlaybackState != null &&
                    this.PlaybackState.Equals(input.PlaybackState))
                ) && 
                (
                    this.PlaybackMode == input.PlaybackMode ||
                    (this.PlaybackMode != null &&
                    this.PlaybackMode.Equals(input.PlaybackMode))
                ) && 
                (
                    this.PlaybackModes == input.PlaybackModes ||
                    this.PlaybackModes != null &&
                    this.PlaybackModes.SequenceEqual(input.PlaybackModes)
                ) && 
                (
                    this.Volume == input.Volume ||
                    (this.Volume != null &&
                    this.Volume.Equals(input.Volume))
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hashCode = 41;
                if (this.Info != null)
                    hashCode = hashCode * 59 + this.Info.GetHashCode();
                if (this.ActiveItem != null)
                    hashCode = hashCode * 59 + this.ActiveItem.GetHashCode();
                if (this.PlaybackState != null)
                    hashCode = hashCode * 59 + this.PlaybackState.GetHashCode();
                if (this.PlaybackMode != null)
                    hashCode = hashCode * 59 + this.PlaybackMode.GetHashCode();
                if (this.PlaybackModes != null)
                    hashCode = hashCode * 59 + this.PlaybackModes.GetHashCode();
                if (this.Volume != null)
                    hashCode = hashCode * 59 + this.Volume.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// To validate all properties of the instance
        /// </summary>
        /// <param name="validationContext">Validation context</param>
        /// <returns>Validation Result</returns>
        IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            yield break;
        }
    }

}
