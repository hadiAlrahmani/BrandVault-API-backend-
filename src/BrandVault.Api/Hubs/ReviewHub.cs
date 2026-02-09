namespace BrandVault.Api.Hubs;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

/// <summary>
/// SignalR hub for real-time review notifications.
/// Agency users connect and join workspace groups. When a client posts
/// a comment or approval via the public review link, connected users
/// in that workspace group get notified instantly.
///
/// Socket.IO equivalent:
///   io.on('connection', (socket) => {
///     socket.on('JoinWorkspace', (id) => socket.join(`workspace_${id}`));
///     socket.on('LeaveWorkspace', (id) => socket.leave(`workspace_${id}`));
///   });
///
///   // Later, when a client comments:
///   io.to(`workspace_${id}`).emit('NewComment', commentData);
/// </summary>
[Authorize]
public class ReviewHub : Hub
{
    /// <summary>
    /// Join a workspace group to receive real-time notifications for that workspace.
    /// Like: socket.join(`workspace_${workspaceId}`)
    /// </summary>
    public async Task JoinWorkspace(string workspaceId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"workspace_{workspaceId}");
    }

    /// <summary>
    /// Leave a workspace group to stop receiving notifications.
    /// Like: socket.leave(`workspace_${workspaceId}`)
    /// </summary>
    public async Task LeaveWorkspace(string workspaceId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"workspace_{workspaceId}");
    }
}
