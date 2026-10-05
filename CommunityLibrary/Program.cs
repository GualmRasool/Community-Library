using CommunityLibrary.Repository;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers()
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(
    new JsonStringEnumConverter());
});

builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename =
    $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

    options.IncludeXmlComments(
    Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

builder.Services.AddScoped<IBooksRepository, BooksRepository>();
builder.Services.AddScoped<ILoansRepository, LoansRepository>();
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseInMemoryDatabase("LibraryDb")
   
);


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    DbSeeder.Seed(
    scope.ServiceProvider.GetRequiredService<LibraryDbContext>());
}

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
