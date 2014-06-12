using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Ais.Internal.Dcm.ModernUIV2.Common
{
    /// <summary>
    /// This class will help the UI to upload multiple files and check their upload status
    /// </summary>
    public class UploadHelper
    {
        

        private const string ASSET_FILENAME_SEPARATOR = "8346306D-10F4-4856-A10C-7A1407642798"; // unique separator t

        private const int DEFAULT_BLOCK_SIZE = 524288;

        #region Private methods
        private void OnStatusChanged(UploadFileEventArgs args)
        {
            if (OnUploadStatusChange != null)
            {
                OnUploadStatusChange(this, args);
            }
        }

        private static object locker = new object();

        private static Dictionary<string, UploadFileRequest> fileQueue = new Dictionary<string, UploadFileRequest>();

        private int UploadBlocks(string assetId, string fileName, CloudBlockBlob blob, int numberOfBlocks, List<byte[]> blockContents, List<string> blockIds)
        {
            int blocksToProcess = numberOfBlocks;
            int index = 0;
            while (blocksToProcess > 0)
            {
                int maxConcurrentThreads = Math.Min(Environment.ProcessorCount, blocksToProcess);
                List<Task> tasks = new List<Task>();
                for (int i = 0; i < maxConcurrentThreads; i++)
                {
                    var blockId = blockIds[i + index];
                    var blockContent = blockContents[i + index];
                    tasks.Add(Task.Factory.StartNew(() =>
                            {
                                using (var ms = new MemoryStream(blockContent))
                                {
                                    var contentMD5 = string.Empty;
                                    blob.PutBlock(blockId, ms, contentMD5);
                                }
                            }));
                }
                Task.WaitAll(tasks.ToArray());
                blocksToProcess -= maxConcurrentThreads;
                //
                int percentage = ((numberOfBlocks - blocksToProcess) * 100 / numberOfBlocks);
                SetUploadStatus(assetId, fileName, percentage);

                FireStatusChangeEvent();
                index += maxConcurrentThreads;
            }
            return index;
        }

        private static int DivideFileIntoBlocks(string localFilePath, int startblockSize, long totalFileSize, List<byte[]> blockContents, List<string> blockIds)
        {
            int index = 0;
            int offset = 0;
            long bytesRemaining = totalFileSize;
            int blockSize = startblockSize;
            string blockIdPrefix = Guid.NewGuid().ToString();
            using (FileStream fs = new FileStream(localFilePath, FileMode.Open))
            {
                while (bytesRemaining > 0)
                {
                    byte[] blockContent = new byte[blockSize];
                    int bytesRead = fs.Read(blockContent, offset, blockSize);
                    bytesRemaining -= bytesRead;
                    blockContents.Add(blockContent);
                    blockIds.Add(Convert.ToBase64String(Encoding.UTF8.GetBytes(blockIdPrefix + "-" + index.ToString("d6"))));
                    index++;
                    if (bytesRemaining < blockSize)
                    {
                        blockSize = (int)bytesRemaining;
                    }
                }
            }
            return index;
        }

        private static string BuildUploadUrl(string fileName,  string sasURL)
        {
            string uploadUrl = "";
            var sasUri = new Uri(sasURL);
            string queryString = sasUri.Query;
            string baseUrl = sasURL.Substring(0, sasURL.Length - queryString.Length);
            uploadUrl = string.Format("{0}/{1}{2}", baseUrl, fileName, queryString);
            return uploadUrl;
        }

        private static int GetBlockSizeFromConfiguration()
        {
            int defaultBlockSize = DEFAULT_BLOCK_SIZE;
            int blockSizeParseResult = DEFAULT_BLOCK_SIZE;
            var rdr = new System.Configuration.AppSettingsReader();
            try
            {
                if (Int32.TryParse((string)rdr.GetValue("DefaultBlockSize", typeof(string)), out blockSizeParseResult))
                {
                    defaultBlockSize = blockSizeParseResult;
                }
            }
            catch (Exception)
            {
                //Logger.WriteLog(exception);
            }
            return defaultBlockSize;
        }


        private void SetUploadStatus(string assetId, string fileName, int percentage)
        {
            lock (locker)
            {
                var key = GetKey(assetId, fileName);
                if (fileQueue.ContainsKey(key)) // if the item exists update the percentage
                {
                    var item = fileQueue[key];
                    item.UploadPercentage = percentage;
                }
                else // if it's new queue for upload
                {
                    var item = new UploadFileRequest { FileName = fileName };
                    item.UploadPercentage = percentage;
                    fileQueue.Add(key, item);
                }
            }
        }

        private string GetKey(string assetId, string localFilePath)
        {
            return (assetId + ASSET_FILENAME_SEPARATOR+ Path.GetFileName(localFilePath));
        }


        private bool CheckifAlreadyInQueue(string assetId, string localFilePath)
        {
            bool exists = false;
            string key = GetKey(assetId, localFilePath);
            lock (locker)
            {
                if (fileQueue.ContainsKey(key))
                {
                    return exists = true;
                }
            }
            return exists;
        }

        private void FireStatusChangeEvent()
        {
            lock (locker)
            {
                UploadFileEventArgs args = new UploadFileEventArgs();
                args.FileStatus = new List<UploadFileStatusInfo>();

                foreach (var item in fileQueue)
                {
                    string key = item.Key;
                    int value = fileQueue[item.Key].UploadPercentage;
                    var itemStatus = new UploadFileStatusInfo
                    {
                        AssetId = GetAssetIDFromKey(key),
                        FileName = GetFileNameFromKey(key),
                        UploadPercentage = value
                    };
                    args.FileStatus.Add(itemStatus);
                }
                OnStatusChanged(args);
            }
        }

        private string GetFileNameFromKey(string key)
        {
            return key.Split(new string[] { ASSET_FILENAME_SEPARATOR },StringSplitOptions.RemoveEmptyEntries)[1];
        }

        private string GetAssetIDFromKey(string key)
        {
            return key.Split(new string[] { ASSET_FILENAME_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries)[0];
        }
        #endregion

        /// <summary>
        /// Uploads local file to an asset
        /// </summary>
        /// <param name="assetId">id of an asset where the media file is uploaded</param>
        /// <param name="localFilePath">local path of the media file</param>
        /// <param name="sasURL">Secure access url for upload</param>
        public void UploadFile(string assetId, string localFilePath, string sasURL)
        {

            if (CheckifAlreadyInQueue(assetId, localFilePath))
            {
                return;
            }

            string fileName = Path.GetFileName(localFilePath);
            SetUploadStatus(assetId, fileName, 1);

            int startblockSize = GetBlockSizeFromConfiguration();

            FileInfo fInfo = new FileInfo(localFilePath);
            string uploadURL = BuildUploadUrl(fInfo.Name, sasURL);
            CloudBlockBlob blob = new CloudBlockBlob(new Uri(uploadURL));
            if (fInfo.Length <= startblockSize)//File size is smaller than default block size. Don't split in chunks.
            {
                using (FileStream fs = new FileStream(localFilePath, FileMode.Open))
                {
                    blob.UploadFromStream(fs);
                }
            }
            else
            {
                //We need to split in chunks
                int numberOfBlocks = 0;
                do
                {
                    numberOfBlocks = (int)(fInfo.Length / startblockSize);
                    if (fInfo.Length % startblockSize != 0)
                    {
                        numberOfBlocks += 1;
                    }
                    if (numberOfBlocks < 50000)//There can be a maximum of 50000 blocks so if the file is really large we just need to increase the default block size.
                    {
                        break;
                    }
                    startblockSize *= 2;
                }
                while (true);

                List<byte[]> blockContents = new List<byte[]>();
                List<string> blockIds = new List<string>();
                int index = DivideFileIntoBlocks(localFilePath, startblockSize, fInfo.Length, blockContents, blockIds);

                index = UploadBlocks(assetId, fileName, blob, numberOfBlocks, blockContents, blockIds);
                //Commit the block list
                blob.PutBlockList(blockIds);
            }
        }

        /// <summary>
        /// Removes all item from upload queue
        /// </summary>
        public static void RemoveAllFromQueue()
        {
            lock (locker)
            {
                fileQueue.Clear();
            }
        }

        /// <summary>
        /// Event that fires when file upload state changes
        /// </summary>
        public event EventHandler<UploadFileEventArgs> OnUploadStatusChange;
    }

    

   
}
