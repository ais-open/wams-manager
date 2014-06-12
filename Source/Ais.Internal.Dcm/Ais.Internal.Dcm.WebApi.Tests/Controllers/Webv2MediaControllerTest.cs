using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ais.Internal.Dcm.Web.Controllers;
using Ais.Internal.Dcm.Web.Models;
using Ais.Internal.Dcm.Web.Service;
using AzurePatterns.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Ais.Internal.Dcm.WebApi.Tests.Controllers
{
    [TestClass]
    public class Webv2MediaControllerTest
    {
        private MockLoggerService loggerService;
        private IStorageAccountInformation account;
        private MetaDataService metaDataService;
        private MediaController controller;
        private UserMediaService userMediaService;
        private ISearchService searchService;

        [TestInitialize]
        public void TestInit()
        {
            loggerService = new MockLoggerService();
            account = new TestStorageAccountService();
            metaDataService = new MetaDataService(account);
            searchService = new SearchService(loggerService);
            userMediaService = new UserMediaService(metaDataService, loggerService, searchService);
            controller = new MediaController(loggerService, userMediaService);
        }

        [TestMethod]
        public void MediaControllerConstructorTestForNull()
        {
            Assert.IsNotNull(userMediaService, "Constructor failed for MediaController");
        }

        [TestMethod]
        public void LoggerServiceTestForNull()
        {
            Assert.IsNotNull(loggerService, "Constructor failed for initializing loggerService");
        }

        [TestMethod]
        public void UserMediaServiceTestForNull()
        {
            Assert.IsNotNull(userMediaService, "Constructor failed for initializing User Media Service");
        }

        [TestMethod]
        public void GetAllMediaServicesTest()
        {
            var mediaServices = controller.GetAllMediaServices("");
            Assert.IsTrue((mediaServices != null), "Get MediaServices Failed");

            controller = null;
            MyAssert.Throws<Exception>(() => controller.GetAllMediaServices(string.Empty));
        }

        [TestMethod]
        public void GetAllAssetsTest()
        {
            var mediaServices = controller.GetAllMediaServices("");
            var mediaServiceModel = mediaServices.FirstOrDefault();
            if (mediaServiceModel != null)
            {
                var assets = controller.GetAllAssets(mediaServiceModel.AccountName);
                Assert.IsTrue((assets != null), "Get Assets Failed");
            }

            controller = null;
            MyAssert.Throws<Exception>(() => controller.GetAllAssets(string.Empty));
        }

        [TestMethod]
        public void CreateNewAssetTest()
        {
            var mediaServices = controller.GetAllMediaServices("");
            var mediaServiceModel = mediaServices.FirstOrDefault();
            if (mediaServiceModel != null)
            {
                var asset = controller.CreateNewAsset(mediaServiceModel.AccountName, "UnitTestAsset1");
                Assert.IsNotNull(asset, "Create New Asset Failed");
            }

            MyAssert.Throws<Exception>(() => controller.CreateNewAsset(string.Empty, string.Empty));
        }

        [TestMethod]
        public void GetUploadSasUrlTest()
        {
            var mediaServices = controller.GetAllMediaServices("");
            var mediaServiceModels = mediaServices as IList<MediaServiceModel> ?? mediaServices.ToList();
            var mediaServiceModel = mediaServiceModels.FirstOrDefault();
            if (mediaServiceModel != null)
            {
                var assets = controller.GetAllAssets(mediaServiceModel.AccountName);
                var firstOrDefault = assets.FirstOrDefault();
                if (firstOrDefault != null)
                {
                    string uploadUrl = controller.GetUploadSasUrl(mediaServiceModel.AccountName, firstOrDefault.Id);
                Assert.IsTrue(!string.IsNullOrWhiteSpace(uploadUrl), "GetUploadSasUrl Failed");
            }
        }

            MyAssert.Throws<Exception>(() => controller.GetUploadSasUrl(string.Empty, string.Empty));
        }

        [TestMethod]
        public void InitializeSeach()
        {
            var v = controller.GetGroupedOutput("mediaservicetag","nb:cid:UUID:17143daa-f6d3-40c1-808b-56ca26090c42");
        }

        [TestMethod]
        public void GetThumbnailUrlsTest()
        {
            var mediaServices = controller.GetAllMediaServices("");
            var mediaService = mediaServices.ToList().FirstOrDefault();
            if (mediaService != null)
            {
                var assets = controller.GetAllAssets(mediaService.AccountName);
                var firstOrDefault = assets.FirstOrDefault();
                if (firstOrDefault != null)
                {
                    var thumbnails = controller.GetThumbnailUrls(firstOrDefault.Id);
                    Assert.IsNotNull(thumbnails, "Get Thumbnails for asset failed");
            }
        }
            controller = null;
            MyAssert.Throws<Exception>(() => controller.GetThumbnailUrls(string.Empty));
        }

        [TestMethod]
        public void GetOutputAssetsTest()
        {
            var mediaServices = controller.GetAllMediaServices("");
            var mediaService = mediaServices.ToList().FirstOrDefault();
            if (mediaService != null)
            {
                var assets = controller.GetAllAssets(mediaService.AccountName);
                var outputAssets = controller.GetOutputAssetFiles(mediaService.AccountName, assets.FirstOrDefault().Id);
                Assert.IsNotNull(outputAssets, "Get Output asset files for asset failed");
            }
            controller = null;
            MyAssert.Throws<Exception>(() => controller.GetOutputAssetFiles(string.Empty, string.Empty));
        }

        [TestMethod]
        public void GetAssetFilesTest()
        {
            var mediaServices = controller.GetAllMediaServices("");
            var mediaService = mediaServices.ToList().FirstOrDefault();
            if (mediaService != null)
            {
                var assets = controller.GetAllAssets(mediaService.AccountName);
                var firstOrDefault = assets.FirstOrDefault();
                if (firstOrDefault != null)
                {
                    var assetFiles = controller.GetAssetFiles(mediaService.AccountName, firstOrDefault.Id);
                Assert.IsNotNull(assetFiles, "Get Asset files for asset failed");
            }
        }
            controller = null;
            MyAssert.Throws<Exception>(() => controller.GetAssetFiles(string.Empty, string.Empty));
        }

        [TestMethod]
        public void GetAssetFilesContextTest()
        {
            var mediaServices = controller.GetAllMediaServices("");
            var mediaService = mediaServices.ToList().FirstOrDefault();
            if (mediaService != null)
            {
                var assets = controller.GetAllAssets(mediaService.AccountName);
                var firstOrDefault = assets.FirstOrDefault();
                if (firstOrDefault != null)
                {
                    var assetFileContexts = controller.GetAssetFilesContext(mediaService.AccountName, firstOrDefault.Id);
                Assert.IsNotNull(assetFileContexts, "Get Asset fileinfo's for asset failed");
            }
        }
            MyAssert.Throws<Exception>(() => controller.GetAssetFilesContext(string.Empty, string.Empty));
        }

        [TestMethod]
        public void GetEncodingTypesTest()
        {
            var mediaServices = controller.GetAllMediaServices(""); 
            var mediaService = mediaServices.ToList().FirstOrDefault();
            if (mediaService != null)
            {
                var encodingTypes = controller.GetEncodingTypes(mediaService.AccountName);
                Assert.IsNotNull(encodingTypes, "Get EncodingTypes failed");
            }
            controller = null;
            MyAssert.Throws<Exception>(() => controller.GetEncodingTypes(string.Empty));
        }

        [TestMethod]
        public void UploadFileInitiateJobTest()
        {
            //string path = @"D:\Sample Videos\Flowers_001_Preview.mp4";
            string path = @"~\..\..\..\Flowers_001_Preview.mp4";
            var mediaServices = controller.GetAllMediaServices(""); 
            var mediaServiceModels = mediaServices as IList<MediaServiceModel> ?? mediaServices.ToList();
            var mediaServiceModel = mediaServiceModels.FirstOrDefault();
            if (mediaServiceModel != null)
            {
                var assets = controller.GetAllAssets(mediaServiceModel.AccountName);
                var firstOrDefault = assets.FirstOrDefault();
                if (firstOrDefault != null)
                {
                    var assetId = firstOrDefault.Id;
                string uploadSasUrl = controller.GetUploadSasUrl(mediaServiceModel.AccountName, assetId);
                var fileName = System.IO.Path.GetFileName(path);
                string sasUrl = BuildSasUrl(uploadSasUrl, fileName);
                bool uploadSuccess = UploadFile(path, uploadSasUrl);
                    Assert.IsTrue(uploadSuccess, "File upload failed");
                //Link uploaded file to asset
                bool linkSuccess = controller.GenerateFileMetaData(mediaServiceModel.AccountName, assetId);
                Assert.IsTrue(linkSuccess, "Linking file to asset failed");
                //Make uploaded file as primary
                var assetFiles = controller.GetAssetFilesContext(mediaServiceModel.AccountName, assetId);
                var files = assetFiles.Where(file => file.Name == Path.GetFileName(fileName));
                var maxDate = files.Max(f => f.Created);
                var assetFile = assetFiles.FirstOrDefault(f => f.Created == maxDate);
                if (assetFile != null)
                {
                    bool isPrimary = controller.MakePrimaryFile(mediaServiceModel.AccountName, assetId, assetFile.Id);
                    Assert.IsTrue(isPrimary, "Failed to make the uploaded file as primary");
                    string encodingStrings = "H264 Broadband 1080p,H264 Broadband 720p";
                    string encodingFriendlyNames = "Broadband 1080, Broadband 720";
                    string jobId = controller.InitiateJob(mediaServiceModel.AccountName, assetId, encodingStrings, encodingFriendlyNames);
                    Assert.IsTrue(!(string.IsNullOrWhiteSpace(jobId)), "Job with selected encodings failed");
                
                    var thumbnailEncodingStrings = "Medium" + "," + "640" + "," + "480" + ","
                                + "5" + "," + "20";

                    string thumbnailJob = controller.InitiateThumbnailJob(mediaServiceModel.AccountName, assetId,
                                                                          thumbnailEncodingStrings);
                    Assert.IsTrue(!(string.IsNullOrWhiteSpace(thumbnailJob)),
                                  "Thumbnail Job with selected encodings failed");

                    bool isAJobPending = controller.IsAJobPending(mediaServiceModel.AccountName, assetId);
                    Assert.IsTrue(isAJobPending, "Call to check for job pending failed");

                        Task.Delay(2000);

                        bool status = true;
                        int counter = 0;
                        do
                        {
                            status = controller.IsAJobPending(mediaServiceModel.AccountName, assetId);
                            System.Threading.Thread.Sleep(60000);
                            controller.Refresh(mediaServiceModel.AccountName);
                            counter = counter + 1;
                            if (counter > 20) status = false;
                        } while (status);
                        Assert.IsTrue(status == false, "Check for job status failed");
                    }
                }
            }
        }

        [TestMethod]
        public void GetJobStatusTest()
        {
            var mediaServices = controller.GetAllMediaServices("");
            var mediaService = mediaServices.ToList().FirstOrDefault();
            if (mediaService != null)
            {
                var assets = controller.GetAllAssets(mediaService.AccountName);
                var firstOrDefault = assets.FirstOrDefault();
                if (firstOrDefault != null)
                {
                    var jobs = controller.GetJobStatus(firstOrDefault.Id);
                    Assert.IsNotNull(jobs, "Currently no Jobs are queued");
                }
            }
        }

        [TestMethod]
        public void DeleteAssetTest()
        {
            var mediaServices = controller.GetAllMediaServices("");
            var mediaService = mediaServices.ToList().FirstOrDefault();
            if (mediaService != null)
            {
                var assets = controller.GetAllAssets(mediaService.AccountName);
                var minDate = assets.Min(f => f.CreatedDate);
                var asset = assets.FirstOrDefault(f => f.CreatedDate == minDate);
                if (asset != null)
                    userMediaService.DeleteAssetEntity(new AssetEntity
                        {
                            PartitionKey = mediaService.AccountName,
                            RowKey = asset.Id,
                            Id = asset.Id,
                            ETag = "*"
                        });
                assets = userMediaService.FindAssetEntity(string.Format("PartitionKey eq '{0}' and RowKey eq '{1}'", mediaService.AccountName, asset.Id));
                if (assets != null)
                {
                    Assert.IsTrue(assets.Count() == 0,"Delete asset failed");
                }
            }
        }

        [TestMethod]
        public void FindAssetTest()
        {
            var mediaServices = controller.GetAllMediaServices("");
            var mediaService = mediaServices.ToList().FirstOrDefault();
            if (mediaService != null)
            {
                var assets = controller.GetAllAssets(mediaService.AccountName);
                var asset = assets.FirstOrDefault();
                if (asset != null)
                    assets = userMediaService.FindAssetEntity(string.Format("PartitionKey eq '{0}' and RowKey eq '{1}'", mediaService.AccountName, asset.Id));
                if (assets != null)
                {
                    Assert.IsTrue(assets.Count() > 0, "Find asset failed");
                }
            }
        }

        [TestMethod]
        public void FindAssetFileTest()
        {
            var mediaServices = controller.GetAllMediaServices("");
            var mediaService = mediaServices.ToList().FirstOrDefault();
            if (mediaService != null)
            {
                var assets = controller.GetAllAssets(mediaService.AccountName);
                var asset = assets.FirstOrDefault();
                if (asset != null)
                {
                    var assetFiles = controller.GetAssetFiles(mediaService.AccountName, asset.Id);
                    var filterAssetFiles = userMediaService.FindAssetFileEntity(string.Format("PartitionKey eq '{0}' and RowKey eq '{1}'", asset.Id, assetFiles.FirstOrDefault().Id));
                    if (filterAssetFiles != null)
                    {
                        Assert.IsTrue(filterAssetFiles.Count() > 0, "Find asset file failed");
                    }
                }
            }
        }

        [TestMethod]
        public void FindAssetOutputTest()
        {
            var mediaServices = controller.GetAllMediaServices("");
            var mediaService = mediaServices.ToList().FirstOrDefault();
            if (mediaService != null)
            {
                var assets = controller.GetAllAssets(mediaService.AccountName);
                var asset = assets.FirstOrDefault();
                if (asset != null)
                {
                    var filterAssetOutputs = userMediaService.FindAssetFileEntity(string.Format("PartitionKey eq '{0}'", asset.Id));
                    if (filterAssetOutputs != null)
                    {
                        Assert.IsTrue(filterAssetOutputs.Count() > 0, "Find asset outputs failed");
                    }
                }
            }
        }

        [TestMethod]
        public void CreateOrUpdateAssetWithTagTest()
        {
            var mediaServices = controller.GetAllMediaServices("");
            var mediaService = mediaServices.ToList().FirstOrDefault();
            if (mediaService != null)
            {
                controller.CreateOrUpdateAssetWithTag("mediaservicetag","","Rhyme,Jeeva,Xbox");
                //Assert.IsNotNull(encodingTypes, "Get EncodingTypes failed");
            }
            controller = null;
            MyAssert.Throws<Exception>(() => controller.GetEncodingTypes(string.Empty));
        }

        internal string BuildSasUrl(string locatorPath, string fileName)
        {
            try
            {
                string queryPart = new Uri(locatorPath).Query;
                string blobContainerUri = locatorPath.Substring(0, locatorPath.Length - queryPart.Length);
                string fileUrl = string.Format("{0}/{1}{2}", blobContainerUri, fileName, queryPart);
                return fileUrl;
            }
            catch (Exception exception)
            {
                loggerService.LogException(exception.Message, exception);
            }
            return string.Empty;
        }

        bool UploadFile(string localFilePath, string sasURL)
        {
            try
            {
                //if (CheckifAlreadyInQueue(localFilePath))
                //{
                //    return;
                //}

                string fileName = Path.GetFileName(localFilePath);
                //SetUploadStatus(fileName, 1);
                int defaultBlockSize = 524288;
                int blockSizeParseResult = 0;
                var rdr = new System.Configuration.AppSettingsReader();
                try
                {
                    if (Int32.TryParse((string)rdr.GetValue("DefaultBlockSize", typeof(string)), out blockSizeParseResult))
                    {
                        defaultBlockSize = blockSizeParseResult;
                    }
                }
                catch (Exception exception)
                {
                    //Logger.WriteLog(exception);
                    return false; 
                }
                FileInfo fInfo = new FileInfo(localFilePath);
                var sasUri = new Uri(sasURL);
                string queryString = sasUri.Query;
                string baseUrl = sasURL.Substring(0, sasURL.Length - queryString.Length);
                sasURL = string.Format("{0}/{1}{2}", baseUrl, fInfo.Name, queryString);
                CloudBlockBlob blob = new CloudBlockBlob(new Uri(sasURL));
                if (fInfo.Length <= defaultBlockSize)//File size is smaller than default block size. Don't split in chunks.
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
                        numberOfBlocks = (int)(fInfo.Length / defaultBlockSize);
                        if (fInfo.Length % defaultBlockSize != 0)
                        {
                            numberOfBlocks += 1;
                        }
                        if (numberOfBlocks < 50000)//There can be a maximum of 50000 blocks so if the file is really large we just need to increase the default block size.
                        {
                            break;
                        }
                        defaultBlockSize *= 2;
                    }
                    while (true);
                    List<byte[]> blockContents = new List<byte[]>();
                    List<string> blockIds = new List<string>();
                    int index = 0;
                    int offset = 0;
                    long bytesRemaining = fInfo.Length;
                    int blockSize = defaultBlockSize;
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
                    int blocksToProcess = numberOfBlocks;
                    index = 0;
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
                        //SetUploadStatus(fileName, percentage);

                        //FireStatusChangeEvent();
                        index += maxConcurrentThreads;
                    }
                    //Commit the block list
                    blob.PutBlockList(blockIds);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public static class MyAssert
    {
        public static void Throws<T>(Action func) where T : Exception
        {
            var exceptionThrown = false;
            try
            {
                func.Invoke();
            }
            catch (T)
            {
                exceptionThrown = true;
            }

            if (!exceptionThrown)
            {
                throw new AssertFailedException(
                    String.Format("An exception of type {0} was expected, but not thrown", typeof(T))
                    );
            }
        }
    }
}
