using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftHSMv2ForTesting
{
    internal static class GzipHelper
    {
        public static void ExtractContentFileTo(string contentFileName, string exportFilePath)
        {
            string embededFileName = string.Concat("SoftHSMv2ForTesting.Content.Windows.", contentFileName);

            using (Stream stream = typeof(GzipHelper).Assembly.GetManifestResourceStream(embededFileName))
            {
                using (FileStream wfs = new FileStream(exportFilePath, FileMode.Create, FileAccess.Write))
                {
                    using (GZipStream gziped = new GZipStream(stream, CompressionMode.Decompress))
                    {
                        gziped.CopyTo(wfs);
                    }
                }
            }
        }
    }
}
