namespace Schedulebot.Mapper.Utils
{
    public struct UserMapping
    {
        public UserMapping(int course, int groupIndex)
        {
            Course = course;
            GroupIndex = groupIndex;
        }

        public int Course { get; }
        public int GroupIndex { get; }
    }
}