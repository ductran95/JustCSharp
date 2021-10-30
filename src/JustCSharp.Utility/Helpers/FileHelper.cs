using System.IO;
using System.Linq;
using System.Web;

namespace JustCSharp.Utility.Helpers
{
    public static class FileHelper
    {
        public static string EncodePath(string path)
        {
            return HttpUtility.UrlEncode(path);
        }

        public static string DecodePath(string encodedPath)
        {
            return HttpUtility.UrlDecode(encodedPath);
        }

        /// <summary>
        /// Get file extension without . and in lowercase
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetExtension(string fileName)
        {
            var ext = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(ext))
            {
                return ext;
            }

            return ext.ToLower().Replace(".", string.Empty);
        }

        public static string GetMIMEType(string fileName)
        {
            return MimeTypes.GetMimeType(fileName);
        }

        public static string GetExtensionFromMIME(string mime)
        {
            return MimeTypes.GetMimeTypeExtensions(mime).FirstOrDefault();
        }

        public static string GetStaticFileDirectory(string folderName)
        {
            var normalPath = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            if (Directory.Exists(normalPath))
            {
                return normalPath;
            }

            var debugPath = Path.Combine(Directory.GetCurrentDirectory(), "bin", "Debug", "net5.0", folderName);
            if (Directory.Exists(debugPath))
            {
                return debugPath;
            }

            var releasePath = Path.Combine(Directory.GetCurrentDirectory(), "bin", "Release", "net5.0", folderName);
            if (Directory.Exists(releasePath))
            {
                return releasePath;
            }

            throw new DirectoryNotFoundException(folderName);
        }
    }
}