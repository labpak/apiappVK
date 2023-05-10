using apiappVK.Models.Entity;
using apiappVK.Models.Enum;
using Newtonsoft.Json;
using System.Security.Claims;
using static apiappVK.Service.UserService;

namespace apiappVK.Service
{
    public interface IUserService
    {
        Task<string> GetUser(int id);
        Task<List<string>> GetUsers(int pageNumber, int pageSize);
        Task<ClaimsIdentity> AddUser(string login, string password, string description, GroupCode code);
        Task<ClaimsIdentity> Login(string login, string password);
        Task<bool> DeleteUser(int id);            
    }
}
