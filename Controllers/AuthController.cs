using Microsoft.AspNetCore.Mvc;
using ApiAutenticacao.DTOs;
using ApiAutenticacao.Services;

namespace ApiAutenticacao.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(UserRegisterDto dto)
        {
            if (await _authService.UserExists(dto.Username))
                return BadRequest("Usuário já existe.");

            var user = await _authService.Register(dto.Username, dto.Password);

            var response = new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username
            };

            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(UserLoginDto dto)
        {
            var user = await _authService.Login(dto.Username, dto.Password);
            if (user == null) return Unauthorized("Credenciais inválidas.");

            var response = new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username
            };

            return Ok(response);
        }
    }

    internal class UserResponseDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
    }
}
