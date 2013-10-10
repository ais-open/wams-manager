using Ais.Internal.Dcm.Business;
using Ais.Internal.Dcm.Web.Models;
using AzurePatterns.Repository;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
namespace Ais.Internal.Dcm.Web.Service
{
    public interface IStorageAccountInformation
    {
        string AccountName { get; }
        string Key { get; }
    }
}
