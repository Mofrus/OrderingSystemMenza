using Microsoft.AspNetCore.Http;
using UTB.Minute.WebApi.Services;

namespace UTB.Minute.WebApi.Endpoints;

public static class NotificationsEndpoints
{
    public static void MapNotificationsEndpoints(this WebApplication app)
    {
        app.MapGet("/notifications/stream", async (HttpContext context, NotificationService notificationService) =>
        {
            context.Response.Headers.Append("Content-Type", "text/event-stream");
            context.Response.Headers.Append("Cache-Control", "no-cache");
            context.Response.Headers.Append("Connection", "keep-alive");

            var reader = notificationService.Subscribe();

            try
            {
                await foreach (var message in reader.ReadAllAsync(context.RequestAborted))
                {
                    await context.Response.WriteAsync($"data: {message}\n\n", context.RequestAborted);
                    await context.Response.Body.FlushAsync(context.RequestAborted);
                }
            }
            catch (OperationCanceledException)
            {
                // Client disconnected
            }
            finally
            {
                notificationService.Unsubscribe(reader);
            }
        }).WithName("GetNotificationStream");
    }
}
