using apiappVK.Models.Entity;
using apiappVK.Models.Enum;
using apiappVK.Service;
using Azure;
using DocumentFormat.OpenXml.Spreadsheet;
using ImageProcessing.DAL;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Security.Claims;
using static apiappVK.Service.UserService;

namespace apiappVK.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;
        private readonly ApplicationDbContext _db;

        public UserController(IUserService userService, ILogger<UserController> logger, ApplicationDbContext db)
        {
            _userService = userService;
            _logger = logger;
            _db = db;
        }


        [HttpGet(), Route("Get")]
        public async Task<string> Get(int id)
        {
            return await _userService.GetUser(id);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet, Route("GetUsers")]
        public async Task<List<string>> GetUsers(int pageNumber = 0, int pageSize = 10)
        {
            return await _userService.GetUsers(pageNumber, pageSize);
        }


        [HttpPost, Route("Add")]
        public async Task<bool> Add(string juser, string juserGroup)
        {
            var user = JsonConvert.DeserializeObject<User>(juser);
            var userGroup = JsonConvert.DeserializeObject<UserGroup>(juserGroup);
            var response = await _userService.AddUser(user.login, user.password, userGroup.description, userGroup.code);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                       new ClaimsPrincipal(response));
            return true;
        }

        [HttpPost, Route("Login")]
        public async Task<bool> Login(string login, string password)
        {
            var response = await _userService.Login(login, password);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                       new ClaimsPrincipal(response));
            return true;
        }

        [HttpPost]
        [Route("Logout")]
        public async Task<bool> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return true;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, Route("Delete")]
        public async Task<bool> Delete(int id)
        {
            return await _userService.DeleteUser(id);
        }

    }
}
