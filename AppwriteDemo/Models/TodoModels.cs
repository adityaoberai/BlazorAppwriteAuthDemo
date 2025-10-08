namespace AppwriteDemo.Models;

public class TodoItem
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTodoRequest
{
    public string Title { get; set; } = string.Empty;
}

public class UpdateTodoRequest
{
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
}
