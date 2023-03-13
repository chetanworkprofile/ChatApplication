using ChatApplication.Models;
using ChatApplication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace ChatApplication.Hubs
{
        //[Authorize  (Roles = "login")]
        public class ChatAppHub : Hub
        {
            private readonly ChatService _chatService;
            public ChatAppHub(ChatService chatService)
            {
                _chatService = chatService;
            }

            public override async Task OnConnectedAsync()
            {
                string? email = Context.User.FindFirstValue(ClaimTypes.Email);
                //await Groups.AddToGroupAsync(Context.ConnectionId, "Come2Chat");
                _chatService.AddUserToList(email,Context.ConnectionId); 
                await Clients.Caller.SendAsync("UserConnected");
                await Clients.All.SendAsync("UpdateOnlineUsers",_chatService.GetOnlineUsers());
            }

            public override async Task OnDisconnectedAsync(Exception exception)
            {
                //await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Come2Chat");
                var user = _chatService.GetUserByConnectionId(Context.ConnectionId);
                _chatService.RemoveUserFromList(user);
                //await DisplayOnlineUsers();

                await base.OnDisconnectedAsync(exception);
            }

        public async Task SendMessage(InputMessage msg)
        {
           string? SenderMail = Context.User.FindFirstValue(ClaimTypes.Email);
           var response = await _chatService.AddMessage(SenderMail, msg.ReceiverEmail,msg.Content);
           string ReceiverId =  _chatService.GetConnectionIdByUser(msg.ReceiverEmail);
           OutputMessage res = new OutputMessage()
           {
               MessageId = response.MessageId,
               Content= response.Content,
               DateTime= response.DateTime,
               ReceiverEmail = response.ReceiverEmail,
               SenderEmail = SenderMail
           };
            await Clients.Client(ReceiverId).SendAsync("ReceivedMessage",res);
        }
        /*
            public async Task AddUserConnectionId(string name)
            {
                _chatService.AddUserConnectinId(name, Context.ConnectionId);
                await DisplayOnlineUsers();
            }

            public async Task ReceiveMessage(MessageDto message)
            {
                await Clients.Group("Come2Chat").SendAsync("NewMessage", message);
            }

            public async Task CreatePrivateChat(MessageDto message)
            {
                string privateGroupName = GetPrivateGroupName(message.From, message.To);
                await Groups.AddToGroupAsync(Context.ConnectionId, privateGroupName);
                var toConnectionId = _chatService.GetConnectionIdByUser(message.To);
                await Groups.AddToGroupAsync(toConnectionId, privateGroupName);

                // opening private chatbox for the other end user
                await Clients.Client(toConnectionId).SendAsync("OpenPrivateChat", message);
            }

            public async Task RecivePrivateMessage(MessageDto message)
            {
                string privateGroupName = GetPrivateGroupName(message.From, message.To);
                await Clients.Group(privateGroupName).SendAsync("NewPrivateMessage", message);
            }

            public async Task RemovePrivateChat(string from, string to)
            {
                string privateGroupName = GetPrivateGroupName(from, to);
                await Clients.Group(privateGroupName).SendAsync("CloseProivateChat");

                await Groups.RemoveFromGroupAsync(Context.ConnectionId, privateGroupName);
                var toConnectionId = _chatService.GetConnectionIdByUser(to);
                await Groups.RemoveFromGroupAsync(toConnectionId, privateGroupName);
            }

            private async Task DisplayOnlineUsers()
            {
                var onlineUsers = _chatService.GetOnlineUsers();
                await Clients.Groups("Come2Chat").SendAsync("OnlineUsers", onlineUsers);
            }

            private string GetPrivateGroupName(string from, string to)
            {
                // from: john, to: david  "david-john"
                var stringCompare = string.CompareOrdinal(from, to) < 0;
                return stringCompare ? $"{from}-{to}" : $"{to}-{from}";
            }*/
        }
}
