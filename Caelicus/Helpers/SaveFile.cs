using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace BlazorApp.Helpers
{
    /// <summary>
    /// Static utility class to create and save files
    /// </summary>
    public static class SaveFile
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
    }
}
