using API_JSONFile_NETChannel;
using API_JSONFile_NETChannel.FileProcessor;
using API_JSONFile_NETChannel.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Channels;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddDbContext<EcommContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("AppConnStr"));
});
builder.Services.AddScoped<ChannellingJob>();
builder.Services.AddScoped<JsonFileProcessor>();
builder.Services.AddAntiforgery();
builder.Services.AddOpenApi();
builder.Services.AddSingleton(Channel.CreateUnbounded<string>());
builder.Services.AddHostedService<FileProcessorBackgroundService>();
builder.Services.AddSwaggerGen(); 

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
     app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
        });
}

app.UseHttpsRedirection();
app.UseAntiforgery(); // Add this line here

app.MapGet("/wf", () =>
{
    return Results.Ok();
})
.WithName("GetWeatherForecast");

app.MapPost("/upload", async (IFormFileCollection files, ChannellingJob channellingJob) =>
{
   // ChannellingJob channellingJob = new ChannellingJob(ctx);
    if (files == null || files.Count == 0)
    {
        return Results.BadRequest("No files uploaded");
    }

    var filePaths = new List<string>();

    foreach (var file in files)
    {
        if (file.Length > 0)
        {
            var filePath = Path.Combine("uploads", file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            filePaths.Add(filePath);
            await channellingJob.EnqueueFileAsync(filePath);
        }
    }
   
    return Results.Accepted($"Accepted All Uploaded Files");
})
.WithName("UploadJsonFiles").DisableAntiforgery();

app.Run();
