using static System.Net.Mime.MediaTypeNames;
using apiappVK.Models.Enum;
using ImageProcessing.DAL;
using apiappVK.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NuGet.Protocol;
using System.Security.Cryptography;
using Npgsql;
using Microsoft.AspNetCore.Mvc;
using Azure;
using System.Security.Claims;

namespace apiappVK.Service
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _db;
        public UserService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<string> GetUser(int id)
        {
            try
            {
                var user = await _db.users.FirstOrDefaultAsync(p => p.id == id);
                if (user == null)
                {
                    return new string("Пользователь не найден") { };//......
                }

                var userGroup = await _db.usergroup.FromSqlRaw<UserGroup>($"SELECT * FROM usergroup where usergroup.id = {user.user_group_id}").ToListAsync();
                var userState = await _db.userstate.FromSqlRaw<UserState>($"SELECT * FROM userstate where userstate.id = {user.user_state_id}").ToListAsync();

                var jsonOut = new JsonOutput
                {
                    _user = user,
                    _userGroup = userGroup[0],
                    _userState = userState[0]
                };
                
                var res = JsonConvert.SerializeObject(jsonOut);
                
                return res;

            }
            catch (Exception ex)
            {
                return new string($"[GetUser] : {ex.Message}"){ }; //...... 
            }
        }

        public async Task<List<string>> GetUsers(int pageNumber, int pageSize)
        {
            List<string> listJson = new List<string>();
            try
            {
                var users = _db.users.ToList().Skip(pageSize * pageNumber).Take(pageSize);
                if (users.Count() == 0)
                {
                    listJson.Add(new string("Пользователи не найдены") { });
                    return listJson;
                }

                foreach (var user in users)
                {
                    var userGroup = await _db.usergroup.FromSqlRaw<UserGroup>($"SELECT * FROM usergroup where usergroup.id = {user.user_group_id}").ToListAsync();
                    var userState = await _db.userstate.FromSqlRaw<UserState>($"SELECT * FROM userstate where userstate.id = {user.user_state_id}").ToListAsync();
                    var jsonOut = new JsonOutput
                    {
                        _user = user,
                        _userGroup = userGroup[0],
                        _userState = userState[0]
                    };
                    var res = JsonConvert.SerializeObject(jsonOut);
                    listJson.Add(res);
                }

                return listJson;
            }
            catch (Exception ex)
            {
                listJson.Clear();
                listJson.Add(new string($"[GetUser] : {ex.Message}") { });
                return listJson;
            }
        }

        //await Task.Delay(milliseconds)
        public async Task<ClaimsIdentity> AddUser(string login, string password, string description, GroupCode code)
        {
             var entity = await _db.users.OrderBy(x=>x.created_date).LastOrDefaultAsync(p => p.login == login);
             var admin = await _db.users.FromSqlRaw<User>
                ($"SELECT users.id,users.login,users.password,users.created_date,users.user_group_id,users.user_state_id FROM users " +
                $"  JOIN usergroup on users.user_group_id = usergroup.id WHERE usergroup.Code = 0").ToListAsync();
            try
            {
                if (code == 0 && admin.Count() > 0)//Пользователь с ролью администратор уже существует
                    throw new Exception("admin exist");

                if (login == entity?.login)
                { 
                    if (DateTime.Now.Subtract(entity.created_date).Seconds < 5) throw new Exception("wait");
                }
                //{"id":95,"login":"manuleee","password":"1234","created_date":"2023-05-10T00:00:00","user_group_id":19,"user_state_id":19}{"id":19,"code":1,"description":"admin"}
               
                //if (login == entity?.login)//пользователь уже существует
                //{
                //    return false;
                //}

                var newUser = new User()
                {
                    login = login,
                    password = password,
                    created_date = DateTime.Now
                };

                await _db.users.AddAsync(newUser);
                await _db.SaveChangesAsync();

                var newUserGCode = new UserGroup()
                {
                    id = newUser.user_group_id,
                    code = code,
                    description = description
                };
                var newUserSCode = new UserState()
                {
                    id = newUser.user_state_id,
                    code = StateCode.Active,
                    description = description
                };

                await _db.usergroup.AddAsync(newUserGCode);
                await _db.userstate.AddAsync(newUserSCode);                 
                await _db.SaveChangesAsync();

                //await _db.Database.ExecuteSqlAsync($"INSERT INTO  users (id,login,password,created_date,user_group_id,user_state_id) VALUES ({3},{login},{password},{DateTime.Now},{3},{3})");
                //await _db.Database.ExecuteSqlAsync($"INSERT INTO  usergroup (id,code,description) VALUES ({newUser.user_group_id},{1},{description})");
                //await _db.Database.ExecuteSqlAsync($"INSERT INTO  userstate (id,code,description) VALUES ({newUser.user_state_id},{1},{description})");
                return Authenticate(newUser, newUserGCode);
            }
            catch (Exception ex)
            {
                throw new Exception($"[AddUser] : {ex.Message}");
            }
        }

        public async Task<ClaimsIdentity> Login(string login, string password)
        {
            try
            {
                var users = await _db.users.Where(x => x.login == login).ToListAsync();

                if (users.Count() == 0)
                {
                    throw new Exception("Пользователь не найден");
                }

                foreach (var user in users)
                {
                    if (!user.password.Equals(password))
                    {
                        throw new Exception("Неверный пароль или логин");
                    }
                    var userGroup = await _db.usergroup.FromSqlRaw<UserGroup>($"SELECT * FROM usergroup where usergroup.id = {user.user_group_id}").ToListAsync();


                    return Authenticate(user, userGroup[0]);
                }
                throw new Exception("что-то пошло не так");
            }
            catch (Exception ex)
            {
                throw new Exception($"[Login] : {ex.Message}");
            }
        }

        public async Task<bool> DeleteUser(int id)
        {
            var user = await _db.users.FirstOrDefaultAsync(p => p.id == id);
            try
            {
                if (user == null)//Пользователь отсутствует
                {
                    return false;
                }
                else
                {
                    await _db.Database.ExecuteSqlAsync($"UPDATE userstate users SET code=0 where users.id = {user.user_state_id}");
                    await _db.SaveChangesAsync();

                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private ClaimsIdentity Authenticate(User user, UserGroup ug)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.login),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, ug.code.ToString())
            };
            return new ClaimsIdentity(claims, "ApplicationCookie",
                ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
        }

        public class JsonOutput
        {
            public User? _user;
            public UserGroup? _userGroup;
            public UserState? _userState;
            public JsonOutput() { }
        }
    }   
}

    

