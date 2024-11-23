using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MyServer.Data;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;
using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using MyServer.Helpers;
using Microsoft.AspNetCore.Identity.Data;
using MyServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace MyServer.Controllers
{
    [ApiController]
    [Route("connect")]
    public class TokenController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public TokenController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        /* Возможно я не так понял тз, и токен должен был получить от авторизации самого приложения,
         * а не пользователя, но на всякий решил оставить это.
         * */
        [HttpPost("token")]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> Exchange()
        {
            Console.WriteLine("Вызван Exchange");

            var request = HttpContext.GetOpenIddictServerRequest();

            if (request == null)
            {
                return BadRequest(new { error = "Invalid request" });
            }

            if (request.GrantType != GrantTypes.Password)
            {
                return BadRequest(new { error = "Unsupported grant type" });
            }

            var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Login == request.Username);
            if (user == null || !Crypto.VerifyPassword(user.Password, request.Password))
            {
                return BadRequest(new { error = "Invalid login or password" });
            }

            var claims = new List<Claim>
            {
                new Claim(Claims.Subject, user.Id.ToString()),
                new Claim(Claims.Name, user.Login),
                new Claim(ClaimTypes.Role, "admin")
            };

            var identity = new ClaimsIdentity(claims,
                TokenValidationParameters.DefaultAuthenticationType);

            var principal = new ClaimsPrincipal(identity);

            principal.SetScopes(Scopes.Roles, "admin");

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        [HttpPost("Authenticate")]
        [Consumes("application/x-www-form-urlencoded")]  // Указываем, что сервер принимает данные в формате form-urlencoded
        public async Task<IActionResult> Authenticate([FromForm] RegisterRequest request)
        {
            Console.WriteLine("Вызван Authenticate");

            //проверка есть в приложение, но на всякий
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                string er = "Требуются имя пользователя и пароль";
                Console.WriteLine(er);
                return BadRequest(new { error = er });
            }

            var existingUser = await _dbContext.Users
                .SingleOrDefaultAsync(u => u.Login == request.Username);

            if (existingUser == null)
            {
                string er = "Такой пользователь не существует";
                Console.WriteLine(er);
                return BadRequest(new { error = er });
            }

            if (!Crypto.VerifyPassword(existingUser.Password, request.Password))
            {
                string er = "Неверный логин или пароль";
                Console.WriteLine(er);
                return BadRequest(new { error = er });
            }

            var claims = new List<Claim>
            {
                new Claim(Claims.Subject, existingUser.Id.ToString()),
                new Claim(Claims.Name, existingUser.Login),
                //new(ClaimTypes.Role, "admin")
            };

            var identity = new ClaimsIdentity(claims, TokenValidationParameters.DefaultAuthenticationType);
            var principal = new ClaimsPrincipal(identity);

            //principal.SetScopes(Scopes.Roles, "admin");
            
            var ret = SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            return ret;
        }

        [HttpPost("register")]
        [Consumes("application/x-www-form-urlencoded")]  
        public async Task<IActionResult> Register([FromForm] RegisterRequest request) 
        {
            Console.WriteLine("Вызван Register");

            //проверка есть в приложение, но на всякий
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                string er = "Требуются имя пользователя и пароль";
                Console.WriteLine(er);
                return BadRequest(new { error = er });
            }

            var existingUser = await _dbContext.Users
                .SingleOrDefaultAsync(u => u.Login == request.Username);

            if (existingUser != null)
            {
                string er = "Пользователь уже существует";
                Console.WriteLine(er);
                return BadRequest(new { error =  er});
            }

            var hashedPassword = Crypto.HashPassword(request.Password);

            var user = new User
            {
                Login = request.Username,
                Password = hashedPassword
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Пользователь успешно зарегистрирован" });
        }

        [HttpGet("getuser")]
        [Authorize()]
        public IActionResult GetUsers()
        {
            return Ok(_dbContext.Users);
        }

        // не знаю насколько это правильная проверка
        [HttpGet("validate-token")]
        [Authorize]
        public IActionResult Get()
        {
            return Ok(new { message = "Token is valide" });
        }
    }

    public class RegisterRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

}
