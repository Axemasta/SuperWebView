namespace Axemasta.SuperWebView
{
    public interface ISuperWebViewDelegate
    {
		/// <summary>
        /// Load Html, For internal use by SuperWebView platform renderers
        /// </summary>
        /// <param name="html">Html content</param>
        /// <param name="baseUrl">Base url of the current platform</param>
        /// <param name="title">Title of the html page, this will update Url property</param>
		void LoadHtml(string html, string baseUrl, string title);

        /// <summary>
        /// Load Html, For internal use by SuperWebView platform renderers
        /// </summary>
        /// <param name="html">Html content</param>
        /// <param name="baseUrl">Base url of the current platform</param>
        void LoadHtml(string html, string baseUrl);

        /// <summary>
        /// Load Url, For internal use by SuperWebView platform renderers
        /// </summary>
        /// <param name="url">Url to load</param>
        void LoadUrl(string url);
	}
}
