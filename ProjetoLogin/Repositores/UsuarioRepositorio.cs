using Microsoft.AspNetCore.Identity;
using MySql.Data.MySqlClient;
using ProjetoLogin.Models;
using ProjetoLogin.Repositores;
using System.Data;

namespace ProjetoLogin.Repositories
{
    public class UsuarioRepositorio : IUsuarioRepositorio
    {
        private readonly string _connectionString;
        private readonly PasswordHasher<UsuarioModel> _passwordHasher;

        public UsuarioRepositorio(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
            _passwordHasher = new PasswordHasher<UsuarioModel>();
        }

        public async Task<bool> Cadastrar(UsuarioModel usuario, string senhaPura)
        {
            // Gera o hash da senha antes de salvar; a senha pura nunca vai para o banco
            usuario.SenhaHash = _passwordHasher.HashPassword(usuario, senhaPura);

            using var conexao = new MySqlConnection(_connectionString);
            await conexao.OpenAsync();

            var query = "INSERT INTO usuarios (Nome, Email, SenhaHash) VALUES (@Nome, @Email, @SenhaHash)";
            using var comando = new MySqlCommand(query, conexao);

            // Parâmetros evitam SQL Injection
            comando.Parameters.AddWithValue("@Nome", usuario.Nome);
            comando.Parameters.AddWithValue("@Email", usuario.Email);
            comando.Parameters.AddWithValue("@SenhaHash", usuario.SenhaHash);

            var linhasAfetadas = await comando.ExecuteNonQueryAsync();
            return linhasAfetadas > 0;
        }

        public async Task<UsuarioModel?> BuscarEmail(string email)
        {
            using var conexao = new MySqlConnection(_connectionString);
            await conexao.OpenAsync();

            var query = "SELECT Id, Nome, Email, SenhaHash FROM usuarios WHERE Email = @Email";
            using var comando = new MySqlCommand(query, conexao);
            comando.Parameters.AddWithValue("@Email", email);

            using var reader = await comando.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new UsuarioModel
                {
                    Id = reader.GetInt32("Id"),
                    Nome = reader.GetString("Nome"),
                    Email = reader.GetString("Email"),
                    SenhaHash = reader.GetString("SenhaHash")
                };
            }

            return null;
        }
    }
}