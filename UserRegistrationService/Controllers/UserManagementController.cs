using Microsoft.AspNetCore.Mvc;
using Response;
using System.Data.SqlClient;
using UserManagement.Controllers;
using UserManagementService.DTO;
using UserManagementService.Entity;
using UserManagementService.Interface;

namespace UserManagementService.Controllers
{
    [ApiController]
    [Route("api/UserManagement")]
    public class UserManagementController : Controller
    {
        private readonly IRegistration _user;
        public UserManagementController(IRegistration user)
        {
            _user = user;
        }

        [HttpPost("SignUp/Admin")]
        public async Task<IActionResult> AdminRegistration(AdminRegistrationModel user)
        {
            try
            {
                var addedUser = await MapToEntity(user);
                if (addedUser!=null)
                {
                    var response = new ResponseModel<AdminRegistrationModel>
                    {
                        Success = true,
                        Message = "User Registration Successful"
                    };
                    return Ok(response);
                }
                else
                {

                    return BadRequest("invalid input");
                }
            }
            catch (Exception ex)
            {
                if (ex is DuplicateEmailException)
                {
                    var response = new ResponseModel<AdminRegistrationModel>
                    {
                        Success = false,
                        Message = ex.Message
                    };
                    return BadRequest(response);


                }
                else if (ex is InvalidEmailFormatException)
                {
                    var response = new ResponseModel<AdminRegistrationModel>
                    {
                        Success = false,
                        Message = ex.Message
                    };
                    return BadRequest(response);

                }
                else
                {
                    return StatusCode(500, $"An error occurred while adding the user: {ex.Message}");
                }
            }
            }
        [HttpPost("SignUp/Patient")]
        public async Task<IActionResult> PatientRegistration(PatientRegistrationModel user)
        {
            try
            {
                var addedUser = await MapToEntity(user);
                if (addedUser != null)
                {
                    var response = new ResponseModel<AdminRegistrationModel>
                    {
                        Success = true,
                        Message = "User Registration Successful"
                    };
                    return Ok(response);
                }
                else
                {

                    return BadRequest("invalid input");
                }
            }
            catch (Exception ex)
            {
                if (ex is DuplicateEmailException)
                {
                    var response = new ResponseModel<AdminRegistrationModel>
                    {
                        Success = false,
                        Message = ex.Message
                    };
                    return BadRequest(response);


                }
                else if (ex is InvalidEmailFormatException)
                {
                    var response = new ResponseModel<AdminRegistrationModel>
                    {
                        Success = false,
                        Message = ex.Message
                    };
                    return BadRequest(response);

                }
                else
                {
                    return StatusCode(500, $"An error occurred while adding the user: {ex.Message}");
                }

            }
        }
        [HttpPost("SignUp/Doctor")]
        public async Task<IActionResult> DoctorRegistration(DoctorRegistrationModel user)
        {
            try
            {
                var addedUser = await MapToEntity(user);
                if (addedUser != null)
                {
                    var response = new ResponseModel<AdminRegistrationModel>
                    {
                        Success = true,
                        Message = "User Registration Successful"
                    };
                    return Ok(response);
                }
                else
                {

                    return BadRequest("invalid input");
                }
            }
            catch (Exception ex)
            {
                if (ex is DuplicateEmailException)
                {
                    var response = new ResponseModel<AdminRegistrationModel>
                    {
                        Success = false,
                        Message = ex.Message
                    };
                    return BadRequest(response);


                }
                else if (ex is InvalidEmailFormatException)
                {
                    var response = new ResponseModel<AdminRegistrationModel>
                    {
                        Success = false,
                        Message = ex.Message
                    };
                    return BadRequest(response);

                }
                else
                {
                    return StatusCode(500, $"An error occurred while adding the user: {ex.Message}");
                }

            }
        }
        private async Task<IActionResult> MapToEntity<T>(T model)
        {
            // Validation logic for the model here

            User user = null;

            if (model is AdminRegistrationModel adminModel)
            {
                user = new User
                {
                    FirstName = adminModel.FirstName,
                    LastName = adminModel.LastName,
                    Email = adminModel.Email,
                    Password = adminModel.Password,
                    Role = "Admin"
                };
            }
            else if (model is PatientRegistrationModel patientModel)
            {
                user = new User
                {
                    FirstName = patientModel.FirstName,
                    LastName = patientModel.LastName,
                    Email = patientModel.Email,
                    Password = patientModel.Password,
                    Role = "Patient"
                };
            }
            else if (model is DoctorRegistrationModel DoctorModel)
            {
                user = new User
                {
                    FirstName = DoctorModel.FirstName,
                    LastName = DoctorModel.LastName,
                    Email = DoctorModel.Email,
                    Password = DoctorModel.Password,
                    Role = "Doctor"
                };
            }
            if (user == null)
            {
                return BadRequest("Invalid registration model type.");
            }
            var result = await _user.RegisterUser(user);
            return Ok(result);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> UserLogin(string email, string password, [FromServices] IConfiguration configuration)
        {
            try
            {
                var details = await _user.UserLogin(email, password, configuration);

                var response = new ResponseModel<string>
                {

                    Message = "Login Sucessfull",
                    Data = details

                };
                return Ok(response);

            }
            catch (Exception ex)
            {
                if (ex is NotFoundException)
                {
                    var response = new ResponseModel<User>
                    {

                        Success = false,
                        Message = ex.Message

                    };
                    return Conflict(response);
                }
                else if (ex is InvalidPasswordException)
                {
                    var response = new ResponseModel<User>
                    {

                        Success = false,
                        Message = ex.Message

                    };
                    return BadRequest(response);
                }
                else
                {
                    return BadRequest();

                }
            }
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsersDetails()
        {
            try
            {
                var details = await _user.GetUsersDetails();
                return Ok(new ResponseModel<IEnumerable<User>>
                {
                    Message = details != null && details.Any() ? "Users retrieved successfully" : "No Users found",
                    Data = details
                });
            }
            catch (Exception ex)
            {
                if (ex is SqlException)
                {
                    return BadRequest (new ResponseModel<string>
                    {
                        Success = false,
                        Message = "An error occurred while retrieving Users from the database.",
                        Data = null
                    });
                }
                else
                {
                    return BadRequest (new ResponseModel<string>
                    {
                        Success = false,
                        Message = "An error occurred.",
                        Data = null
                    });
                }
            }
        }

        [HttpGet("userid")]
        public async Task<IActionResult> GetById(int userid)
        {
            try
            {
                var details = await _user.GetById(userid);
                return Ok(details);
            }
            catch (Exception ex)
            {
                if (ex is SqlException)
                {
                    return BadRequest(new ResponseModel<string>
                    {
                        Success = false,
                        Message = "An error occurred while retrieving Users from the database.",
                        Data = null
                    });
                }
                else
                {
                    return BadRequest(new ResponseModel<string>
                    {
                        Success = false,
                        Message = "An error occurred.",
                        Data = null
                    });
                }
            }
        }


    }
}
