using Ardalis.GuardClauses;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Axemasta.SuperWebView
{
    /// <summary>
    /// Embedded Resource Helper
    /// </summary>
    public static class EmbeddedResourceHelper
    {
        /// <summary>
        /// Load Resource From Assembly
        /// </summary>
        /// <param name="resourceName">Name of the resource to load</param>
        /// <param name="assemblyName">Name of the assembly the resource coincides</param>
        /// <returns>The embedded resource content as a string</returns>
        public static string Load(string resourceName, string assemblyName)
        {
            resourceName = Guard.Against.NullOrEmpty(resourceName, nameof(resourceName));
            assemblyName = Guard.Against.NullOrEmpty(assemblyName, nameof(assemblyName));

            Assembly assembly = Assembly.Load(assemblyName);

            if (assembly is null)
                throw new InvalidOperationException($"Could not load assembly: {assemblyName}");

            string[] assemblyResources = assembly.GetManifestResourceNames();

            string resource = assemblyResources.FirstOrDefault(r => r == resourceName);

            if (resource is null)
                throw new InvalidOperationException($"No assembly resource found in assembly: {assembly.FullName} with name: {resourceName}");

            using (var stream = assembly.GetManifestResourceStream(resource))
            {
                if (stream is null)
                    return null;

                using (var streamReader = new StreamReader(stream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }
    }
}
