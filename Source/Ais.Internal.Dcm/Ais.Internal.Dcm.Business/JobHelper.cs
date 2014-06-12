using System.Collections.Generic;
using System.Text;

namespace Ais.Internal.Dcm.Business
{
    public static class MediaServiceConstants
    {
        public static string MediaProcessorId = "nb:mpid:UUID:70bdc2c3-ebf4-42a9-8542-5afc1e55d217";
    }

    public class TaskStep
    {
        public string InputAssetName { get; set; }
        public string OutputAssetName { get; set; }
        public short Order { get; set; }
        public string Configuration { get; set; }
        public string MediaProcessorID { get; set; }
        public override string ToString()
        {
            string inputAssetNameStr = string.IsNullOrEmpty(InputAssetName) ? "" : "assetName=\\\"" + InputAssetName + "\\\"";
            return string.Format("<?xml version=\\\"1.0\\\" encoding=\\\"utf-8\\\"?>" +
    "<taskBody>" +
    "<inputAsset " + inputAssetNameStr + ">JobInputAsset(0)</inputAsset>" +
        "<outputAsset assetName=\\\"{0}\\\">JobOutputAsset({1})</outputAsset>" +
        "</taskBody>", OutputAssetName, Order);
        }
    }

    public class MediaServiceJobInfo
    {
        public string Name { get; set; }
        public List<string> InputAssetIds { get; set; }
        public List<TaskStep> Tasks { get; set; }

        /// <summary>
        /// Constructor that takes the name of the job
        /// </summary>
        /// <param name="name">name of the media service job</param>
        public MediaServiceJobInfo(string name)
        {
            this.Name = name;
            this.InputAssetIds = new List<string>();
            this.Tasks = new List<TaskStep>();
        }

        public string CreateJsonString()
        {
            return "{\"Name\":\"" + Name + "\",\"InputMediaAssets\":[" + GetInputAssets() + "],\"Tasks\":[" + GetTasks() + "]}";
        }

        private string GetInputAssets()
        {
            StringBuilder strBuilder = new StringBuilder();
            foreach (string asset in InputAssetIds)
            {
                strBuilder.Append("{\"__metadata\":{\"uri\":\"https://media.windows.net/api/Assets('" + asset + "')\"}}");
                strBuilder.Append(",");
            }
            strBuilder.Remove(strBuilder.Length - 1, 1);
            return strBuilder.ToString();
        }

        private string GetTasks()
        {
            StringBuilder strBuilder = new StringBuilder();
            foreach (var task in Tasks)
            {
                strBuilder.Append("{");
                strBuilder.Append("\"Configuration\":\"" + task.Configuration + "\",");
                strBuilder.Append("\"MediaProcessorId\":\"" + task.MediaProcessorID + "\",");
                strBuilder.Append("\"TaskBody\":\"" + task.ToString() + "\"");
                strBuilder.Append("}");
                strBuilder.Append(",");
            }
            strBuilder.Remove(strBuilder.Length - 1, 1);
            return strBuilder.ToString();
        }
    }
}
