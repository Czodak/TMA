using System.ComponentModel.DataAnnotations;

namespace TaskApi.Common.Contracts.Request
{
    public class UpdateTaskAssigmentDto
    {
        [Required]
        public int Id { get; set; }

        public Guid NewAssignedUser{ get; set; }
    }
}
