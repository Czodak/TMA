using System.Text.Json.Serialization;

namespace TaskApi.Common.Events.Common
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TaskEventType
    {
        Unkown,
        TaskAssignementUpdated,
        TaskRemoved,
        TaskStatusUpdated
    }
}
