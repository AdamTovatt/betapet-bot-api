using System;

namespace BetapetBotApi.Helpers
{
    public class PlayTimeHelper
    {
        private Dictionary<int, SleepConfiguration> daysOfTheWeek = new Dictionary<int, SleepConfiguration>();

        public PlayTimeHelper()
        {
            daysOfTheWeek.Add(1, new SleepConfiguration(-0.1, 0.5));
            daysOfTheWeek.Add(2, new SleepConfiguration(0.3, 0.6));
            daysOfTheWeek.Add(3, new SleepConfiguration(0.5, 0.8));
            daysOfTheWeek.Add(4, new SleepConfiguration(-0.3, 0.4));
            daysOfTheWeek.Add(5, new SleepConfiguration(-1.1, 0.2));
            daysOfTheWeek.Add(6, new SleepConfiguration(-1.4, 0));
            daysOfTheWeek.Add(7, new SleepConfiguration(-0.2, 0.2));
        }

        public DateTime GetNextTimeAwake(DateTime dateTime)
        {
            Random random = new Random(dateTime.Year);
            DateTime rounded = IncreaseTime(dateTime, random);

            while (!GetAwake(rounded))
                rounded = IncreaseTime(rounded + TimeSpan.FromMinutes(random.Next(2)), random);

            return rounded + TimeSpan.FromSeconds(random.Next(59));
        }

        private DateTime IncreaseTime(DateTime time, Random random)
        {
            return RoundUp(time, TimeSpan.FromMinutes(4));
        }

        private DateTime RoundUp(DateTime dateTime, TimeSpan timeSpan)
        {
            return new DateTime((dateTime.Ticks + timeSpan.Ticks - 1) / timeSpan.Ticks * timeSpan.Ticks, dateTime.Kind);
        }

        public bool GetAwake(DateTime time)
        {
            SleepConfiguration configuration = daysOfTheWeek[((int)time.DayOfWeek)];

            double offset = configuration.Offset;
            double shift = configuration.Shift;

            double timeNow = time.Hour + (time.Minute / 60.0);

            double wakeTime = Math.Sin(((timeNow) / (5.8)) + 5.2) * 4;
            double sleepTime = Math.Sin(((timeNow) / (7.62)) + shift) * 2.5 * offset + Math.Cos(timeNow + shift) * 2 + Math.Sin(4 * timeNow + shift) * Math.Sin(4 * timeNow + shift) * 2;
            
            if (wakeTime < sleepTime || timeNow < 6)
                return false;

            return true;
        }
    }

    public class SleepConfiguration
    {
        public double Shift { get; set; }
        public double Offset { get; set; }
        public SleepConfiguration(double shift, double offset)
        {
            Shift = shift;
            Offset = offset;
        }
    }
}
