using VkNet.Utils;

namespace Schedulebot.Commands
{
    class Command
    {
        public CommandType Type { get; }
        public VkParameters VkParameters { get; }
        public long? UserId { get; }

        public Command(CommandType type, VkParameters vkParameters, long? userId = null)
        {
            Type = type;
            VkParameters = vkParameters;
            UserId = userId;
        }
    }
}
