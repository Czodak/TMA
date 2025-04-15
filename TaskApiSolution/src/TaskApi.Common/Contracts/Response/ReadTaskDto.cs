using TaskApi.Common.Enums;

namespace TaskApi.Common.Contracts.Response
{
    public class ReadTaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid CreatorId { get; set; }
        public Guid? CurrentlyAssignedUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public TaskStatuses Status { get; set; }
        public int Priority { get; set; }
    }
}
