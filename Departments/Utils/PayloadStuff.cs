using System;
using System.Collections.Generic;
using System.Text;

namespace Schedulebot.Departments.Utils
{
    public class PayloadStuff
    {
        public string Command { get; set; } = "";
        public int? Menu { get; set; } = null;
        public int Course { get; set; } = -1;
        public string Group { get; set; } = "";
        public int Page { get; set; } = -1;
    }
}
