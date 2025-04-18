﻿namespace TaskApi.Common.Contracts.Response
{
    public class ReadTaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int CreatorId { get; set; }
        public int? CurrentlyAssignedUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public string Status { get; set; }
        public int Priority { get; set; }
    }
}
