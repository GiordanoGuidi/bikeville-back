
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using BikeVille.Models;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType; // aaa
using System.Runtime.ConstrainedExecution;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using BikeVille.Models.Mongodb;
using BikeVille.Models.Services;


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
            builder.Services.AddScoped<FilterService>();

            //Aggiungo configurazione al database Sql
            builder.Services.AddDbContext<AdventureWorksLt2019Context>(opt => opt.UseSqlServer(
            builder.Configuration.GetConnectionString("MainSqlConnection")));

            // Aggiungo la configurazione di MongoDbSettings
            builder.Services.Configure<MongoDbSettings>(
                //Popolo l'oggetto MongoDbSettings con il contenuto di MongodbSettings del file di configurazione json
                builder.Configuration.GetSection("MongoDbSettings"));

            // Aggiungo la connessione MongoDB
            builder.Services.AddSingleton<IMongoClient>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                //Creo MongoClient utilizzando la stringa di connessione
                return new MongoClient(settings.ConnectionString);
            });

            //Registro istanza IMongoDatabase nel contenitore DI
            builder.Services.AddSingleton<IMongoDatabase>(sp =>
            {
                //Recupero impostazioni di configurazione
                var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(settings.DatabaseName);
            });

            //Registro il servizio come DI
            builder.Services.AddSingleton<MongoPasswordService>();

            //Cors Policy
            builder.Services.AddCors(opts =>
            {
                opts.AddPolicy("CorsPolicy",
                    builder =>
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });

            //Create JwtSettings istances
            JwtSettings jwtSettings = new();
            /*popolo l'oggetto jwtSettings con i valori presenti
           nella sezione "JwtSettings" presente nell'appsettings,json*/
            builder.Configuration.GetSection("JwtSettings").Bind(jwtSettings);
            //Aggiungo jwtSettings come singleton nei servizi
            _ = builder.Services.AddSingleton(jwtSettings);
            //Configurazione servizio autenticazione
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                //Configura jwt Bearer
                .AddJwtBearer(opts =>
                {
                    opts.TokenValidationParameters = new TokenValidationParameters
                    {
                        /*Controlla che il token sia emesso da un issuer 
                      * valido (emittente) specificato in ValidIssuer*/
                        ValidateIssuer = true,
                        //Verifica che il token sia destinato al pubblico(audience)
                        //corretto specificato in ValidAudience.
                        ValidateAudience = true,
                        /*Controlla che il token non sia scaduto 
                     * (data di scadenza inclusa nel token).*/
                        ValidateLifetime = true,
                        /*Verifica che il token sia firmato 
                     * correttamente con una chiave valida.*/
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        //Indica che il token deve avere una data di scadenza.
                        RequireExpirationTime = true,
                        //Specifica la chiave usata per firmare e verificare il token.
                        IssuerSigningKey =
                        new SymmetricSecurityKey(
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
