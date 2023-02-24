namespace ChatBot.Models
{
    /// <summary>
    /// Class for containing bot training progress
    /// </summary>
    public class BotTrainingProgress
    {
        /// <summary>
        /// The total tasks that are needed to finish training the bot
        /// </summary>
        public int TotalTasks { get; set; }

        /// <summary>
        /// The tasks that have been completed so far
        /// </summary>
        public int CompletedTasks { get; set; }

        /// <summary>
        /// The current task that is being performed
        /// </summary>
        public string? CurrentTask { get; set; }

        /// <summary>
        /// Public constructor for creating an instance of the object
        /// </summary>
        /// <param name="totalTasks">The total amount of tasks that needs to be performed</param>
        public BotTrainingProgress(int totalTasks)
        {
            TotalTasks = totalTasks;
        }

        /// <summary>
        /// Will get the object as a readable string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} ({1}/{2})", CurrentTask, (CompletedTasks + 1), TotalTasks);
        }
    }
}
