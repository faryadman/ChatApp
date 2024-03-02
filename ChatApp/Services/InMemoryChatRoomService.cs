using ChatApp.Models;

namespace ChatApp.Services
{
    public class InMemoryChatRoomService : IChatRoomService
    {
        private readonly Dictionary<Guid, ChatRoom> _roomInfo = new Dictionary<Guid, ChatRoom>();
        private readonly Dictionary<Guid, List<ChatMessage>> _messageList = new Dictionary<Guid, List<ChatMessage>>();


        public Task<Guid> CreateRoom(string connectionId)
        {
            var id = Guid.NewGuid();
            _roomInfo[id] = new ChatRoom()
            {
                OwerConnectionId = connectionId
            };

            return Task.FromResult(id);
        }

        public Task<Guid> GetRoomForConnectionId(string connectionId)
        {
            var foundRoom = _roomInfo.FirstOrDefault(x => x.Value.OwerConnectionId == connectionId);

            if (foundRoom.Key == Guid.Empty)
            {
                throw new ArgumentException("Invalid Connection ID");
            }

            return Task.FromResult(foundRoom.Key);
        }

        public async Task SetRoomName(Guid roomId, string name)
        {
            if (!_roomInfo.ContainsKey(roomId))
            {
                throw new Exception("Invalid Room Id");
            }
            _roomInfo[roomId].Name = name;

        }

        public Task AddMessage(Guid roomId, ChatMessage message)
        {
            if (!_messageList.ContainsKey(roomId))
            {
                _messageList[roomId] = new List<ChatMessage>();
            }
            _messageList[roomId].Add(message);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<ChatMessage>> GetHistoryChat(Guid roomId)
        {
            _messageList.TryGetValue(roomId, out var messages);
            messages = messages ?? new List<ChatMessage>();
            var sortMessages = messages.OrderBy(x => x.SendAt).AsEnumerable();
            return Task.FromResult(sortMessages);
        }

        public Task<IReadOnlyDictionary<Guid, ChatRoom>> GetAllRooms()
        {
            return Task.FromResult(_roomInfo as IReadOnlyDictionary<Guid, ChatRoom>);
        }
    }
}
