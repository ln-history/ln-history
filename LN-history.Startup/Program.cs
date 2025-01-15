using System.Reflection;
using Bitcoin.Core;
using Bitcoin.Data;
using Dapper.FluentMap;
using Dapper.FluentMap.Dommel;
using LightningGraph.Model;
using LN_history.Api;
using LN_history.Api.Controllers;
using LN_history.Api.Mapping;
using LN_history.Cache;
using LN_history.Core;
using LN_history.Core.Mapping;
using LN_history.Core.Services;
using LN_history.Data;
using LN_history.Data.Configuration;
using LN_History.Model.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGossipMessageData(builder.Configuration);

builder.Services.AddBitcoinBlocks(builder.Configuration);

FluentMapper.Initialize(configuration =>
{
    configuration.AddMap(new NodeAnnouncementMessageConfiguration());
    configuration.AddMap(new ChannelUpdateMessageConfiguration());
    configuration.AddMap(new ChannelAnnouncementMessageConfiguration());
    configuration.AddMap(new ChannelMessageCompleteConfiguration());
    configuration.ForDommel();
});

builder.Services.Configure<LightningSettings>(builder.Configuration.GetSection("LightningSettings"));

builder.Services.AddCaching(builder.Configuration);

builder.Services.AddLightningNetworkServices(builder.Configuration);
builder.Services.AddBitcoinServices();

builder.Services.AddApiServices(
    [Assembly.GetAssembly(typeof(NodeController))]
);

builder.Services.AddAutoMapper(typeof(LightningNodeMappingProfile));
builder.Services.AddAutoMapper(typeof(LightningChannelMappingProfile));
builder.Services.AddAutoMapper(typeof(ChannelAnnouncementMappingProfile));
builder.Services.AddAutoMapper(typeof(ChannelUpdateMappingProfile));
builder.Services.AddAutoMapper(typeof(NodeAnnouncementMappingProfile));
builder.Services.AddAutoMapper(typeof(ChannelMessageMappingProfile));


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

builder.Services.AddSwaggerGen(opt =>
{
    var assemblies = new [] {
        Assembly.GetAssembly(typeof(NodeService))
    };

    foreach (var assembly in assemblies)
    {
        var xmlFileName = $"{assembly!.GetName().Name}.xml";
        opt.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFileName));
    }
    
    opt.SwaggerDoc("v1", new OpenApiInfo {Title = "Lightning Network History", Version = "v1"});
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Name = "x-api-key",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "API key required to access this API"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

    var apiAssemblies = new[]
    {
        Assembly.GetAssembly(typeof(LightningNetworkController))
    };

    foreach (var assembly in apiAssemblies)
    {
        var fileName = $"{assembly!.GetName().Name}.xml";
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, fileName));
    }
});


var app = builder.Build();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});


// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "LN-history API V1");
    });
// }

app.UseHttpsRedirection();

app.Run();
