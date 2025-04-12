using System.Text.Json.Serialization;
using TaskApi.Common.Enums;

namespace TaskApi.Contracts.Request
{
    public class CreateTaskRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int CreatorId { get; set; }
        public int? CurrentlyAssignedUserId { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TaskStatuses Status { get; set; }
    }
}
