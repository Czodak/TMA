using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TaskApi.Common.Enums;

namespace TaskApi.Contracts.Request
{
    public class CreateTaskRequest
    {
        [Required]
        [MaxLength(255)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [JsonIgnore]
        public Guid CreatorId { get; set; }
        
        [JsonIgnore]
        public Guid? CurrentlyAssignedUserId { get; set; }

        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TaskStatuses Status { get; set; }
    }
}
