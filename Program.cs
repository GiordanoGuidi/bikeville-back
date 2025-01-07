using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using BikeVille.Models;
using BikeVille.Models.Mongodb;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Text;
using BikeVille.Services;

namespace BikeVille
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Configurazione del database SQL con logging
            builder.Services.AddDbContext<AdventureWorksLt2019Context>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("MainSqlConnection"))
                       .EnableSensitiveDataLogging() // Mostra i dettagli delle query
                       .LogTo(Console.WriteLine, LogLevel.Information); // Log delle query in console
            });

            // Configurazione di MongoDB
            builder.Services.Configure<MongoDbSettings>(
                builder.Configuration.GetSection("MongoDbSettings"));

            builder.Services.AddSingleton<IMongoClient>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                return new MongoClient(settings.ConnectionString);
            });

            builder.Services.AddSingleton<IMongoDatabase>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(settings.DatabaseName);
            });

            builder.Services.AddSingleton<MongoPasswordService>();

            // **Aggiungi DbManager**
            builder.Services.AddSingleton<DbManager>(sp =>
            {
                // Recupera la stringa di connessione
                string connectionString = builder.Configuration.GetConnectionString("MainSqlConnection");
                return new DbManager(connectionString);
            });

            // **Aggiungi ErrorHandlingService**
            builder.Services.AddScoped<ErrorHandlingService>();

            // Cors Policy
            builder.Services.AddCors(opts =>
            {
                opts.AddPolicy("CorsPolicy",
                    builder =>
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader());
            });

            // Configura autenticazione con JWT
            JwtSettings jwtSettings = new();
            builder.Configuration.GetSection("JwtSettings").Bind(jwtSettings);
            builder.Services.AddSingleton(jwtSettings);
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opts =>
                {
                    opts.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        RequireExpirationTime = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
                    };
                });

            var app = builder.Build();

            app.UseCors("CorsPolicy");

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
