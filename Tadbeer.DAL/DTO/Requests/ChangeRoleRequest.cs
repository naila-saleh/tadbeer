using System.ComponentModel.DataAnnotations;

namespace Tadbeer.DAL.DTO.Requests;

public class ChangeRoleRequest
{
    [Required]
    public string Role { get; set; }
}