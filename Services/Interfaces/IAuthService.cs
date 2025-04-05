namespace ApiAutenticacao.Services.Interfaces
{
    public interface IAuthService
    {
        /// <summary>
        /// Cria um novo usuário no banco de dados.
        /// </summary>
        Task<bool> RegisterAsync(string username, string password);

        /// <summary>
        /// Realiza o login de um usuário e retorna um token.
        /// </summary>
        Task<string?> LoginAsync(string username, string password);

        Task<bool> UserExistsAsync(string username);
    }
}
