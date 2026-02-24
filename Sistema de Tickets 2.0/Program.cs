using Capa_Datos.ReclamosBodega;
using Capa_Datos.Tickets;
using Capa_Entidad.Tickets;
using Capa_Negocios.ReclamosBodega;
using Capa_Negocios.Tickets;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios MVC
builder.Services.AddControllersWithViews();

#region Tickets
// Inyección de dependencias
builder.Services.AddScoped<CD_Usuarios>();
builder.Services.AddScoped<CN_Usuarios>();

builder.Services.AddScoped<CD_Permisos>(); // Si tu clase de datos se llama así
builder.Services.AddScoped<CN_Permisos>();

builder.Services.AddScoped<CD_Acceso>(); // Capa de Datos
builder.Services.AddScoped<CN_Acceso>(); // Capa de Negocios

builder.Services.AddScoped<CD_Roles>(); // Capa de Datos
builder.Services.AddScoped<CN_Roles>(); // Capa de Negocios


builder.Services.AddScoped<CD_Estados>(); // Capa de Datos
builder.Services.AddScoped<CN_Estados>(); // Capa de Negocios

builder.Services.AddScoped<CD_Categorias>(); // Capa de Datos
builder.Services.AddScoped<CN_Categorias>(); // Capa de Negocios


builder.Services.AddScoped<CD_Equipos>(); // Capa de Datos
builder.Services.AddScoped<CN_Equipos>(); // Capa de Negocios

builder.Services.AddScoped<CD_Niveles_Urgencia_tickets>(); // Capa de Datos
builder.Services.AddScoped<CN_Niveles_Urgencia_Tickets>(); // Capa de Negocios

builder.Services.AddScoped<CD_Tipos_Incidencias_Tickets>(); // Capa de Datos
builder.Services.AddScoped<CN_Tipos_Incidencias_Tickets>(); // Capa de Negocios

builder.Services.AddScoped<CD_Cajas>(); // Capa de Datos
builder.Services.AddScoped<CN_Cajas>(); // Capa de Negocios

builder.Services.AddScoped<CD_Tickets>(); // Capa de Datos
builder.Services.AddScoped<CN_Tickets>(); // Capa de Negocios

builder.Services.AddScoped<Capa_Datos.Tickets.CD_Archivos>(); // Capa de Datos
builder.Services.AddScoped<Capa_Negocios.Tickets.CN_Archivos>(); // Capa de Negocios


builder.Services.AddScoped<Capa_Datos.Tickets.CD_Comentarios>(); // Capa de Datos
builder.Services.AddScoped<Capa_Negocios.Tickets.CN_Comentarios>(); // Capa de Negocios

builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();

#endregion


#region RECLAMOS
builder.Services.AddScoped<CD_TiposIncidenciasReclamos>();
builder.Services.AddScoped<CN_TiposIncidenciasReclamos>();

builder.Services.AddScoped<CD_ReclamosBodega>();
builder.Services.AddScoped<CN_ReclamosBodega>();

builder.Services.AddScoped<Capa_Datos.ReclamosBodega.CD_Archivos>(); // Capa de Datos
builder.Services.AddScoped<Capa_Negocios.ReclamosBodega.CN_Archivos>(); // Capa de Negocios

builder.Services.AddScoped<Capa_Datos.ReclamosBodega.CD_Comentarios>(); // Capa de Datos
builder.Services.AddScoped<Capa_Negocios.ReclamosBodega.CN_Comentarios>(); // Capa de Negocios
#endregion


builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 52428800; // 50 MB
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 52428800; // 50 MB
});




builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}



app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Acceso}/{action=Login}/{id?}");

app.Run();
