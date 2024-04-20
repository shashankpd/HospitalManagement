using Microsoft.AspNetCore.Mvc;
using UserManagementService.DTO;
using UserManagementService.Entity;

namespace UserManagementService.Interface
{
    public interface IRegistration
    {
        public Task<bool> RegisterUser(User userRegistration);

        public Task<string> UserLogin(string email, string password, IConfiguration configuration);

        public Task<IEnumerable<User>> GetUsersDetails();

    }
}
