using Dapper;
using Microsoft.IdentityModel.Tokens;
using Repository.Context;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using UserManagementService.DTO;
using UserManagementService.Entity;
using UserManagementService.Interface;

namespace UserManagementService.Service
{
    public class RegistrationService : IRegistration
    {
        public static String Otp;
        public static string Email;
        public readonly DapperContext _context;
        public RegistrationService(DapperContext context)
        {
            _context = context;
        }

        public async Task<bool> RegisterUser(User userRegistration)
        {
            var parametersToCheckEmailIsValid = new DynamicParameters();
            parametersToCheckEmailIsValid.Add("Email", userRegistration.Email, DbType.String);
            var querytoCheckEmailIsNotDuplicate = @"SELECT COUNT(*) FROM Users WHERE Email = @Email;";
            var query = @" INSERT INTO Users(FirstName, LastName, Email, Password,Role) VALUES (@FirstName, @LastName, @Email, @Password,@Role);";
            var parameters = new DynamicParameters();
            parameters.Add("FirstName", userRegistration.FirstName, DbType.String);
            parameters.Add("LastName", userRegistration.LastName, DbType.String);
            parameters.Add("Email", userRegistration.Email, DbType.String);
            string hashedPassword = HashPassword(userRegistration.Password);
            parameters.Add("Password", hashedPassword, DbType.String);
            parameters.Add("Role", userRegistration.Role, DbType.String);
            using (var connection = _context.CreateConnection())
            {
                // Check if table exists
                bool tableExists = await connection.QueryFirstOrDefaultAsync<bool>(@" SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users';");
                // Create table if it doesn't exist
                if (!tableExists)
                {
                    await connection.ExecuteAsync(@" CREATE TABLE Users(
                                 UserID INT IDENTITY(1, 1) PRIMARY KEY,
                                 FirstName NVARCHAR(50) NOT NULL,
                                 LastName NVARCHAR(50) NOT NULL,
                                 Email NVARCHAR(100) UNIQUE NOT NULL,
                                 Password NVARCHAR(100) NOT NULL,
                                 Role NVARCHAR(50) CHECK (Role IN ('Admin', 'Doctor', 'Patient')) NOT NULL,
                                 IsApproved BIT DEFAULT 0 NOT NULL);");
                }
                // Check if email already exists
                bool emailExists = await connection.QueryFirstOrDefaultAsync<bool>(querytoCheckEmailIsNotDuplicate, parametersToCheckEmailIsValid);
                if (emailExists)
                {
                    throw new Exception("Email address is already in use");
                }

                // Insert new user
                var result = await connection.ExecuteAsync(query, parameters);
                return result > 0;
            }


        }

        // Hash the password using a secure hashing algorithm like SHA256
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        public async Task<string> UserLogin(string email, string password, IConfiguration configuration)
        {
            try
            {
                var query = "SELECT * FROM Users WHERE Email=@Email";
                using (var connection = _context.CreateConnection())
                {
                    var user = await connection.QueryFirstOrDefaultAsync<User>(query, new { Email = email });

                    // Check if user exists
                    if (user != null)
                    {
                        // Hash the provided password
                        string hashedPassword = HashPassword(password);

                        // Compare the hashed password with the stored hashed password
                        if (hashedPassword == user.Password)
                        {
                            // Generate JWT token
                            var token = GenerateJwtToken(user, configuration);
                            return token;
                        }
                    }

                    // Authentication failed
                    throw new UnauthorizedAccessException("Invalid email or password.");
                }
            }
            catch (Exception ex)
            {
                // Log any exceptions
                Console.WriteLine($"An error occurred while attempting login for user with email: {email}. Error: {ex.Message}");
                throw;
            }
        }



        // Generating JWT Tokens    
        private string GenerateJwtToken(User user, IConfiguration configuration)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]);
            var issuer = configuration["Jwt:Issuer"];
            var audience = configuration["Jwt:Audience"];

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                new Claim(ClaimTypes.NameIdentifier, user.Email),
                new Claim("UserId", user.UserID.ToString()),
                 new Claim(ClaimTypes.Role, user.Role) 
                    // Add more claims as needed
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = issuer,
                Audience = audience
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<IEnumerable<User>> GetUsersDetails()
        {
            try
            {
                var query = "SELECT * FROM Users";
                using (var connection = _context.CreateConnection())
                {
                    var users = await connection.QueryAsync<User>(query);
                    return users.AsList();
                }
            }
            catch (Exception ex)
            {
                // Log the exception message to the console or handle it as per your requirement
                Console.WriteLine($"Error occurred while getting registration details: {ex.Message}");
                throw;
            }
        }




    }
}
