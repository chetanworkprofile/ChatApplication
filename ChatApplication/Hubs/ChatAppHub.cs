using ChatApplication.Controllers;
using ChatApplication.Data;
using ChatApplication.Models;
using ChatApplication.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace ChatApplication.Hubs
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ChatAppHub : Hub
        {

        private static readonly Dictionary<string, string> Users = new Dictionary<string, string>();
        Response response = new Response();
        ResponseWithoutData response2 = new ResponseWithoutData();
        //TokenUser tokenUser = new TokenUser();
        object result = new object();
        private readonly ChatAppDbContext DbContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;


        public ChatAppHub( ChatAppDbContext dbContext, ILogger<AuthController> logger)
        {
            //this._configuration = configuration;
            DbContext = dbContext;
            _logger = logger;
        }


        /*private readonly ChatService _chatService;
         public ChatAppHub(ChatService chatService)
         {
             _chatService = chatService;
         }*/

        public async override Task<Task> OnConnectedAsync()             // on connected server calls client g=function to send email that is added in dictionary
        {
            try
            {
                _logger.LogInformation("New User connected to socket");
                Console.WriteLine("connected");
                //string? email = Context.User.FindFirstValue(ClaimTypes.Email);
                //await Groups.AddToGroupAsync(Context.ConnectionId, "Come2Chat");
                //_chatService.AddUserToList(email,Context.ConnectionId); 
                Clients.Caller.SendAsync("UserConnected");
                Clients.All.SendAsync("refresh");

                //refereh function is called on client side to alarm them to call Online users function and get updated list of online users

                //DisplayOnlineUsers();
                //await Clients.All.SendAsync("UpdateOnlineUsers",_chatService.GetOnlineUsers());
            }
            catch (Exception ex)
            {
                _logger.LogError("Internal server error ", ex.Message);
            }
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation("user disconnected");
            //await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Come2Chat");
            var user = GetUserByConnectionId(Context.ConnectionId);
            RemoveUserFromList(user);
            Clients.All.SendAsync("refresh");
            //await OnlineUsers();
            await base.OnDisconnectedAsync(exception);
        }
        public void refesh()
        {
            Clients.All.SendAsync("refresh");
        }
        public async Task AddUserConnectionId(string email)
        {
            _logger.LogInformation("User added to online dictionary ",email);
            AddUserToList(email.ToLower(), Context.ConnectionId);
            //await OnlineUsers();
        }
        /*private async Task DisplayOnlineUsers()
        {
            var onlineUsers = GetOnlineUsers();
            await Clients.All.SendAsync("UpdateOnlineUsers", onlineUsers);
        }*/
        public async Task OnlineUsers()
        {
            
            _logger.LogInformation("OnlineUsers method started");
            var onlineUsers = GetOnlineUsers();
            var httpContext = Context.GetHttpContext();
            var user1 = httpContext.User;
            var loggedInEmail = user1.FindFirst(ClaimTypes.Email)?.Value;
            //string loggedInEmail = GetUserByConnectionId(Context.ConnectionId);
            //var getChats = GetChatsService(loggedInEmail);
            //------
            var chatMaps = DbContext.ChatMappings.Where(s => (s.FirstEmail == loggedInEmail) || (s.SecondEmail == loggedInEmail)).OrderByDescending(s => s.DateTime).ToList();
            //chatMaps = chatMaps.Where(s => (s.FirstEmail == loggedInEmail) || (s.SecondEmail == loggedInEmail)).OrderByDescending(s=>s.DateTime).ToList();
            Console.WriteLine(chatMaps.Count());
            
            List<string> res = new List<string>();

            foreach ( var chatMap in chatMaps)
            {
                if (chatMap.FirstEmail != loggedInEmail)
                {
                    res.Add(chatMap.FirstEmail);
                }
                else if(chatMap.FirstEmail == loggedInEmail)
                {
                    res.Add(chatMap.SecondEmail);
                }
            }
            Console.WriteLine(res.Count());
            List<ActiveUsers> activeList = new List<ActiveUsers>();
            //var users = DbContext.Users.ToList();
            int len = res.Count();
            for (int i=0;i<len;i++)
            {
                Console.WriteLine(res[i]);
                var user = DbContext.Users.Where(s => (s.Email == res[i])).Select(s=>s).First();
                Console.WriteLine(user);
                /*if(user != null)
                {
                    Console.WriteLine(user.Result.FirstName);
                }*/
                    ActiveUsers a = new ActiveUsers
                    {
                        Email = res[i],
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        IsActive = false,
                        PathToPic = user.PathToProfilePic
                    };

                    if (onlineUsers.Contains(res[i])) { a.IsActive = true; }
                    else { a.IsActive = false; }
                    activeList.Add(a);
                    Console.WriteLine(a);
            }
            /*foreach (var user in activeList)
            {
                var id = GetConnectionIdByUser(user.Email);
                var temp = activeList;
                if (id == null)
                {
                    continue;
                }
                temp.Remove(temp.Where(x => x.Email == user.Email).FirstOrDefault());
                await Clients.Client(id).SendAsync("UpdateOnlineUsers", temp);
            }*/
            activeList.Remove(activeList.Where(x => x.Email == loggedInEmail).FirstOrDefault());
            await Clients.Caller.SendAsync("UpdateOnlineUsers", activeList);

            //return activeList;
        }
        public async Task SendMessage(InputMessage msg,string? PathToFileAttachement = "empty")
        {
            _logger.LogInformation("SendMessage method started ");
            if (msg.ReceiverEmail == "" || msg.ReceiverEmail == null)
            {
                return;
            }
            //string? SenderMail = Context.User.FindFirstValue(ClaimTypes.Email);
            var httpContext = Context.GetHttpContext();
            var user1 = httpContext.User;
            var SenderMail = user1.FindFirst(ClaimTypes.Email)?.Value;
            //string SenderMail = GetUserByConnectionId(Context.ConnectionId);
            //string path = msg.PathToFileAttachement;

            //var response = AddMessage(SenderMail, msg.ReceiverEmail, msg.Content,msg.Type,msg.PathToFileAttachement);
            
            Console.WriteLine(msg);
            Message message = new Message()
            {
                MessageId = Guid.NewGuid(),
                SenderEmail = SenderMail,
                ReceiverEmail = msg.ReceiverEmail,
                Content = msg.Content,
                DateTime = DateTime.Now,
                Type = msg.Type,
                PathToFileAttachement = PathToFileAttachement,
                IsDeleted = false
            };
            _logger.LogInformation(message.ToString());
            
            await DbContext.Messages.AddAsync(message);
            
            var chatmap = DbContext.ChatMappings.Where(s=>(s.FirstEmail== msg.ReceiverEmail && s.SecondEmail==SenderMail) || (s.FirstEmail == SenderMail && s.SecondEmail == msg.ReceiverEmail)).FirstOrDefault();
            chatmap.DateTime = DateTime.Now;
            await DbContext.SaveChangesAsync();


            string ReceiverId = GetConnectionIdByUser(msg.ReceiverEmail);
            var fileName = PathToFileAttachement.Split("/").Last();
            var sender = DbContext.Users.Where(x => x.Email == SenderMail).First();
            var receiver = DbContext.Users.Where(x => x.Email == msg.ReceiverEmail).First();
            RecevierMessage sendMsg = new RecevierMessage()
            {
                SenderEmail = SenderMail,
                SenderPicPath = sender.PathToProfilePic,
                ReceiverEmail = msg.ReceiverEmail,
                ReceiverPicPath = receiver.PathToProfilePic,
                Content = msg.Content,
                Type = msg.Type,
                DateTime = message.DateTime,
                PathToFileAttachement= PathToFileAttachement,
                FileName = fileName
            };
            
            await Clients.Caller.SendAsync("ReceivedMessage", sendMsg);
            if (ReceiverId!=null)
            {
                await Clients.Client(ReceiverId).SendAsync("ReceivedMessage", sendMsg); 
            }
            refesh();
            return;
            //await Clients.Caller.SendAsync("ReceivedMessage","helo sender");
            //handle if user is not online
        }

        public async Task CreateChat(string ConnectToMail)
        {
            Console.WriteLine("createChat fxn called");
            //string? SenderMail = Context.User.FindFirstValue(ClaimTypes.Email);
            //string SenderMail = GetUserByConnectionId(Context.ConnectionId);
            var httpContext = Context.GetHttpContext();
            var user1 = httpContext.User;
            var SenderMail = user1.FindFirst(ClaimTypes.Email)?.Value;
            string ReceiverId = GetConnectionIdByUser(ConnectToMail);
            if (SenderMail==ConnectToMail)
            {
                await Clients.Caller.SendAsync("ChatCreated", "Can't Connect to same email");
                return;
            }
            var res = await AddChat(SenderMail, ConnectToMail);
            await Clients.Client(ReceiverId).SendAsync("ChatCreated", res);
            await Clients.Caller.SendAsync("ChatCreated", res);
            await OnlineUsers();
            /*  string ReceiverId = _chatService.GetConnectionIdByUser(ConnectToMail);
              await Clients.Client(ReceiverId).SendAsync("ReceivedMessage", res);*/
        }

        public List<OutputChatMappings> GetChats()
        {
            _logger.LogInformation("GetChats fxn called");
            //string? Mail = Context.User.FindFirstValue(ClaimTypes.Email);
            //string Mail = GetUserByConnectionId(Context.ConnectionId);
            var httpContext = Context.GetHttpContext();
            var user1 = httpContext.User;
            var Mail = user1.FindFirst(ClaimTypes.Email)?.Value;
            var res = GetChatsService(Mail);
            string ReceiverId = GetConnectionIdByUser(Mail);
            Clients.Client(ReceiverId).SendAsync("RecievedChats", res);
            return res;
        }

        public List<OutputMessage> GetChatMessages(string OtherMail, int pageNumber)
        {
            _logger.LogInformation("GetChatMessages fxn called");
            //string? Mail = Context.User.FindFirstValue(ClaimTypes.Email);
            //string Mail = GetUserByConnectionId(Context.ConnectionId);
            var httpContext = Context.GetHttpContext();
            var user1 = httpContext.User;
            var Mail = user1.FindFirst(ClaimTypes.Email)?.Value;
            List<OutputMessage> res = GetChatMessagesService(Mail, OtherMail, pageNumber, 30);
            string ReceiverId = GetConnectionIdByUser(OtherMail);
            Clients.Caller.SendAsync("RecievedChatMessages", res);
            //Clients.Client(ReceiverId).SendAsync("RecievedChatMessages", res);
            return res;
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

        public  void RemoveUserFromList(string user)
        {
            lock (Users)
            {
                if (Users.ContainsKey(user))
                {
                    Users.Remove(user);
                }
            }
        }

        //get users email in dictionary
        public string[] GetOnlineUsers()
        {
            lock (Users)
            {
                return Users.OrderBy(x => x.Key).Select(x => x.Key).ToArray();
            }
        }


        //------------------------------------------------------------------------------------------------------------------//
        //----------------------------service functions-------------------------------------------------------------------//

        //create new message and add in database
        public OutputMessage AddMessage(string sender, string reciever, string content,int type, string path)
        {
            Message message = new Message()
            {
                MessageId = Guid.NewGuid(),
                SenderEmail = sender,
                ReceiverEmail = reciever,
                Content = content,
                DateTime = DateTime.Now,
                Type = type,
                PathToFileAttachement = path,
                IsDeleted = false
            };
            Console.WriteLine(message);
            Console.WriteLine("path" + path);
            OutputMessage res = new OutputMessage()
            {
                MessageId = message.MessageId,
                Content = message.Content,
                DateTime = message.DateTime,
                ReceiverEmail = message.ReceiverEmail,
                SenderEmail = message.SenderEmail,
                Type= type,
                PathToFileAttachement= path,
            };
            DbContext.Messages.Add(message);
            DbContext.SaveChanges();
            return res;
        }

        // create new chat mapping if exist send it
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

                await DbContext.ChatMappings.AddAsync(chatMap);
                await DbContext.SaveChangesAsync();
                chats = chatMap;
               /* response.Data = output;*/
            }
            var user1 = DbContext.Users.Where(s => s.Email == chats.FirstEmail).FirstOrDefault();
            user2 = DbContext.Users.Where(s => s.Email == chats.SecondEmail).FirstOrDefault();
            OutputChatMappings output = new OutputChatMappings()
            {
                ChatId = chats.ChatId,
                FirstEmail = chats.FirstEmail,
                FirstName1 = user1.FirstName,
                LastName1 = user1.LastName,
                SecondEmail = chats.SecondEmail,
                FirstName2= user2.FirstName,
                LastName2= user2.LastName,
                DateTime = chats.DateTime,
            };
            response.Data = output;
            response.StatusCode = 200;
            response.Message = "Chat created/exists";
            response.Success = true;

            return response;
        }

        //function invoked to get all chat mappings created for a particular user
        public List<OutputChatMappings> GetChatsService(string email)
        {
            var chatMaps = DbContext.ChatMappings.ToList();
            chatMaps = chatMaps.Where(s => (s.FirstEmail == email) || (s.SecondEmail==email)).ToList();
            //var chatMaps2 = chatMaps.Where(s => (s.SecondEmail == email)).ToList();
            /*chatMaps = chatMaps.OrderBy(m => m.DateTime).Select(m => m).ToList();
            chatMaps2 = chatMaps2.OrderBy(m => m.DateTime).Select(m => m).ToList();*/
            chatMaps.Remove(chatMaps.Where(s=>s.FirstEmail==s.SecondEmail).FirstOrDefault());
            List<OutputChatMappings> res = new List<OutputChatMappings>();
            OutputChatMappings output = new OutputChatMappings() { };
            foreach (var cm in chatMaps)
            {
                var user1 = DbContext.Users.Where(s => s.Email == cm.FirstEmail).FirstOrDefault();
                var user2 = DbContext.Users.Where(s => s.Email == cm.SecondEmail).FirstOrDefault();


                    output.ChatId = cm.ChatId;
                    output.FirstEmail = cm.FirstEmail;
                    output.FirstName1 = user1.FirstName;
                    output.LastName1 = user1.LastName;
                    output.SecondEmail = cm.SecondEmail;
                    output.FirstName2 = user2.FirstName;
                    output.LastName2 = user2.LastName;
                    output.DateTime = cm.DateTime;
                
                res.Add(output);
            }
           
            res = res.OrderByDescending(x=>x.DateTime).ToList();
            Console.WriteLine(res.Count);

            return res;
        }

        // function invoked to get previous chat between two users
        public List<OutputMessage> GetChatMessagesService(string email, string otherEmail, int pageNumber, int skipLimit)
        {
            //var messages = DbContext.Messages.AsQueryable();
            var messages = DbContext.Messages.ToList();
            //chatMaps = chatMaps.Where(s => (s.FirstEmail == email || s.SecondEmail == email)).ToList();
            messages = messages.Where(m => (m.SenderEmail == email && m.ReceiverEmail == otherEmail) || (m.SenderEmail == otherEmail && m.ReceiverEmail == email)).ToList();

            messages = messages.OrderByDescending(m => m.DateTime).Select(m => m).ToList();
            messages = messages.Skip((pageNumber - 1) * skipLimit).Take(skipLimit).ToList();

            List<OutputMessage> res = new List<OutputMessage>();
            
            foreach (var msg in messages)
            {
                var sender = DbContext.Users.Where(x => x.Email == msg.SenderEmail).First();
                var receiver = DbContext.Users.Where(x => x.Email == msg.ReceiverEmail).First();
                var fileName  = msg.PathToFileAttachement.Split("/").Last();
                OutputMessage output = new OutputMessage()
                {
                    MessageId = msg.MessageId,
                    Content = msg.Content,
                    DateTime = msg.DateTime,
                    ReceiverEmail = msg.ReceiverEmail,
                    ReceiverPicPath = receiver.PathToProfilePic,
                    SenderEmail = msg.SenderEmail,
                    SenderPicPath = sender.PathToProfilePic,
                    Type= msg.Type,
                    PathToFileAttachement= msg.PathToFileAttachement,
                    FileName= fileName,
                };
                res.Add(output);
            }
            res.Reverse();
            return res;
        }
        //-----------------------------------------------------------------------------------------------------------------//
    }
}
