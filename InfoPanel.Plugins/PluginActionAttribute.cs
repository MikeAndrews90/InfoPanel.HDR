namespace InfoPanel.Plugins
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class PluginActionAttribute : Attribute
    {
        public string DisplayName { get; }

        public PluginActionAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }
}
