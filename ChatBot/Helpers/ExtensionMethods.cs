namespace ChatBot.Helpers
{
    public static class ExtensionMethods
    {
        private static Random? random;

        public static T TakeRandomElement<T>(this List<T> list)
        {
            if (random == null)
                random = new Random();

            return list[random.Next(list.Count)];
        }

        public static List<T> AddIfNotNull<T>(this List<T> list, T element)
        {
            if (element == null)
                return list;

            list.Add(element);
            return list;
        }
    }
}
