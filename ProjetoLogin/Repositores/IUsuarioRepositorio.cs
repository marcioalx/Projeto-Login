using ProjetoLogin.Models;

namespace ProjetoLogin.Repositores
{
    public interface IUsuarioRepositorio
    {
        Task<bool> Cadastrar(UsuarioModel usuario, string senhaPura);
        Task<UsuarioModel?> BuscarEmail(string email);
    }
}