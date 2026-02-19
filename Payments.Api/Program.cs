using Microsoft.AspNetCore.Diagnostics;
using Payments.Api.Configuration;
using Payments.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<StripeConfig>(builder.Configuration.GetSection("Stripe"));

builder.Services.AddScoped<StripeService>();
builder.Services.AddSingleton<StripeService>();

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        context.Response.ContentType = "application/json";

        switch (exception)
        {
            case KeyNotFoundException:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                break;

            case BadHttpRequestException:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                break;

            case ApplicationException:
                context.Response.StatusCode = StatusCodes.Status502BadGateway;
                break;

            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                break;
        }

        await context.Response.WriteAsJsonAsync(new
        {
            error = exception?.Message
        });
    });
});

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



