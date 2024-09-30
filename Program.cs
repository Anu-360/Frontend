using Microsoft.EntityFrameworkCore;
using DinkToPdf;
using DinkToPdf.Contracts;
using Hexa_Hub.Interface;
using Hexa_Hub.Repository;
using Hexa_Hub.Constants;
using Hexa_Hub.Models;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
using System.Configuration;
using Microsoft.AspNetCore.Hosting;
using iText.Kernel.XMP.Options;
namespace Hexa_Hub
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddSingleton<IConverter, SynchronizedConverter>(provider => new SynchronizedConverter(new PdfTools()));

            builder.Services.AddScoped<ICategory, CategoryService>();
            builder.Services.AddScoped<IAsset, AssetService>();
            builder.Services.AddScoped<IAssetAllocation, AssetAllocationService>();
            builder.Services.AddScoped<IAssetRequest, AssetRequestService>();
            builder.Services.AddScoped<ISubCategory, SubCategoryService>();
            builder.Services.AddScoped<IServiceRequest, ServiceRequestImpl>();
            builder.Services.AddScoped<IAuditRepo, AuditRepo>();
            builder.Services.AddScoped<IMaintenanceLogRepo, MaintenanceLogRepo>();
            builder.Services.AddScoped<IUserRepo, UserRepo>();
            builder.Services.AddScoped<IReturnReqRepo, ReturnRequestRepo>();
            builder.Services.AddScoped<IEmail, EmailService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

        


            builder.Services.AddControllers()
            .AddJsonOptions(opts => opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve);

            builder.Services.AddControllers()
         .AddNewtonsoftJson();

            builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReact",
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:5173")
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                                .AllowCredentials();
                    });
            });

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey= true
                };
            });


            builder.Services.AddAuthorization();
            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).AddEnvironmentVariables();

            // Add services to the container.
            builder.Services.AddControllers()
          .AddJsonOptions(options =>
          {
              options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
              options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
          });


            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Hexa Hub", Version = "v1" });
              
                options.SchemaFilter<EnumSchemaFilter>();
                
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please Enter Token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "bearer"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[]{}
                    }
                });



               
            });
           



            builder.Services.AddDbContext<DataContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("ConStr")));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("AllowReact");
            app.UseAuthentication();
            app.UseAuthorization();
            IConfiguration configuration = app.Configuration;
            IWebHostEnvironment environment = app.Environment;


            app.MapControllers();

            app.Run();
        }
    }
}
