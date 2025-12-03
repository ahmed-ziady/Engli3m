using Microsoft.AspNetCore.SignalR;

namespace Engli3m.Infrastructure.Hubs
{
    public class NotificationHub : Hub<INotificationClient>
    {
        public override Task OnConnectedAsync()
        {
            Clients.Client(Context.ConnectionId).ReceiveNotification(



                $"Welcome to the Notification Hub! Your connection ID is {Context.ConnectionId}.");


            return base.OnConnectedAsync();
        }


        //public async Task SendNotification(string message)
        //{
        //    await Clients.All.ReceiveNotification(message);
        //}
        //public async Task SendNotification(string message, string userId)
        //{
        //    await Clients.User(userId).ReceiveNotification(message);
        //}
        //public async Task SendNotification(string message, string userId, string groupName)
        //{
        //    await Clients.Group(groupName).ReceiveNotification(message, userId);
        //}
    }
    public interface INotificationClient
    {
        Task ReceiveNotification(string message);
        Task ReceiveNotification(string message, string userId);
        Task ReceiveNotification(string message, string userId, string groupName);
    }
}
