using System;
namespace Axemasta.SuperWebView
{
	/// <summary>
	/// Raw Java Script
	/// </summary>
	public class RawJavaScript : JavaScript
	{
		public string Content { get; set; }

		public RawJavaScript(string name, string content)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name), "Script must have a name");

			this.Name = name;
			this.Content = content;
		}

		protected override string Load()
		{
			// Sanitise javascript here

			return Content;
		}
	}
}
