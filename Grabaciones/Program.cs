using Grabaciones.Services.Interface;
using Grabaciones.Services.Repositorio;
using Grabaciones.Services.Econtact;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddTransient<IDescargaDiaria, RDescargaDiaria>();
builder.Services.AddTransient<IDescargaMayor60Dias, RDescargaMayor60Dias>();
builder.Services.AddTransient<IDescargaDiaria, RDescargaDiaria>();
builder.Services.AddTransient<IEC_Metodos, EC_Metodos>();

builder.Services.AddHttpClient<RDescargaMayor60Dias>();
builder.Services.AddHttpClient<RDescargaDiaria>();
builder.Services.AddHttpClient<EC_Metodos>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
