using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace ServerHub
{
    public class ChatHub : Hub
    {
        //private readonly static IDictionary<string,string> onlineUsers = 
        //    new Dictionary<string,string>();
        private static int onlineUsersCount = 0;

        public async Task SendMessage(string message, string user)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);            
        }

        //public async void SendUser(string user)
        //{
        //    await Task.Run(() => {
        //        if (!onlineUsers.Values.Contains(user))
        //            onlineUsers.TryAdd(Context.ConnectionId, user);
        //    });          
        //}

        public async override Task OnConnectedAsync()
        {
            //broadcast notification message to all clients (except current client/user) letting them know that a new user just joined in/connected.            
            //await Clients.Client(Context.ConnectionId).SendAsync("GetUser");
            onlineUsersCount++;
            var shortUserId = Context.ConnectionId.Substring(Context.ConnectionId.Length - 6);
            await Clients.All.SendAsync("ClientConnected", $"User {shortUserId} has connected...", onlineUsersCount);
        }

        public async override Task OnDisconnectedAsync(Exception exception)
        {
            //var disconnectedUser = onlineUsers[Context.ConnectionId];
            //if (onlineUsers.ContainsKey(Context.ConnectionId))
            //    onlineUsers.Remove(Context.ConnectionId);

            onlineUsersCount--;
            var shortUserId = Context.ConnectionId.Substring(Context.ConnectionId.Length-6);
            await Clients.AllExcept(Context.ConnectionId)
                         .SendAsync("ClientDisconnected", $"User {shortUserId} disconnected...", onlineUsersCount);
        }
    }
}
