using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TaskApi.Common.Enums;

namespace TaskApi.Common.Contracts.Request
{
    public class UpdateTaskStatusDto
    {
        [Required]
        public int Id { get; set; }
        
        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TaskStatuses Status { get; set; }

    }
}
