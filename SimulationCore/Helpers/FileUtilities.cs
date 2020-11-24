using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationCore.Helpers
{
    public class FileUtilities
    {
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
