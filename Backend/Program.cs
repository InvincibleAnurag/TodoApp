using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<TodoService>();
builder.Services.AddLogging();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader());
});

var app = builder.Build();

// Use CORS
app.UseCors("CorsPolicy");

// Logging Middleware
app.Use(async (context, next) =>
{
    context.Request.EnableBuffering();
    var requestBodyStream = new StreamReader(context.Request.Body);
    var requestBodyText = await requestBodyStream.ReadToEndAsync();
    context.Request.Body.Position = 0;
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation($"Request: {context.Request.Method} {context.Request.Path} {requestBodyText}");

    var originalResponseBodyStream = context.Response.Body;
    using (var responseBodyStream = new MemoryStream())
    {
        context.Response.Body = responseBodyStream;
        await next();
        context.Response.Body = originalResponseBodyStream;

        responseBodyStream.Seek(0, SeekOrigin.Begin);
        var responseBodyText = await new StreamReader(responseBodyStream).ReadToEndAsync();
        logger.LogInformation($"Response: {context.Response.StatusCode} {responseBodyText}");
        responseBodyStream.Seek(0, SeekOrigin.Begin);
        await responseBodyStream.CopyToAsync(originalResponseBodyStream);
    }
});

app.MapGet("/", () => "Welcome to the Todo API with MongoDB!");

app.MapGet("/todos", async (TodoService todoService) =>
{
    var todos = await todoService.GetAllAsync();
    return Results.Ok(todos);
});

app.MapGet("/todos/{id}", async (string id, TodoService todoService) =>
{
    var todo = await todoService.GetByIdAsync(id);
    return todo is not null ? Results.Ok(todo) : Results.NotFound();
});

app.MapPost("/todos", async (TodoItem newTodo, TodoService todoService) =>
{
    await todoService.CreateAsync(newTodo);
    return Results.Created($"/todos/{newTodo.Id}", newTodo);
});

app.MapPut("/todos/{id}", async (string id, TodoItem updatedTodo, TodoService todoService) =>
{
    var todo = await todoService.GetByIdAsync(id);
    if (todo is not null)
    {
        updatedTodo.Id = todo.Id;
        await todoService.UpdateAsync(id, updatedTodo);
        return Results.Ok(updatedTodo);
    }
    return Results.NotFound();
});

app.MapDelete("/todos/{id}", async (string id, TodoService todoService) =>
{
    var todo = await todoService.GetByIdAsync(id);
    if (todo is not null)
    {
        await todoService.DeleteAsync(id);
        return Results.NoContent();
    }
    return Results.NotFound();
});

app.Run();

public class TodoItem
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    
    public string Title { get; set; }
    public bool IsCompleted { get; set; }
}
