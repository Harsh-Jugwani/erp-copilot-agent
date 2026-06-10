using ERP.API.Extensions;
using ERP.API.Utilities;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

//Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContextAndIdentity(builder.Configuration);
builder.Services.AddAppSettings(builder.Configuration);
builder.Services.AddAuthenticationWithJwtBearer(builder.Configuration);
builder.Services.AddOpenApiDocumentation();
builder.Services.AddAuthorization();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddHttpContextAccessor();
builder.Services.AddServices(builder.Configuration);

//Build the app.
var app = builder.Build();

//Configure the HTTP request pipeline.
app.MapOpenApi();
app.MapScalarApiReference();
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

//Seed the database.
await app.Services.SeedAsync();

app.Run();
