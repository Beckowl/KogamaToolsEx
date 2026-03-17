namespace KogamaToolsEx.Plugin.Misc
{
    internal static class GreetingMessage
    {
        [InvokeOnInit]
        private static void DoGreeting()
        {
            const string msg =
                "<color=cyan>Welcome to {0} v{1}!</color>\n\n" +
                "The quick brown fox jumps over the lazy dog.";

            TextCommand.NotifyUser(string.Format(msg, PluginMeta.NAME, PluginMeta.VERSION));
        }
    }
}
