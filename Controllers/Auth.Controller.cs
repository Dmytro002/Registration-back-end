using System;
using auth.Data;
using auth.Dtos;
using auth.Helpers;
using auth.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Renci.SshNet.Messages;

namespace auth.Controllers
{
    [Route("api")]
    [ApiController]
    
    public class AuthController:Controller
    {
        private readonly IUserRepository _repository;
        private readonly JwtService _jwtService;

        public AuthController(IUserRepository repository ,JwtService jwtService)
        {
            _repository = repository;
            _jwtService = jwtService;
        }
        [HttpPost("register")]
        public IActionResult Register(RegisterDto dto)
        {
            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };
            return Created("ssf", _repository.Create(user));
        }

        [HttpPost("login")]
        public IActionResult Login (LoginDto dto)
        {
            var user = _repository.GetByEmail(dto.Email);

            if (user == null) return BadRequest(new {message = "Invalid Credentials"});

            if (BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            {
                return BadRequest(new {message = "Invalid Credentials"}); 
            }

            var jwt = _jwtService.Generate(user.Id);
            Response.Cookies.Append("jtw",jwt,new CookieOptions
            {
                 HttpOnly = true
            });
            return Ok(new
            {
                messsge = "success"
            });
        }
        [HttpPost("user")]
        public new  IActionResult User()
        {
            try
            {
                var jwt = Request.Cookies["jwt"];
                var token = _jwtService.Verify(jwt);

                int userId = int.Parse(token.Issuer);
                var user = _repository.GetById(userId);
                return Ok(user);
            }
            catch (Exception)
            {
                return Unauthorized();
            }
        }
        [HttpPost ("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("lwt");
            return Ok(new {message = "success"});
        }
    }
}