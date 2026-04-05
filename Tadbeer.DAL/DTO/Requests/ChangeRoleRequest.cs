using System.ComponentModel.DataAnnotations;
using Tadbeer.DAL.Models;

namespace Tadbeer.DAL.DTO.Requests;

public class ChangeRoleRequest
{
    [Required]
    public UserRole Role { get; set; }
}