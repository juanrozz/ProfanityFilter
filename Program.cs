using Microsoft.EntityFrameworkCore;
using ProfanityFilter.Entities;
using ProfanityFilter.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");


builder.Services.AddDbContext<InternsDbContext>(options =>
    options.UseSqlServer(connectionString));

// //for when use with the service IDisponsable or when is only <--------------------------------------
//builder.Services.AddScoped<IProfanityFilterService, ProfanityFilterServiceOnly>();



// for when use with the service IScopeFactory <---------------------------------------------
builder.Services.AddSingleton<IProfanityFilterService, ProfanityFilterServiceWithScopeFactory>();


// Add services to the container.


builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(policy =>
	{
		policy.AllowAnyOrigin()
			  .AllowAnyHeader()
			  .AllowAnyMethod();
	});
});

var app = builder.Build();


//for when use with the service IScopeFactory < ---------------------------------------------
using (var scope = app.Services.CreateScope())
{

    var filterService = scope.ServiceProvider.GetRequiredService<IProfanityFilterService>();

    await filterService.LoadWordsFromDBAsync();
}

// Configure the HTTP request pipeline.


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
