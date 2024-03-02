using ChatApp.Models;
using ChatApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp
{
    [Authorize]
    public class SupportHub : Hub
    {
        private readonly IChatRoomService _chatRoomService;
        private readonly IHubContext<ChatHub> _chatHub;

        public SupportHub(IChatRoomService chatRoomService, IHubContext<ChatHub> chatHub)
        {
            _chatRoomService = chatRoomService;
            _chatHub = chatHub;
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("ActiveRooms", await _chatRoomService.GetAllRooms());
            await base.OnConnectedAsync();
        }

        public async Task SendSupportMessage(Guid roomId, string text)
        {
            var message = new ChatMessage
            {
                SendAt = DateTimeOffset.Now,
                SenderName = Context.User.Identity.Name,
                Text = text
            };
            await _chatRoomService.AddMessage(roomId, message);
            await _chatHub.Clients.Groups(roomId.ToString())
                .SendAsync("ReceiveMessage", message.SenderName, message.SendAt, message.Text);
        }

        public async Task LoadHistory(Guid roomId)
        {
            var history = await _chatRoomService.GetHistoryChat(roomId);
            await Clients.Caller.SendAsync("ReceiveMessages", history);
        }

    }
}
