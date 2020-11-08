using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Caelicus.Helpers
{
    /// <summary>
    /// Static utility class to create and save files
    /// </summary>
    public static class FileUtilities
    {
        /// <summary>
        /// Saves a byte array to a local file using the JavaScript runtime
        /// </summary>
        /// <param name="js">The JavaScript runtime</param>
        /// <param name="filename">The filename of the file to create</param>
        /// <param name="data">The data of the file</param>
        /// <returns></returns>
        public static async Task SaveAs(IJSRuntime js, string filename, byte[] data)
        {
            await js.InvokeAsync<object>(
                "saveAsFile",
                filename,
                Convert.ToBase64String(data));
        }

        /// <summary>
        /// Creates a byte array representing a zip file containing multiple files
        /// </summary>
        /// <param name="values">A dictionary of file names and content</param>
        /// <returns></returns>
        public static byte[] CreateZipFile(Dictionary<string, string> values)
        {
            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var (key, value) in values)
                {
                    var file = archive.CreateEntry(key + ".json");
                    using var entryStream = file.Open();
                    using var streamWriter = new StreamWriter(entryStream);
                    streamWriter.Write(value);
                }
            }

            return memoryStream.ToArray();
        }
    }
}
