using ChatApp.Models;
using ChatApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp
{
    public class ChatHub : Hub
    {
        private static Dictionary<Guid, string> userToSupportMapping = new Dictionary<Guid, string>();
        private readonly IChatRoomService _chatRoomService;
        private readonly IHubContext<SupportHub> _supportHub;

        public ChatHub(IChatRoomService chatRoomService, IHubContext<SupportHub> supportHub)
        {
            _chatRoomService = chatRoomService;
            _supportHub = supportHub;
        }
        public override async Task OnConnectedAsync()
        {
            if (Context.User.Identity.IsAuthenticated)
            {
                await base.OnConnectedAsync();
                return;
            }
            var roomId = await _chatRoomService.CreateRoom(Context.ConnectionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());

            await Clients.Caller.SendAsync("ReceiveMessage",
                "Support",
                DateTimeOffset.UtcNow,
                "Hello ! What can we help you with today ?");

            await base.OnConnectedAsync();
        }

        public async Task SendMessage(string name, string text)
        {
            var roomId = await _chatRoomService.GetRoomForConnectionId(Context.ConnectionId);
            var message = new ChatMessage
            {
                SenderName = name,
                Text = text,
                SendAt = DateTimeOffset.Now
            };
            await _chatRoomService.AddMessage(roomId, message);
            await Clients.Group(roomId.ToString()).SendAsync("ReceiveMessage", message.SenderName, message.SendAt, message.Text);
        }

        public async Task SetName(string visitorName)
        {
            var roomName = $@"Chat with {visitorName}";
            var roomId = await _chatRoomService.GetRoomForConnectionId(Context.ConnectionId);
            await _chatRoomService.SetRoomName(roomId, roomName);
            await _supportHub.Clients.All.SendAsync("ActiveRooms", await _chatRoomService.GetAllRooms());
        }
        [Authorize]
        public async Task JoinRoom(Guid roomId)
        {
            if (roomId == Guid.Empty)
            {
                throw new Exception("Invalid Room ID");
            }
            var connectionId = Context.ConnectionId;
            userToSupportMapping[roomId] = connectionId;
            await Groups.AddToGroupAsync(connectionId, roomId.ToString());
        }
        [Authorize]
        public async Task LeaveRoom(Guid roomId)
        {
            if (roomId == Guid.Empty)
            {
                throw new Exception("Invalid Room ID");
            }
            var connectionId = Context.ConnectionId;
            userToSupportMapping.Remove(roomId);
            await Groups.RemoveFromGroupAsync(connectionId, roomId.ToString());
        }

        public async Task SendTypingNotification(Guid roomId, bool isTyping)
        {
            await Clients.Group(roomId.ToString()).SendAsync("ReceiveTypingNotification", isTyping);
        }
    }
}
