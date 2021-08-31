using System;

namespace Schedulebot.Users.Utils
{
    public readonly struct MessageInfo
    {
        public long Id { get; init; }
        public DateTime Time { get; init; }

        public MessageInfo(long id, DateTime time)
        {
            Id = id;
            Time = time;
        }
    }
}
