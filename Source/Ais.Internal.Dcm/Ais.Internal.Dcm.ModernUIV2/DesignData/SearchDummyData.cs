using Ais.Internal.Dcm.ModernUIV2.ViewModels;
using System.Collections.Generic;

namespace Ais.Internal.Dcm.ModernUIV2.DesignData
{
    class SearchDummyData
    {
        public static PagingCollectionViewModel Data { get {

            var assets = new List<SearchResultViewModel>();//await FetchAssets();
            assets.Add(new SearchResultViewModel
            {
                FileName = "Simple Video.mp4",
                Tags = GetDummyTags(),
                Outputs = GetDummyOutputs(),
                DefaultThumbnailUrl = "http://media.ch9.ms/ch9/fdc5/bdb5377e-dee2-472a-8bf3-f00c6331fdc5/aspnet26534_960.jpg",
                CollectionName = "Nature Video",
                AlbumName = "Zoo Animals"
            });
           
            return new PagingCollectionViewModel(assets,1,1);
        
        }  }

        private static List<VideoOutput> GetDummyOutputs()
        {
            var outputs = new List<VideoOutput>();
            outputs.Add(new VideoOutput
            {
                EncodingName = "MP4",
                Url = "http://content2.catalog.video.msn.com/e2/ds/86c378b1-78ce-4408-bb36-0b751f6b496b.mp4"
            });
            outputs.Add(new VideoOutput
            {
                EncodingName = "iOS",
                Url = "http://content2.catalog.video.msn.com/e2/ds/86c378b1-78ce-4408-bb36-0b751f6b496b.mp4"
            });
            return outputs;
        }

        private static List<ViewModels.Tag> GetDummyTags()
        {
            var tags = new List<Tag>();
            tags.Add(new Tag { Name = "MSDN" });
            tags.Add(new Tag { Name = "Microsoft" });
            tags.Add(new Tag { Name = "Training Videos1" });
            tags.Add(new Tag { Name = "Training Videos2" });
            tags.Add(new Tag { Name = "Training Videos3" });
            tags.Add(new Tag { Name = "Training Videos4" });
            tags.Add(new Tag { Name = "Training Videos5" });
            return tags;
        }
    }

    class SharingData
    {
        public static List<VideoOutput> EncodingTypes
        {
            get
            {
                var output = new List<VideoOutput>();
                output.Add(new VideoOutput { EncodingName = "h264 Windows Phone", Url = "http://www.google.com" });
                output.Add(new VideoOutput { EncodingName = "h264 Windows Phone1", Url = "http://www.google1.com" });
                return output;
            }
        }

        public static string SelectedVideoURL { get { return "http://google.com"; } }
        public static string DefaultThumbnailURL { get { return "http://google.com"; } }
    }

    public class ImageUrls
    {
        public static ThumbnailRollViewModel Urls
        {
            get
            {
                var v = new List<string>();
                v.Add("https://encrypted-tbn2.gstatic.com/images?q=tbn:ANd9GcQVM-3Qy4_E31MAR6kQepI4hNoTau5j4iFRpg90ptL--Y8pcs-IGF_PNQ");
                v.Add("https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRKASlrRBbd2Xvbpm7jsyyfH5lWY8etqy9OEj4Y9VsCUDelfkgZ2hXKUQ");
                v.Add("https://encrypted-tbn3.gstatic.com/images?q=tbn:ANd9GcSV4B6MQKb_eIMpjUvlsgoA6HG66NMNlylrc2-z-nU2L1XZKq2WadxYVKY");
                v.Add("https://encrypted-tbn1.gstatic.com/images?q=tbn:ANd9GcSL99XAL3kDbu0SUnqAyDl9HHPBJsxuBAz23Ce2_hk3wjQrKM-jSwyb8w");
                v.Add("https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSABjp6X9UeYMfwo8TqSz-yiEKfsITuymw-e6xM10CjxhRZ_OI6ed5xow");
                v.Add("https://encrypted-tbn1.gstatic.com/images?q=tbn:ANd9GcTQPUUFZy5oVhyBilys3n9Z5TXEKYTpqEG4Rz1F_dDB2ZR2eljEPdI-k9A");

                var data = new ThumbnailRollViewModel(v, 4);
                return data;

            }
        }
    }
}
