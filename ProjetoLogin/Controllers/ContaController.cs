using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjetoLogin.Models;
using ProjetoLogin.Repositores;
using System.Security.Claims;

namespace ProjetoLogin.Controllers
{
    public class ContaController : Controller
    {
        private readonly IUsuarioRepositorio _usuarioRepositorio;

        public ContaController(IUsuarioRepositorio usuarioRepositorio)
        {
            _usuarioRepositorio = usuarioRepositorio;
        }

        // GET: /Conta/Register
        [HttpGet]
        public IActionResult Register() => View();

        // POST: /Conta/Register
        [HttpPost]
        public async Task<IActionResult> Register(string nome, string email, string senha)
        {
            if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
            {
                ViewBag.Erro = "Preencha todos os campos.";
                return View();
            }

            var usuarioExistente = await _usuarioRepositorio.BuscarEmail(email);
            if (usuarioExistente != null)
            {
                ViewBag.Erro = "Este e-mail já está cadastrado.";
                return View();
            }

            var novoUsuario = new UsuarioModel { Nome = nome, Email = email };
            var sucesso = await _usuarioRepositorio.Cadastrar(novoUsuario, senha);

            if (sucesso)
            {
                TempData["Sucesso"] = "Cadastro realizado com sucesso! Faça o login.";
                return RedirectToAction(nameof(Login));
            }

            ViewBag.Erro = "Erro ao cadastrar usuário.";
            return View();
        }

        // GET: /Conta/Login
        [HttpGet]
        public IActionResult Login() => View();

        // POST: /Conta/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var usuario = await _usuarioRepositorio.BuscarEmail(model.Email);
            if (usuario == null)
            {
                // Mensagem genérica para não revelar se o e-mail existe
                ModelState.AddModelError(string.Empty, "E-mail ou senha inválidos.");
                return View(model);
            }

            var passwordHasher = new PasswordHasher<UsuarioModel>();
            var resultado = passwordHasher.VerifyHashedPassword(usuario, usuario.SenhaHash, model.Senha);

            if (resultado == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "E-mail ou senha inválidos.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString())
            };

            var identidade = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identidade));

            return RedirectToAction(nameof(Index));
        }

        // Página protegida: só acessa quem estiver logado
        [Authorize]
        [HttpGet]
        public IActionResult Index() => View();

        // POST: /Conta/Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }
    }
}