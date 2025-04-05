using ApiAutenticacao.Services.Interfaces;
using ApiAutenticacao.Data;
using ApiAutenticacao.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration; // ✅ Usado para acessar o appsettings.json
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ApiAutenticacao.Services
{
    /// <summary>
    /// Serviço de autenticação responsável pelo registro e login de usuários.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration; // ✅ Para acessar a SecretKey do appsettings.json

        /// <summary>
        /// Injeta o contexto do banco de dados e as configurações.
        /// </summary>
        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <inheritdoc />
        public async Task<bool> RegisterAsync(string username, string password)
        {
            if (await _context.Users.AnyAsync(u => u.Username == username))
                return false; // Usuário já existe

            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User
            {
                Username = username,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc />
       public async Task<string?> LoginAsync(string username, string password)
{
    var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
    if (user == null || !VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
        return null;

    // 🔐 Gera o token com dados do usuário
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!);

    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Username)
    };

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.UtcNow.AddHours(2),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
}

        /// <summary>
        /// Gera um hash e um salt a partir de uma senha.
        /// </summary>
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        /// <summary>
        /// Verifica se a senha informada confere com o hash armazenado.
        /// </summary>
        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        /// <summary>
        /// Verifica se o usuário já existe.
        /// </summary>
        public async Task<bool> UserExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }

        /// <summary>
        /// 🔐 Gera um token JWT com base nos dados do usuário.
        /// </summary>
        private string GenerateJwtToken(User user)
        {
            // 📌 Cria os "claims" (dados codificados dentro do token)
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("userId", user.Id.ToString())
            };

            // 📌 Recupera a chave secreta do appsettings.json
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));

            // 📌 Cria as credenciais de assinatura usando o algoritmo HMAC-SHA256
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 📌 Cria o token com validade de 1 hora
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            // 📌 Retorna o token serializado (em string)
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
