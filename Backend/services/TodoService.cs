using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class TodoService
{
    private readonly IMongoCollection<TodoItem> _todoItems;
    private readonly ILogger<TodoService> _logger;

    public TodoService(ILogger<TodoService> logger)
    {
        var client = new MongoClient("mongodb+srv://invincibleanurag5:anurag@todoapp.2x6qk94.mongodb.net/TodoDb?retryWrites=true&w=majority&appName=TodoApp");
        var database = client.GetDatabase("TodoDb");
        _todoItems = database.GetCollection<TodoItem>("TodoItems");
        _logger = logger;
    }

    public async Task<List<TodoItem>> GetAllAsync()
    {
        try
        {
            return await _todoItems.Find(todo => true).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting all todos.");
            throw;
        }
    }

    public async Task<TodoItem> GetByIdAsync(string id)
    {
        try
        {
            return await _todoItems.Find(todo => todo.Id == id).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while getting the todo with ID {id}.");
            throw;
        }
    }

    public async Task CreateAsync(TodoItem newTodo)
    {
        try
        {
            if (string.IsNullOrEmpty(newTodo.Id))
            {
                newTodo.Id = ObjectId.GenerateNewId().ToString();
            }
            await _todoItems.InsertOneAsync(newTodo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating a new todo.");
            throw;
        }
    }

    public async Task UpdateAsync(string id, TodoItem updatedTodo)
    {
        try
        {
            await _todoItems.ReplaceOneAsync(todo => todo.Id == id, updatedTodo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while updating the todo with ID {id}.");
            throw;
        }
    }

    public async Task DeleteAsync(string id)
    {
        try
        {
            await _todoItems.DeleteOneAsync(todo => todo.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while deleting the todo with ID {id}.");
            throw;
        }
    }
}
