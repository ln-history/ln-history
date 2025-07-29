using Microsoft.AspNetCore.Mvc;

namespace LN_history.Api.v2.Controllers;

public class FileCallbackResult : IActionResult
{
    private readonly string _contentType;
    private readonly string _fileName;
    private readonly Func<Stream, ActionContext, Task> _callback;

    public FileCallbackResult(string contentType, string fileName, Func<Stream, ActionContext, Task> callback)
    {
        _contentType = contentType;
        _fileName = fileName;
        _callback = callback;
    }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        var response = context.HttpContext.Response;
        response.ContentType = _contentType;
        response.Headers.Add("Content-Disposition", $"attachment; filename=\"{_fileName}\"");
        
        await _callback(response.Body, context);
    }
}