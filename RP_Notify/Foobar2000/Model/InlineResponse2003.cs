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
using System.Runtime.Serialization;
using System.Text;

namespace Foobar2000.RESTClient.Model
{
    /// <summary>
    /// InlineResponse2003
    /// </summary>
    [DataContract]
    public partial class InlineResponse2003 :  IEquatable<InlineResponse2003>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InlineResponse2003" /> class.
        /// </summary>
        /// <param name="player">player.</param>
        /// <param name="playlists">playlists.</param>
        /// <param name="playlistItems">playlistItems.</param>
        public InlineResponse2003(PlayerState player = default(PlayerState), PlaylistsResult playlists = default(PlaylistsResult), PlaylistItemsResult playlistItems = default(PlaylistItemsResult))
        {
            this.Player = player;
            this.Playlists = playlists;
            this.PlaylistItems = playlistItems;
        }
        
        /// <summary>
        /// Gets or Sets Player
        /// </summary>
        [DataMember(Name="player", EmitDefaultValue=false)]
        public PlayerState Player { get; set; }

        /// <summary>
        /// Gets or Sets Playlists
        /// </summary>
        [DataMember(Name="playlists", EmitDefaultValue=false)]
        public PlaylistsResult Playlists { get; set; }

        /// <summary>
        /// Gets or Sets PlaylistItems
        /// </summary>
        [DataMember(Name="playlistItems", EmitDefaultValue=false)]
        public PlaylistItemsResult PlaylistItems { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class InlineResponse2003 {\n");
            sb.Append("  Player: ").Append(Player).Append("\n");
            sb.Append("  Playlists: ").Append(Playlists).Append("\n");
            sb.Append("  PlaylistItems: ").Append(PlaylistItems).Append("\n");
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
            return this.Equals(input as InlineResponse2003);
        }

        /// <summary>
        /// Returns true if InlineResponse2003 instances are equal
        /// </summary>
        /// <param name="input">Instance of InlineResponse2003 to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(InlineResponse2003 input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.Player == input.Player ||
                    (this.Player != null &&
                    this.Player.Equals(input.Player))
                ) && 
                (
                    this.Playlists == input.Playlists ||
                    (this.Playlists != null &&
                    this.Playlists.Equals(input.Playlists))
                ) && 
                (
                    this.PlaylistItems == input.PlaylistItems ||
                    (this.PlaylistItems != null &&
                    this.PlaylistItems.Equals(input.PlaylistItems))
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
                if (this.Player != null)
                    hashCode = hashCode * 59 + this.Player.GetHashCode();
                if (this.Playlists != null)
                    hashCode = hashCode * 59 + this.Playlists.GetHashCode();
                if (this.PlaylistItems != null)
                    hashCode = hashCode * 59 + this.PlaylistItems.GetHashCode();
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
