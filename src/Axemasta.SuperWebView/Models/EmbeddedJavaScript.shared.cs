namespace Axemasta.SuperWebView
{
    public class EmbeddedJavaScript : JavaScript
    {
        public string Path { get; }

        public string AssemblyName { get; }

        public EmbeddedJavaScript(string name, string path, string assemblyName)
        {
            this.Name = name;
            this.Path = path;
            this.AssemblyName = assemblyName;
        }

        protected override string Load()
        {
            return EmbeddedResourceHelper.Load(Path, AssemblyName);
        }
    }
}
