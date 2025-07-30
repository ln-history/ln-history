using System.Reflection;
using Asp.Versioning;
using Bitcoin.Core;
using Bitcoin.Data;
using Dapper.FluentMap;
using Dapper.FluentMap.Dommel;
using LN_history.Api;
using LN_history.Api.Mapping;
using LN_history.Api.v1.Controllers;
using LN_history.Api.v2.Controllers;
using LN_history.Cache;
using LN_history.Core;
using LN_history.Core.Services;
using LN_history.Data;
using LN_history.Data.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLnHistoryDatabase(builder.Configuration);

builder.Services.AddBitcoinBlocks(builder.Configuration);

FluentMapper.Initialize(configuration =>
{
    configuration.AddMap(new NodeEntityMap());
    configuration.AddMap(new ChannelEntityMap());
    configuration.ForDommel();
});

builder.Services.AddCaching(builder.Configuration);

builder.Services.AddLightningNetworkServices(builder.Configuration);
builder.Services.AddBitcoinServices();

builder.Services.AddApiServices(
    [Assembly.GetAssembly(typeof(LightningNetworkController)), Assembly.GetAssembly(typeof(SnapshotController))]
);

builder.Services.AddAutoMapper(typeof(LightningNodeMappingProfile));
builder.Services.AddAutoMapper(typeof(LightningChannelMappingProfile));


builder.Services.AddSwaggerGenNewtonsoftSupport();
builder.Services
    .AddControllers(opt =>
    {
        var noContentFormatter = opt.OutputFormatters.OfType<HttpNoContentOutputFormatter>().FirstOrDefault();
        if (noContentFormatter != null)
        {
            noContentFormatter.TreatNullValueAsNoContent = false;
        }
    }).AddNewtonsoftJson().ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var problemDetails = new ValidationProblemDetails(context.ModelState)
            {
                Status = StatusCodes.Status400BadRequest,
            };
            return new BadRequestObjectResult(problemDetails);
        };
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1);
        options.ReportApiVersions = true;
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ApiVersionReader = ApiVersionReader.Combine(
            new UrlSegmentApiVersionReader(),
            new HeaderApiVersionReader("X-Api-Version"));
    })
    .AddMvc() // This is needed for controllers
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'V";
        options.SubstituteApiVersionInUrl = true;
    });

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
// Configure Swagger
builder.Services.AddSwaggerGen(opt =>
{
    // Create separate Swagger specs for v1 and v2
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "Lightning Network History", Description = "Queries a PostgreSQL database that stores the data on a SSD", Version = "v1" });
    opt.SwaggerDoc("v2", new OpenApiInfo { Title = "Lightning Network History", Description = "Queries a DuckDB that stores the data in RAM", Version = "v2" });

    // Include XML comments
    var assemblies = new[] { Assembly.GetAssembly(typeof(NodeService)), Assembly.GetAssembly(typeof(SnapshotController)) };
    foreach (var assembly in assemblies)
    {
        var xmlFileName = $"{assembly!.GetName().Name}.xml";
        opt.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFileName));
    }

    // Security definition (if applicable to both versions)
    opt.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Name = "x-api-key",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "API key required to access this API"
    });
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                },
                In = ParameterLocation.Header
            },
            Array.Empty<string>()
        }
    });
});


var app = builder.Build();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.DocumentTitle = "Lightning Network History";
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "LN-history API V1");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "LN-history API V2");
});


app.UseHttpsRedirection();

app.Run();
