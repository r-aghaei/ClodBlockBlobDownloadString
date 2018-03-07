using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Core;
using System.Text;

namespace AzureExtensions
{
    public static class CloudBlockBlobExtensions
    {
        /// <summary>
        /// Downloads string content of the given blob. If encoding is not specified, it removes the BOM mark from beginning of the string if exists.
        /// </summary>
        /// <param name="blob">CloudBlockBlob to download contents.</param>
        /// <param name="encoding">Encoding which should be used to convert the content of blob to string. If encoding is not specified, it removes the BOM mark from beginning of the string if exists.</param>
        /// <returns>string content of the given blob.</returns>
        public static string DownloadString(this CloudBlockBlob blob, System.Text.Encoding encoding = null, AccessCondition accessCondition = null, BlobRequestOptions options = null, OperationContext operationContext = null)
        {
            using (SyncMemoryStream stream = new SyncMemoryStream())
            {
                blob.DownloadToStream(stream, accessCondition, options, operationContext);
                byte[] streamAsBytes = stream.GetBuffer();
                return GetStringUsingEncoding(streamAsBytes, encoding);
            }
        }
        private static string GetStringUsingEncoding(byte[] data, Encoding encoding = null)
        {
            int bomLengthInData = -1;

            // If no content encoding listed in the ContentType HTTP header, or no Content-Type header present, then
            // check for a byte-order-mark (BOM) in the data to figure out encoding.
            if (encoding == null)
            {
                byte[] preamble;
                // UTF32 must be tested before Unicode because it's BOM is the same but longer.
                Encoding[] encodings = { Encoding.UTF8, Encoding.UTF32, Encoding.Unicode, Encoding.BigEndianUnicode };
                for (int i = 0; i < encodings.Length; i++)
                {
                    preamble = encodings[i].GetPreamble();
                    if (ByteArrayHasPrefix(preamble, data))
                    {
                        encoding = encodings[i];
                        bomLengthInData = preamble.Length;
                        break;
                    }
                }
            }

            // Do we have an encoding guess?  If not, use default.
            if (encoding == null)
                encoding = Encoding.Default;

            // Calculate BOM length based on encoding guess.  Then check for it in the data.
            if (bomLengthInData == -1)
            {
                byte[] preamble = encoding.GetPreamble();
                if (ByteArrayHasPrefix(preamble, data))
                    bomLengthInData = preamble.Length;
                else
                    bomLengthInData = 0;
            }

            // Convert byte array to string stripping off any BOM before calling GetString().
            // This is required since GetString() doesn't handle stripping off BOM.
            return encoding.GetString(data, bomLengthInData, data.Length - bomLengthInData);
        }
        private static bool ByteArrayHasPrefix(byte[] prefix, byte[] byteArray)
        {
            if (prefix == null || byteArray == null || prefix.Length > byteArray.Length)
                return false;
            for (int i = 0; i < prefix.Length; i++)
            {
                if (prefix[i] != byteArray[i])
                    return false;
            }
            return true;
        }
    }
}
