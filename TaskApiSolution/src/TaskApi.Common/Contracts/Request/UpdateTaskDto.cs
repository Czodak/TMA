using System.ComponentModel.DataAnnotations;

namespace TaskApi.Common.Contracts.Request
{
    public class UpdateTaskDto
    {
        [Required]
        public int Id { get; set; }

        [MaxLength(255)]
        public string? Title { get; set; }
        public string? Description { get; set; }
        
        public int? Priority { get; set; }
    }
}
