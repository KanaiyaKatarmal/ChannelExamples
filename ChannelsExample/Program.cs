using ChannelsExample;
using ChannelsExample.Worker;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Channels;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


builder.Services.AddSwaggerGen();

// Register channel as singleton
builder.Services.AddSingleton(Channel.CreateUnbounded<ChannelRequest>());
builder.Services.AddSingleton(Channel.CreateUnbounded<FileImportRequest>());




builder.Services.AddDbContext<ApplicationDbContext>(optionsBuilder =>
optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=Product;Username=postgres;Password=postgres"));

// Register background processor
builder.Services.AddHostedService<Processor>();

 // Register background processor
builder.Services.AddHostedService<FileProcessor>();
builder.Services.AddScoped<IFileImportWorker, FileImportWorker>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/Send", async (Channel<ChannelRequest> _channel) =>
{
    await _channel.Writer.WriteAsync(new ChannelRequest($"Hello from {DateTime.UtcNow}"));
    return Results.Ok();
})
.WithName("Send");


app.MapPost("/FileUpload",
    async ([FromForm] IFormFileCollection files, Channel<FileImportRequest> _channel) =>
    {
        List<FileImportResponseModel> result = new();

        foreach (var file in files)
        {
            var requestId = Guid.NewGuid().ToString();

            var request = new FileImportRequest
            {
                RequestId = requestId,
                FileData = new MemoryStream()
            };

            await file.CopyToAsync(request.FileData);
            request.FileData.Position = 0;

            await _channel.Writer.WriteAsync(request);

            result.Add(new FileImportResponseModel
            {
                RequestId = requestId,
                FileName = file.FileName,
                FileSize = file.Length,
                Status = "Scheduled for Processing"
            });
        }

        return result;
    })
    .WithName("FileUpload")
    .Accepts<IFormFile[]>("multipart/form-data")   // ?? important
    .DisableAntiforgery();


app.Run();

