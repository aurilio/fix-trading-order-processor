using FixTrading.OrderGenerator.Api.Services;
using FixTrading.OrderProcessing.Application.Abstractions;
using FixTrading.OrderProcessing.Application.Services;
using FixTrading.OrderProcessing.Application.Validators;
using FixTrading.OrderProcessing.Infrastructure.Fix;
using FluentValidation;

namespace FixTrading.Ordergenerator.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new()
            {
                Title = "Order Generator API",
                Version = "v1",
                Description = "API para geração de ordens FIX"
            });
        });

        builder.Services.AddValidatorsFromAssemblyContaining<SendOrderRequestValidator>();

        builder.Services.AddFixInitiator(builder.Configuration);

        builder.Services.AddScoped<IOrderApplicationService, OrderApplicationService>();

        builder.Services.AddHostedService<FixClientHostedService>();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors("AllowAll");
        app.UseStaticFiles();
        app.MapControllers();

        app.MapFallbackToFile("index.html");

        app.Run();
    }
}