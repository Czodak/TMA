using System.Text.Json.Serialization;

namespace NotificationService.Events.Common
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
