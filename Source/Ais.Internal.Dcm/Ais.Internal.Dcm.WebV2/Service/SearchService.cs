using System.Data;
using Ais.Internal.Dcm.Web.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Web;
using System.Text;

namespace Ais.Internal.Dcm.Web.Service
{
    public class SearchService : ISearchService
    {

        private ILoggerService _loggerService = null;
        private string _connectionString = null;

        public SearchService(ILoggerService logger)
        {
            this._loggerService = logger;
            _connectionString = GetConnectionString();
        }

        public SearchData SearchMedia(string searchParam, int rowsToSkip, int rowsToRetrieve, SearchType type)
        {
            SearchData data = null;
            try
            {
                IEnumerable<SearchResultViewModel> query = null;
                data = PrepareSearchData(searchParam, rowsToSkip, rowsToRetrieve, type);
                if (data != null && data.Data != null)
                {
                    switch (type)
                    {
                        case SearchType.FreeText:
                            query = from result in data.Data
                                    select result;
                            break;
                        case SearchType.TagSearch:
                            query = from result in data.Data
                                    where result.Tags.Contains(new Tag { Name = searchParam }, new TagComparer())
                                    select result;
                            break;
                        default:
                            break;
                    }
                    data.Data = query.ToList();
                }

            }
            catch (Exception exp)
            {
                this._loggerService.LogException("SearchService SearchMedia", exp);
            }
            return data;
        }

        public void InsertMediaHistory(SearchResultViewModel model)
        {
            try
            {
                StringBuilder insertCommand = new StringBuilder("");

                string str = GetFileAssetInsertStatement(model);
                insertCommand.Append(str);
                for (int i = 0; i < model.Outputs.Count; i++)
                {
                    insertCommand.Append(GetOutputInsertStatement(model.AssetFileId, model.Outputs[i]));
                }
                using (var connection = new SQLiteConnection(this._connectionString))
                {
                    SQLiteCommand command = new SQLiteCommand(insertCommand.ToString(), connection);
                    command.CommandType = CommandType.Text;
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (Exception exception)
            {
                _loggerService.LogException("Insert mediahistory" + exception, exception);
            }
        }

        private string GetFileAssetInsertStatement(SearchResultViewModel model)
        {
            return "INSERT INTO MediaHistory (ParentAssetId,FileName,DefaultThumbnailUrl,CollectionName,AlbumName,NameForSearch,TagsForSearch,MediaServiceName,AssetFileId) SELECT '" + Clean(model.ParentAssetId) + "','" + Clean(model.FileName) + "','" + Clean(model.DefaultThumbnailUrl) + "','" + Clean(model.CollectionName) + "','" + Clean(model.AlbumName) + "','" + Clean(model.NameForSearch) + "','" + Clean(model.TagsForSearch) + "','" + Clean(model.MediaServiceName) + "','" + Clean(model.AssetFileId) + "' " +
                 "WHERE NOT EXISTS (SELECT 1 FROM MEDIAHISTORY WHERE ASSETFILEID='" + model.AssetFileId + "') ;";
        }

        private string GetOutputInsertStatement(string assetFileId, VideoOutput output)
        {
            string encodingName = Clean(output.EncodingName);
            string url = Clean(output.Url);
            return "INSERT INTO MEDIAHISTORYOutput(ASSETFILEID,ENCODINGNAME,OUTPUTURL)"
                    + "SELECT '" + assetFileId + "', '" + encodingName + "','" + url + "' "
                    + "WHERE NOT EXISTS (SELECT 1 FROM MEDIAHISTORYOutput WHERE ASSETFILEID='" + assetFileId + "' AND OutputUrl='" + url + "'); ";
        }

        private string Clean(string value)
        {
            return value == null ? "" : value.Replace("'","''");
        }

        private static string GetFirstInsertValues(SearchResultViewModel model, VideoOutput searchModel)
        {
            var fullTextForSearch = (model.MediaServiceName == null ? "" : model.MediaServiceName) + (model.CollectionName == null ? "" : model.CollectionName) +
                                  (model.AlbumName == null ? "" : model.AlbumName)  + (model.FileName == null ? "" : model.FileName) + "," + (searchModel.EncodingName == null ? "" : searchModel.EncodingName) 
                                   +(model.TagsForSearch == null ? "" : model.TagsForSearch) 
                                  ;
            fullTextForSearch = fullTextForSearch.ToLower();
            string commandText = "SELECT '" + model.ParentAssetId + "' AS 'ParentAssetId','" +
                                    model.FileName + "' AS 'FileName','" +
                                    model.DefaultThumbnailUrl + "' AS 'DefaultThumbnailUrl','" +
                                    model.CollectionName + "' AS 'CollectionName','" +
                                    model.AlbumName + "' AS 'AlbumName','" +
                                    fullTextForSearch + "' AS 'NameForSearch','" +
                                    model.TagsForSearch + "' AS 'TagsForSearch','" +
                                    model.MediaServiceName + "' AS 'MediaServiceName','" +
                                    searchModel.EncodingName + "' AS 'EncodingName','" +
                                    searchModel.Url + "' AS 'OutputUrl','" +
                                    model.AssetFileId + "' AS 'AssetFileId' ";
            return commandText;
        }

        private static string GetInsertUnionValues(SearchResultViewModel model, VideoOutput searchModel)
        {
            var fullTextForSearch = (model.MediaServiceName == null ? "" : model.MediaServiceName) + (model.CollectionName == null ? "" : model.CollectionName) +
                                  (model.AlbumName == null ? "" : model.AlbumName) + (model.FileName == null ? "" : model.FileName) + "," + (searchModel.EncodingName == null ? "" : searchModel.EncodingName)
                                   + (model.TagsForSearch == null ? "" : model.TagsForSearch)
                                  ;
            fullTextForSearch = fullTextForSearch.ToLower();
            //(ParentAssetId,FileName,DefaultThumbnailUrl,CollectionName,AlbumName,NameForSearch,TagsForSearch,MediaServiceName,EncodingName,OutputUrl,AssetFileId) ");
            string commandText = " UNION SELECT '" + model.ParentAssetId + "','" +
                                    model.FileName +  "','"+
                                    model.DefaultThumbnailUrl + "','" +
                                    model.CollectionName + "','" +
                                    model.AlbumName + "','" +
                                    fullTextForSearch + "','" +
                                    model.TagsForSearch + "','" +
                                    model.MediaServiceName + "','" +
                                    searchModel.EncodingName + "','" +
                                    searchModel.Url + "','" + 
                                    model.AssetFileId +  "' ";
            return commandText;
        }

        private string GetConnectionString()
        {
            return "Data Source=" +
                                HttpContext.Current.Server.MapPath("~/App_Data\\WAMSSearchDB.sqlite;") +
                                "Version=3;New=False;Compress=True;";
        }

        private SearchData PrepareSearchData(string searchParam, int rowsToSkip, int rowsToRetrieve, SearchType type)
        {
            SearchData searchData = null;
            string tagsForSearch = string.Empty;
            string columnToSearch = "NameForSearch";
            switch (type)
            {
                case SearchType.TagSearch:
                    columnToSearch = "TagsForSearch";
                    break;
                default:
                    break;
            }
            try
            {
                searchData = ReadData(searchParam,rowsToSkip,rowsToRetrieve, columnToSearch);
                // now that we have read all the records, we will combine the search results
                // right now each output has different record for itself
                // we use dictionary for this
                Dictionary<string, SearchResultViewModel> dictionary = new Dictionary<string, SearchResultViewModel>();
                foreach (var item in searchData.Data)
                {
                    if (dictionary.ContainsKey(item.AssetFileId))
                    {
                        var searchItem = dictionary[item.AssetFileId];
                        searchItem.Outputs.Add(item.Outputs.FirstOrDefault());
                    }
                    else
                    {
                        dictionary.Add(item.AssetFileId, item);
                    }
                }
                searchData.Data = dictionary.Values.ToList();
            }
            catch (Exception exception)
            {
                this._loggerService.LogException("Preparing MediaHistory: " + exception.ToString(), exception);
            }
            return searchData;
        }

        private SearchData ReadData(string searchParam, int rowsToSkip, int rowsToRetrieve, string columnToSearch)
        {
            SearchData searchData = null;
            var data = new List<SearchResultViewModel>();
            long count = 0;
            string connectionString = GetConnectionString();
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                SQLiteCommand sql_cmd = connection.CreateCommand();
                ReadAssetFiles(searchParam, rowsToSkip, rowsToRetrieve, columnToSearch, data, sql_cmd);
                //get the total count
                sql_cmd.CommandText = string.Format("select count(1) from (select distinct assetfileid from  MediaHistory WHERE {0} LIKE '%{1}%' group by assetfileid) ", columnToSearch, searchParam);
                count = (long)sql_cmd.ExecuteScalar();

                ReadOutputs(data, sql_cmd);
                connection.Close();
            }
            searchData = new SearchData { Data = data, TotalCount = count };
            return searchData;
        }

        private static void ReadAssetFiles(string searchParam, int rowsToSkip, int rowsToRetrieve, string columnToSearch, List<SearchResultViewModel> data, SQLiteCommand sql_cmd)
        {
            sql_cmd.CommandText = string.Format("select * from MediaHistory WHERE {0} LIKE '%{1}%'  order by FileName  LIMIT {2}, {3}", columnToSearch, searchParam, rowsToSkip, rowsToRetrieve);
            using (SQLiteDataReader reader = sql_cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    SearchResultViewModel item = GetSearchModel(reader);
                    data.Add(item);
                }
            }
        }

        private  void ReadOutputs(List<SearchResultViewModel> data, SQLiteCommand sqlCommand)
        {
            if (data != null && data.Count > 0)
            {
                string listOfAssetIds = GetAssetIdList(data);
                sqlCommand.CommandText = string.Format("SELECT ASSETFILEID,ENCODINGNAME,OUTPUTURL FROM MEDIAHISTORYOUTPUT WHERE ASSETFILEID IN ({0})", listOfAssetIds);
                Dictionary<string, SearchResultViewModel> dictionary = new Dictionary<string, SearchResultViewModel>();
                foreach (SearchResultViewModel item in data)
                {
                    if (!dictionary.ContainsKey(item.AssetFileId))
                    {
                        dictionary.Add(item.AssetFileId, item);
                    }
                }
                using (SQLiteDataReader reader = sqlCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string fileAssetId = reader[0].ToString();
                        string encodingName = reader[1].ToString();
                        string outputurl = reader[2].ToString();
                        if (dictionary.ContainsKey(fileAssetId))
                        {
                            var item = dictionary[fileAssetId];
                            if (item.Outputs == null)
                            {
                                item.Outputs = new List<VideoOutput>();
                            }
                            item.Outputs.Add(new VideoOutput { EncodingName = encodingName, Url = outputurl });
                        }
                    }

                }
            }
        }

        private string GetAssetIdList(List<SearchResultViewModel> data)
        {
            StringBuilder builder = new StringBuilder();
            var ids = from s in data
                      select s.AssetFileId;
            foreach (string s in ids)
            {
                builder.Append("'");
                builder.Append(s);
                builder.Append("'");
                builder.Append(",");
            }
            builder.Length--; // removing last comma
            return builder.ToString();
        }

        private static SearchResultViewModel GetSearchModel(SQLiteDataReader reader)
        {
            SearchResultViewModel data = null;
            // read all columns
            string tagsForSearch = string.Empty;
            var id = reader[0].ToString();
            var parentAssetId = reader[1].ToString();
            var fileName = reader[2].ToString();
            var defaultThumbnailUrl = reader[3].ToString();
            var collectionName = reader[4].ToString();
            var albumName = reader[5].ToString();
            var nameForSearch = reader[6].ToString();
            tagsForSearch = reader[7].ToString();
            var mediaServiceName = reader[8].ToString();
            var assetFileId = reader[9].ToString();
            // data to object
            data = new SearchResultViewModel()
            {
                Id = id,
                ParentAssetId = parentAssetId,
                AlbumName = albumName,
                FileName = fileName,
                CollectionName = collectionName,
                DefaultThumbnailUrl = defaultThumbnailUrl,
                NameForSearch = nameForSearch,
                TagsForSearch = tagsForSearch,
                MediaServiceName = mediaServiceName,
                AssetFileId = assetFileId
            };
            data.Outputs = new List<VideoOutput>();
            // it's kind of expensive to do it for all records but for now we'll keep it like this
            data.Tags = tagsForSearch.Split(',')
                                                .ToList()
                                                .Select(t => new Tag { Id = Guid.Empty.ToString(), Name = t }).ToList();
            return data;
        }
    }

    public class SearchData
    {
        public List<SearchResultViewModel> Data { get; set; }
        public long TotalCount { get; set; }
    }
    internal class TagComparer : IEqualityComparer<Tag>
    {
        public bool Equals(Tag x, Tag y)
        {
            return x.Name.ToUpper() == y.Name.ToUpper();
        }

        public int GetHashCode(Tag obj)
        {
            return obj.Name.ToUpper().GetHashCode();
        }
    }

}