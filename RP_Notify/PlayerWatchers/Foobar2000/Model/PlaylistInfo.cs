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
    /// PlaylistInfo
    /// </summary>
    [DataContract]
    public partial class PlaylistInfo :  IEquatable<PlaylistInfo>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlaylistInfo" /> class.
        /// </summary>
        /// <param name="id">id.</param>
        /// <param name="index">index.</param>
        /// <param name="title">title.</param>
        /// <param name="isCurrent">isCurrent.</param>
        /// <param name="itemCount">itemCount.</param>
        /// <param name="totalTime">totalTime.</param>
        public PlaylistInfo(string id = default(string), decimal? index = default(decimal?), string title = default(string), bool? isCurrent = default(bool?), decimal? itemCount = default(decimal?), decimal? totalTime = default(decimal?))
        {
            this.Id = id;
            this.Index = index;
            this.Title = title;
            this.IsCurrent = isCurrent;
            this.ItemCount = itemCount;
            this.TotalTime = totalTime;
        }
        
        /// <summary>
        /// Gets or Sets Id
        /// </summary>
        [DataMember(Name="id", EmitDefaultValue=false)]
        public string Id { get; set; }

        /// <summary>
        /// Gets or Sets Index
        /// </summary>
        [DataMember(Name="index", EmitDefaultValue=false)]
        public decimal? Index { get; set; }

        /// <summary>
        /// Gets or Sets Title
        /// </summary>
        [DataMember(Name="title", EmitDefaultValue=false)]
        public string Title { get; set; }

        /// <summary>
        /// Gets or Sets IsCurrent
        /// </summary>
        [DataMember(Name="isCurrent", EmitDefaultValue=false)]
        public bool? IsCurrent { get; set; }

        /// <summary>
        /// Gets or Sets ItemCount
        /// </summary>
        [DataMember(Name="itemCount", EmitDefaultValue=false)]
        public decimal? ItemCount { get; set; }

        /// <summary>
        /// Gets or Sets TotalTime
        /// </summary>
        [DataMember(Name="totalTime", EmitDefaultValue=false)]
        public decimal? TotalTime { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class PlaylistInfo {\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  Index: ").Append(Index).Append("\n");
            sb.Append("  Title: ").Append(Title).Append("\n");
            sb.Append("  IsCurrent: ").Append(IsCurrent).Append("\n");
            sb.Append("  ItemCount: ").Append(ItemCount).Append("\n");
            sb.Append("  TotalTime: ").Append(TotalTime).Append("\n");
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
            return this.Equals(input as PlaylistInfo);
        }

        /// <summary>
        /// Returns true if PlaylistInfo instances are equal
        /// </summary>
        /// <param name="input">Instance of PlaylistInfo to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(PlaylistInfo input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.Id == input.Id ||
                    (this.Id != null &&
                    this.Id.Equals(input.Id))
                ) && 
                (
                    this.Index == input.Index ||
                    (this.Index != null &&
                    this.Index.Equals(input.Index))
                ) && 
                (
                    this.Title == input.Title ||
                    (this.Title != null &&
                    this.Title.Equals(input.Title))
                ) && 
                (
                    this.IsCurrent == input.IsCurrent ||
                    (this.IsCurrent != null &&
                    this.IsCurrent.Equals(input.IsCurrent))
                ) && 
                (
                    this.ItemCount == input.ItemCount ||
                    (this.ItemCount != null &&
                    this.ItemCount.Equals(input.ItemCount))
                ) && 
                (
                    this.TotalTime == input.TotalTime ||
                    (this.TotalTime != null &&
                    this.TotalTime.Equals(input.TotalTime))
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
                if (this.Id != null)
                    hashCode = hashCode * 59 + this.Id.GetHashCode();
                if (this.Index != null)
                    hashCode = hashCode * 59 + this.Index.GetHashCode();
                if (this.Title != null)
                    hashCode = hashCode * 59 + this.Title.GetHashCode();
                if (this.IsCurrent != null)
                    hashCode = hashCode * 59 + this.IsCurrent.GetHashCode();
                if (this.ItemCount != null)
                    hashCode = hashCode * 59 + this.ItemCount.GetHashCode();
                if (this.TotalTime != null)
                    hashCode = hashCode * 59 + this.TotalTime.GetHashCode();
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
