using ChatApplication.Data;
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

        private static readonly Dictionary<string, string> Users = new Dictionary<string, string>();
        Response response = new Response();
        ResponseWithoutData response2 = new ResponseWithoutData();
        //TokenUser tokenUser = new TokenUser();
        object result = new object();
        private readonly ChatAppDbContext DbContext;
        private readonly IConfiguration _configuration;


        public ChatAppHub( ChatAppDbContext dbContext)
        {
            //this._configuration = configuration;
            DbContext = dbContext;
        }


        /*private readonly ChatService _chatService;
            public ChatAppHub(ChatService chatService)
            {
                _chatService = chatService;
            }*/

            public async override Task<Task> OnConnectedAsync()
            {
                try
                {
                    Console.WriteLine("connected");
                    //string? email = Context.User.FindFirstValue(ClaimTypes.Email);
                    //await Groups.AddToGroupAsync(Context.ConnectionId, "Come2Chat");
                    //_chatService.AddUserToList(email,Context.ConnectionId); 
                    Clients.Caller.SendAsync("UserConnected");
                    DisplayOnlineUsers();
                //await Clients.All.SendAsync("UpdateOnlineUsers",_chatService.GetOnlineUsers());
                 }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return base.OnConnectedAsync();
            }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            //await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Come2Chat");
            var user = GetUserByConnectionId(Context.ConnectionId);
            RemoveUserFromList(user);
            //await DisplayOnlineUsers();

            await base.OnDisconnectedAsync(exception);
        }

        public async Task AddUserConnectionId(string email)
        {
            AddUserToList(email, Context.ConnectionId);
            await DisplayOnlineUsers();
        }
        private async Task DisplayOnlineUsers()
        {
            var onlineUsers = GetOnlineUsers();
            await Clients.All.SendAsync("UpdateOnlineUsers", onlineUsers);
        }
        public async Task SendMessage(InputMessage msg)
        {
            Console.WriteLine("SendMessage Socket fxn called");
            //string? SenderMail = Context.User.FindFirstValue(ClaimTypes.Email);
            string SenderMail = GetUserByConnectionId(Context.ConnectionId);
            var response = AddMessage(SenderMail, msg.ReceiverEmail, msg.Content);
            string ReceiverId = GetConnectionIdByUser(msg.ReceiverEmail);
            await Clients.Client(ReceiverId).SendAsync("ReceivedMessage", msg.Content);
            await Clients.Caller.SendAsync("hello");
            //handle if user is not online
        }

        public async Task CreateChat(string ConnectToMail)
        {
            Console.WriteLine("createChat fxn called");
            //string? SenderMail = Context.User.FindFirstValue(ClaimTypes.Email);
            string SenderMail = GetUserByConnectionId(Context.ConnectionId);
            var res = await AddChat(SenderMail, ConnectToMail);
            string ReceiverId = GetConnectionIdByUser(ConnectToMail);
            await Clients.Client(ReceiverId).SendAsync("ChatCreated", res);
          /*  string ReceiverId = _chatService.GetConnectionIdByUser(ConnectToMail);
            await Clients.Client(ReceiverId).SendAsync("ReceivedMessage", res);*/
        }

        public List<OutputChatMappings> GetChats()
        {
            Console.WriteLine("GetChats fxn called");
            //string? Mail = Context.User.FindFirstValue(ClaimTypes.Email);
            string Mail = GetUserByConnectionId(Context.ConnectionId);
            var res = GetChatsService(Mail);
            string ReceiverId = GetConnectionIdByUser(Mail);
            Clients.Client(ReceiverId).SendAsync("RecievedChats", res);
            return res;
        }

        public void GetChatMessages(string OtherMail, int pageNumber)
        {
            Console.WriteLine("GetChatMessages fxn called");
            //string? Mail = Context.User.FindFirstValue(ClaimTypes.Email);
            string Mail = GetUserByConnectionId(Context.ConnectionId);
            var res = GetChatMessagesService(Mail, OtherMail, pageNumber, 30);
            string ReceiverId = GetConnectionIdByUser(Mail);
            Clients.Caller.SendAsync("RecievedChatMessages", res);
            Clients.Client(ReceiverId).SendAsync("RecievedChatMessages", res);
        }





        public bool AddUserToList(string userToAdd, string connectionId)
        {
            lock (Users)
            {
                foreach (var user in Users)
                {
                    if (user.Key.ToLower() == userToAdd.ToLower())
                    {
                        return false;
                    }
                }

                Users.Add(userToAdd, connectionId);
                return true;
            }
        }

        /*public void AddUserConnectinId(string user, string connectionId)
        {
            lock (Users)
            {
                if (Users.ContainsKey(user))
                {
                    Users[user] = connectionId;
                }
            }
        }*/

        public string GetUserByConnectionId(string connectionId)
        {
            lock (Users)
            {
                return Users.Where(x => x.Value == connectionId).Select(x => x.Key).FirstOrDefault();
            }
        }

        public string GetConnectionIdByUser(string user)
        {
            lock (Users)
            {
                return Users.Where(x => x.Key == user).Select(x => x.Value).FirstOrDefault();
            }
        }

        public void RemoveUserFromList(string user)
        {
            lock (Users)
            {
                if (Users.ContainsKey(user))
                {
                    Users.Remove(user);
                }
            }
        }

        public string[] GetOnlineUsers()
        {
            lock (Users)
            {
                return Users.OrderBy(x => x.Key).Select(x => x.Key).ToArray();
            }
        }

        public OutputMessage AddMessage(string sender, string reciever, string content)
        {
            Message message = new Message()
            {
                MessageId = Guid.NewGuid(),
                SenderEmail = sender,
                ReceiverEmail = reciever,
                Content = content,
                DateTime = DateTime.Now,
                IsDeleted = false
            };
            OutputMessage res = new OutputMessage()
            {
                MessageId = message.MessageId,
                Content = message.Content,
                DateTime = message.DateTime,
                ReceiverEmail = message.ReceiverEmail,
                SenderEmail = message.SenderEmail,
            };
            DbContext.Messages.Add(message);
            DbContext.SaveChanges();
            return res;
        }

        public async Task<object> AddChat(string FirstMail, string SecondMail)
        {
            var chatdb = DbContext.ChatMappings;
            var user2 = DbContext.Users.Where(s => s.Email == SecondMail).FirstOrDefault();
            if (user2 == null)
            {
                response2.StatusCode = 400;
                response2.Message = "User you are trying to connect does not exist";
                response2.Success = true;
                return response2;
            }
            var chats = chatdb.Where(c => c.FirstEmail == FirstMail && c.SecondEmail == SecondMail).FirstOrDefault();
            if (chats == null)
            {
                chats = chatdb.Where(c => c.FirstEmail == SecondMail && c.SecondEmail == FirstMail).FirstOrDefault();
            }

            if (chats == null)
            {
                ChatMappings chatMap = new ChatMappings()
                {
                    ChatId = Guid.NewGuid(),
                    FirstEmail = FirstMail,
                    SecondEmail = SecondMail,
                    DateTime = DateTime.Now,
                    IsDeleted = false
                };

                OutputChatMappings output = new OutputChatMappings()
                {
                    ChatId = chatMap.ChatId,
                    FirstEmail = chatMap.FirstEmail,
                    SecondEmail = chatMap.SecondEmail,
                    DateTime = chatMap.DateTime,
                };
                await DbContext.ChatMappings.AddAsync(chatMap);
                await DbContext.SaveChangesAsync();
                response.Data = output;
            }
            else
            {
                OutputChatMappings output = new OutputChatMappings()
                {
                    ChatId = chats.ChatId,
                    FirstEmail = chats.FirstEmail,
                    SecondEmail = chats.SecondEmail,
                    DateTime = chats.DateTime,
                };
                response.Data = output;
            }

            response.StatusCode = 200;
            response.Message = "Chat created/exists";
            response.Success = true;

            return response;
        }

        public List<OutputChatMappings> GetChatsService(string email)
        {
            var chatMaps = DbContext.ChatMappings.ToList();
            chatMaps = chatMaps.Where(s => (s.FirstEmail == email || s.SecondEmail == email)).ToList();
            chatMaps = chatMaps.OrderBy(m => m.DateTime).Select(m => m).ToList();
            List<OutputChatMappings> res = new List<OutputChatMappings>();

            foreach (var cm in chatMaps)
            {
                var user1 = DbContext.Users.Where(s => s.Email == cm.FirstEmail).FirstOrDefault();
                var user2 = DbContext.Users.Where(s => s.Email == cm.SecondEmail).FirstOrDefault();
                OutputChatMappings output = new OutputChatMappings()
                {
                    ChatId = cm.ChatId,
                    FirstEmail = cm.FirstEmail,
                    FirstName1 = user1.FirstName,
                    LastName1 = user1.LastName,
                    SecondEmail = cm.SecondEmail,
                    FirstName2 = user2.FirstName,
                    LastName2 = user2.LastName,
                    DateTime = cm.DateTime,
                };
                res.Add(output);
            }

            return res;
        }

        public List<OutputMessage> GetChatMessagesService(string email, string otherEmail, int pageNumber, int skipLimit)
        {
            var messages = DbContext.Messages.AsQueryable();
            //chatMaps = chatMaps.Where(s => (s.FirstEmail == email || s.SecondEmail == email)).ToList();
            messages = messages.Where(m => (m.SenderEmail == email && m.ReceiverEmail == otherEmail) || (m.SenderEmail == otherEmail && m.ReceiverEmail == email));

            messages = messages.OrderBy(m => m.DateTime).Select(m => m);
            messages = messages.Skip((pageNumber - 1) * skipLimit).Take(skipLimit);

            List<OutputMessage> res = new List<OutputMessage>();

            foreach (var msg in messages)
            {
                OutputMessage output = new OutputMessage()
                {
                    MessageId = msg.MessageId,
                    Content = msg.Content,
                    DateTime = msg.DateTime,
                    ReceiverEmail = msg.ReceiverEmail,
                    SenderEmail = msg.SenderEmail
                };
                res.Add(output);
            }

            return res;
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
