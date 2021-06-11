namespace Axemasta.SuperWebView
{
    /// <summary>
    /// Java Script
    /// </summary>
    public abstract class JavaScript
    {
        public string Name { get; set; }

        protected abstract string Load();

        public bool TryLoadScript(out string content)
        {
            content = null;

            try
            {
                content = Load();

                return !string.IsNullOrEmpty(content);
            }
            catch
            {
                return false;
            }
        }
    }
}
