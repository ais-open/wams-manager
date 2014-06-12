using System.Collections.Generic;

namespace Ais.Internal.Dcm.ModernUIV2.ViewModels
{
    public class SearchResultViewModel
    {
        public string Id { get; set; }
        public string ParentAssetId { get; set; }
        public string FileName { get; set; }
        public string DefaultThumbnailUrl { get; set; }
        public string CollectionName { get; set; }
        public string AlbumName { get; set; }
        public List<VideoOutput> Outputs { get; set; }
        public List<Tag> Tags { get; set; }
        public string NameForSearch { get; set; }
        public string TagsForSearch { get; set; }
        public string MediaServiceName { get; set; }
        public string AssetFileId { get; set; }
    }

    public class VideoOutput
    {
        public string EncodingName { get; set; }
        public string Url { get; set; }
    }

    public class Tag
    {
        public string Id { get; set; }
        public string  Name { get; set; }
    }

    public class SearchData
    {
        public List<SearchResultViewModel> Data { get; set; }
        public long TotalCount { get; set; }
    }
}
