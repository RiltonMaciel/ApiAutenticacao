using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApiAutenticacao.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProtegidoController : ControllerBase
    {
        [HttpGet]
        [Authorize]
        public IActionResult Get()
        {
            // 🔎 Recupera os dados do usuário autenticado (do JWT)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var username = User.Identity?.Name;

            return Ok(new
            {
                Id = userId,
                Username = username,
                Mensagem = "✅ Token válido! Acesso concedido."
            });
        }
    }
}
