using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ais.Internal.Dcm.Web.Models
{
    public class EncodingTypeModel
    {
        [Required]
        public string TechnicalName { get; set; }
        [Required]
        public string FriendlyName { get; set; }
    }

    public class MediaServiceModel
    {
        public int Id { get; set; }

        public string MediaServiceFriendlyName { get; set; }

        public string AccountName { get; set; }

        public string SecondaryAccountKey { get; set; }

        public string PrimaryAccountKey { get; set; }

        public string ClientKey { get; set; }
    }

    public class AssetInfoModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string DefaultThumbnailUrl { get; set; }
        public string MediaServiceName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Tags { get; set; }
        public string MediaServiceFriendlyName { get; set; }
    }

    public class ThumbnailAssetModel
    {
        public string AssetId { get; set; }
        public string AssetFileId { get; set; }
        public string ThumbnailFileId { get; set; }
        public string Name { get; set; }
        public string URL { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class AssetWithFilesModel
    {
        public string OutputAssetId { get; set; }
        public string Name { get; set; }
        public string URL { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ParentAssetId { get; set; }
        public string FriendlyName { get; set; }
        public string Tags { get; set; }
    }

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

    public class GroupedOutputViewModel
    {
        public string FileName { get; set; }
        public string DefaultThumbnailUrl { get; set; }
        public List<VideoOutput> Outputs { get; set; }
        public List<Tag> Tags { get; set; }
        public List<string> Thumbnails { get; set; }

    }

    public class VideoOutput
    {
        public string EncodingName { get; set; }
        public string Url { get; set; }
    }

    public class Tag
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}