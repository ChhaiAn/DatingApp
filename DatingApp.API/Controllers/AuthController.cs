using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.DTOS;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
   
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepo _repo;
        private readonly IConfiguration _config;
        public AuthController(IAuthRepo repo, IConfiguration config)
        {
            _config = config;
            _repo = repo;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDTOs userForRegisterDTOs)
        {
            //Validation need 
            userForRegisterDTOs.username = userForRegisterDTOs.username.ToLower();

            if (await _repo.UserExists(userForRegisterDTOs.username))
            {
                return BadRequest("Username is already exists!");
            }

            var userToCreate = new User
            {
                Username = userForRegisterDTOs.username
            };

            var createdUser = _repo.Register(userToCreate, userForRegisterDTOs.password);
            return StatusCode(201);

        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDTOs userForLoginDTOs)
        {
            var userFromRepo = await _repo.Login(userForLoginDTOs.Username.ToLower(), userForLoginDTOs.Password);
            if (userFromRepo == null)
            {
                return Unauthorized();
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userForLoginDTOs.Username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key,SecurityAlgorithms.HmacSha512Signature);
            var tokenDesc = new SecurityTokenDescriptor 
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDesc);

            return Ok(new {
                token = tokenHandler.WriteToken(token)
            });

        }
    }
}