using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Models
{
    public class BotTrainingProgress
    {
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public string? CurrentTask { get; set; }

        public BotTrainingProgress(int totalTasks)
        {
            TotalTasks = totalTasks;
        }

        public override string ToString()
        {
            return string.Format("{0} ({1}/{2})", CurrentTask, (CompletedTasks + 1), TotalTasks);
        }
    }
}
