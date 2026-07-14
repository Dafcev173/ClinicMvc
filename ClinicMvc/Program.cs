using ClinicMvc.Data;
using ClinicMvc.Middleware;
using ClinicMvc.Repositories;
using ClinicMvc.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// Firebird конекција (читана од appsettings.json преку IConfiguration)
builder.Services.AddSingleton<IDbConnectionFactory, FirebirdConnectionFactory>();

// Репозитории
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();

// Логер за грешки - Singleton бидејќи чува само патека до Logs/errors.txt,
// а пишувањето е синхронизирано преку SemaphoreSlim (безбедно за паралелни барања)
builder.Services.AddSingleton<IErrorLogger, FileErrorLogger>();

var app = builder.Build();

// Само во продукција - HSTS (наметнува HTTPS) за дополнителна безбедност
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ВАЖНО: GlobalExceptionMiddleware мора да е РЕГИСТРИРАН ОВДЕ -
// по UseRouting() (за да има пристап до route values: контролер/акција),
// но пред UseAuthorization()/MapControllerRoute() (за да го опфати
// извршувањето на секоја акција во секој контролер).
app.UseGlobalExceptionHandling();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
