namespace PaperMagicTracker.Classes
{
    public static class GlobalStaticResources
    {
        public static Game CurrentGame { get; set; }

        public static HttpClient Client { get; set; }

        /// <summary>
        /// Just to circumvent the unbound breakpoints
        /// </summary>
        /// <param name="arg"></param>
        public static void WeirdDebug(dynamic arg)
        {
            Console.WriteLine("Weird Debug");
        }
    }
}

