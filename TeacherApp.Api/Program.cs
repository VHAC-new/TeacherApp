using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Amazon;
using Amazon.S3;
using TeacherApp.Api.Application.Admin.Exercises;
using TeacherApp.Api.Application.Admin.FinalExercises;
using TeacherApp.Api.Application.Admin.Lessons;
using TeacherApp.Api.Application.Admin.Media;
using TeacherApp.Api.Application.Admin.Modules;
using TeacherApp.Api.Application.Auth;
using TeacherApp.Api.Application.Catalog;
using TeacherApp.Api.Application.Exercises;
using TeacherApp.Api.Application.Media;
using TeacherApp.Api.Application.Progress;
using TeacherApp.Api.Data;
using TeacherApp.Api.Infrastructure.Seed;
using TeacherApp.Api.Infrastructure.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TeacherApp.Api",
        Version = "v1",
    });

    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer",
        },
    };

    options.AddSecurityDefinition("Bearer", scheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            scheme,
            Array.Empty<string>()
        }
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Missing connection string: ConnectionStrings:DefaultConnection");
}

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminModuleService, AdminModuleService>();
builder.Services.AddScoped<IAdminLessonService, AdminLessonService>();
builder.Services.AddScoped<IAdminExerciseService, AdminExerciseService>();
builder.Services.AddScoped<IAdminFinalExerciseService, AdminFinalExerciseService>();
builder.Services.Configure<MediaS3Options>(builder.Configuration.GetSection("Media:S3"));

var s3Bucket = builder.Configuration["Media:S3:Bucket"];
if (!string.IsNullOrWhiteSpace(s3Bucket))
{
    var s3Region = builder.Configuration["Media:S3:Region"]
        ?? builder.Configuration["AWS:Region"]
        ?? Environment.GetEnvironmentVariable("AWS_REGION");
    if (string.IsNullOrWhiteSpace(s3Region))
    {
        throw new InvalidOperationException(
            "Media:S3:Bucket is set but Media:S3:Region (or AWS_REGION / AWS:Region) is required for S3.");
    }

    builder.Services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(RegionEndpoint.GetBySystemName(s3Region)));
    builder.Services.AddSingleton<IS3MediaOperations, S3MediaOperations>();
}
else
{
    builder.Services.AddSingleton<IS3MediaOperations, DisabledS3MediaOperations>();
}

builder.Services.AddScoped<IAdminMediaService, AdminMediaService>();
builder.Services.AddScoped<IMediaPlaybackService, MediaPlaybackService>();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<IExerciseSubmitService, ExerciseSubmitService>();
builder.Services.AddScoped<IProgressService, ProgressService>();

var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
var jwtSecretEnvVarName = builder.Configuration["Jwt:SecretEnvVarName"];

if (string.IsNullOrWhiteSpace(jwtIssuer) ||
    string.IsNullOrWhiteSpace(jwtAudience) ||
    string.IsNullOrWhiteSpace(jwtSecretEnvVarName))
{
    throw new InvalidOperationException("Missing JWT configuration: Jwt:Issuer, Jwt:Audience, Jwt:SecretEnvVarName");
}

var jwtSecret = Environment.GetEnvironmentVariable(jwtSecretEnvVarName);
if (string.IsNullOrWhiteSpace(jwtSecret))
{
    throw new InvalidOperationException($"Missing JWT secret environment variable: {jwtSecretEnvVarName}");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

var mediaBucket = app.Configuration["Media:S3:Bucket"];
if (!string.IsNullOrWhiteSpace(mediaBucket))
{
    var mediaRegion = app.Configuration["Media:S3:Region"]
        ?? app.Configuration["AWS:Region"]
        ?? Environment.GetEnvironmentVariable("AWS_REGION");
    var mediaArnObjectPattern = $"arn:aws:s3:::{mediaBucket}/*";
    app.Logger.LogInformation(
        "S3 media storage enabled. Bucket={Bucket}. Region={Region}. IAM policy Resource should include {MediaArnObjectPattern}",
        mediaBucket,
        mediaRegion ?? "(from SDK default / instance profile)",
        mediaArnObjectPattern);

    if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID")))
    {
        app.Logger.LogInformation(
            "AWS_ACCESS_KEY_ID is not set; the AWS SDK will use instance profile, shared credentials file, or other default chain.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "TeacherApp.Api v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    await DevSeed.TrySeedAsync(scope.ServiceProvider, app.Lifetime.ApplicationStopping);
}

app.Run();
