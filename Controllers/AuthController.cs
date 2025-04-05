using Microsoft.AspNetCore.Mvc;
using ApiAutenticacao.DTOs;
using ApiAutenticacao.Services.Interfaces;

namespace ApiAutenticacao.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(UserRegisterDto dto)
        {
            if (await _authService.UserExistsAsync(dto.Username))
                return BadRequest("Usuário já existe.");

            var success = await _authService.RegisterAsync(dto.Username, dto.Password);
            if (!success) return StatusCode(500, "Erro ao registrar usuário.");

            return Ok(new { message = "Usuário registrado com sucesso!" });
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(UserLoginDto dto)
        {
            var token = await _authService.LoginAsync(dto.Username, dto.Password);
            if (token == null) return Unauthorized("Credenciais inválidas.");

            return Ok(new { token });
        }
    }
}
