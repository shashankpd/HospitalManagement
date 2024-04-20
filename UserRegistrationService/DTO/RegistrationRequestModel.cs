using System.ComponentModel.DataAnnotations;

namespace UserManagementService.DTO
{
    public class RegistrationRequestModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }
}
