using System.ComponentModel.DataAnnotations;

namespace TaskApi.Common.Contracts.Request
{
    public class UpdateTaskAssigmentDto
    {
        [Required]
        public int Id { get; set; }

        //null = usassign user.
        public Guid? NewAssignedUser{ get; set; }
    }
}
