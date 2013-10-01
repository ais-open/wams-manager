using Ais.Internal.Dcm.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ais.Internal.Dcm.Web
{
    //interface IMetaDataService
    //{
    //    MediaServiceContext GetMediaServiceContext(string mediaServiceName);
    //}

    //public class MockMetaDataService : IMetaDataService
    //{
    //    private Dictionary<string, MediaServiceContext> mediaServiceContexts;

    //    public MockMetaDataService(List<MediaServiceInfo> mediaServiceInfos = null)
    //    {

    //        if (mediaServiceInfos != null)
    //        {
    //            mediaServiceContexts = new Dictionary<string, MediaServiceContext>();
    //            foreach (var mediaServiceInfo in mediaServiceInfos)
    //            {
    //                var context = new MediaServiceContext(mediaServiceInfo.AccountName,
    //                                                      mediaServiceInfo.PrimaryAccountKey);
    //                mediaServiceContexts.Add(
    //                    mediaServiceInfo.AccountName, context);
    //                System.Web.HttpContext.Current.Application[mediaServiceInfo.AccountName] = context;
    //            }
    //        }
    //        else
    //        {
    //            //mediaServiceContexts = new Dictionary<string, MediaServiceContext>
    //            //    {
    //            //        {
    //            //            "aismediaservice3", new MediaServiceContext("aismediaservice3",
    //            //                                                        "LrOXhQRf/0R8X1TRHMMkLVtEMJsJtXV7MbRNi90ZiFY=")
    //            //        },
    //            //        {
    //            //            "aisdemomodernui", new MediaServiceContext("aisdemomodernui",
    //            //                                                       "A72Gb+BUnGKQDlO2ptqaZpHMyi2oOEKrbVKgLOBFmc0=")
    //            //        },
    //            //        {
    //            //            "aisdemomediaservice", new MediaServiceContext("aisdemomediaservice",
    //            //                                                           "byfwSL991NeVHc5De1gs7HLbXiTZk+caY49dgPPGXSA=")
    //            //        },
    //            //        {
    //            //            "uxmediaservice", new MediaServiceContext("uxmediaservice",
    //            //                                                      "eTQLzz9n4vSDkGKDplMMH4pHjfSj/R+oI6bDEyWSH1I=")
    //            //        },
    //            //        {
    //            //            "aismetadataservice",
    //            //            new MediaServiceContext("aismetadataservice", "nUosuhYaJag7OJVD3ojHAkYNRySuiRqoQ0h7IORo3HU=")
    //            //        }
    //            //    };
    //        }

    //    }

    //    public MediaServiceContext GetMediaServiceContext(string mediaServiceName)
    //    {
    //        var mediaServiceContext = mediaServiceContexts[mediaServiceName];
    //        return mediaServiceContext;
    //    }
    //}
}
