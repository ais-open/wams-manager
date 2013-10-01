using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Ais.Internal.Dcm.ModernUIV2.Common
{
    /// <summary>
    /// This class provides methods to read and write settings to User App Data
    /// </summary>
    class SettingHelper
    {
        string urlFilePath = string.Empty;
        string credentialFilePath = string.Empty;
        string apiUrl = string.Empty;
        string credential = string.Empty;
        string settingFilePath = "\\AIS\\WAMS.setting";
        string securityInfoFilePath = "\\AIS\\security.setting";

        /// <summary>
        /// Constructor
        /// </summary>
        public SettingHelper()
        {
            string userRoamingFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            urlFilePath = userRoamingFolder + settingFilePath;
            credentialFilePath = userRoamingFolder + securityInfoFilePath;
        }

        #region Public Methods for Setting Reading and Writing
        public void SetNewCredential(string userName, string password)
        {
            credential = GetEncodedCredential(userName, password);
            if (!Directory.Exists(Path.GetDirectoryName(credentialFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(credentialFilePath));
            }
            File.WriteAllText(credentialFilePath, credential);
        }

        public string GetCredential()
        {
            string credential = string.Empty;
            if (SettingFileIsPresent(credentialFilePath))
            {
                string[] lines = File.ReadAllLines(credentialFilePath);
                credential = lines.FirstOrDefault();
            }
            else
            {
                System.Configuration.AppSettingsReader reader = new System.Configuration.AppSettingsReader();
                var uname = (string)reader.GetValue("Username", typeof(string));
                var pwd = (string)reader.GetValue("Password", typeof(string));
                SetNewCredential(uname, pwd);
                credential = GetEncodedCredential(uname, pwd);
            }
            return credential;
        }

        public void SetNewUrlFromUser(string newUrl)
        {
            apiUrl = newUrl;
            if (!Directory.Exists(Path.GetDirectoryName(urlFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(urlFilePath));
            }
            File.WriteAllText(urlFilePath, newUrl);
        }

        public bool SettingFileIsPresent(string filePath)
        {
            return File.Exists(filePath);
        }

        public string GetUrlFromSettingFile()
        {
            string url = string.Empty;
            if (SettingFileIsPresent(urlFilePath))
            {
                string[] lines = File.ReadAllLines(urlFilePath);
                url = lines.FirstOrDefault();
            }
            else
            {
                //Create File and write URL
                System.Configuration.AppSettingsReader reader = new System.Configuration.AppSettingsReader();
                url = (string)reader.GetValue("ServiceUri", typeof(string));
                SetNewUrlFromUser(url);
            }
            return url;
        }

        public async Task<bool> IsApiAccessible(string apiUrl, string userName, string password)
        {

            bool isSuccess = false;
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(apiUrl);
                string credentials = GetEncodedCredential(userName, password);
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
                string uri = Literals.URL_GET_ENCODING_TYPE;
                var response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode();
                isSuccess = true;
            }
            catch (Exception)
            {
                isSuccess = false;
                throw;
            }
            return isSuccess;
        }
        #endregion

        #region private methods
        private string GetEncodedCredential(string userName, string password)
        {
            string credential = userName + ":" + password;
            byte[] bytes = UTF8Encoding.Default.GetBytes(credential);
            return Convert.ToBase64String(bytes);
        }
        #endregion
    }
}
