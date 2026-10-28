using Microsoft.AspNetCore.Authentication.Cookies;
using ProjetoLogin.Repositores;
using ProjetoLogin.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Registra o repositório: sempre que um controller pedir IUsuarioRepositorio, recebe UsuarioRepositorio
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();

// Configura a autenticação por cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Conta/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseRouting();

app.UseAuthentication(); // Identifica quem é o usuário
app.UseAuthorization();  // Verifica o acesso às rotas protegidas

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Conta}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();