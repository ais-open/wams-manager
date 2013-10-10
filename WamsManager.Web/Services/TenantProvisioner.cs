using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using CommandCenter.UI;
using System.Text;

namespace WamsManager.Web.Models
{
    public class TenantProvisioner
    {
        public delegate void StatusUpdateHandler(object sender, ProgressEventArgs e);

        public event StatusUpdateHandler OnUpdateStatus;
        public event StatusUpdateHandler OnProvisionComplete;

        private string MsgTxt;
        private StringBuilder messageDisplay = new StringBuilder();
        private string adminUsername;
        private string adminPassword;
        private string storageAcctInfo;
        private string primaryKey;
        AzureTableService azureTableService = new AzureTableService();

        public StringBuilder ProvisionSite(CustomerInfo customerInfo)
        {
            this.OnUpdateStatus += provisioner_OnUpdateStatus;
            this.OnProvisionComplete += provisioner_OnProvisionComplete;

            this.adminUsername = customerInfo.Username;
            this.adminPassword = customerInfo.Password;
            this.storageAcctInfo = customerInfo.AzureStorageAccount;
            this.primaryKey = customerInfo.AzureStoragePrimaryKey;

            return ProvisionTenant(customerInfo);

        }

        private StringBuilder ProvisionTenant(CustomerInfo customerInfo)
        {
            try
            {
                azureTableService.UpdateEvaluationId(customerInfo.InvitationId);

                string tenantName = customerInfo.SitePrefix;
                string siteName = "mtws-" + tenantName;
                const string siteAdmin = "mtws-admin";
                const string basePath = @"c:\WAMSTool_Temp";
                const string certPath = @"cert:\LocalMachine\My\A0F6CA0654F021348371E17B1995A1B7D9C314A0";
                const string azureModulePath = @"C:\Program Files (x86)\Microsoft SDKs\Windows Azure\PowerShell\Azure\Azure.psd1";
                const string gitPath = @"C:\Program Files (x86)\Git\cmd";
                // const string siteUsername = "kirti_sain";

                var cmd = "set-executionpolicy -executionpolicy unrestricted -force" + Environment.NewLine;
                ExecutePSCommand(cmd);

                cmd = "Import-Module '" + azureModulePath + "'";
                ExecutePSCommand(cmd);

                UpdateStatus("Preparing client environment");
                cmd = @"$certificate = Get-Item '" + certPath + "'" + Environment.NewLine +
                    "Set-AzureSubscription -SubscriptionName Azdem194I33542W -SubscriptionId 01efbbcd-874b-49b6-9b69-dd126bd9afd9 -Certificate $certificate" + Environment.NewLine +
                    "Select-AzureSubscription -SubscriptionName Azdem194I33542W";
                ExecutePSCommand(cmd);

                cmd = "$env:Path += " + "';" + gitPath + "'";
                ExecutePSCommand(cmd);

                UpdateStatus("Creating website..");
                cmd = "New-AzureWebsite -Name " + siteName + " -Location 'East US' -Git -PublishingUserName " + siteAdmin;
                ExecutePSCommand(cmd);
                
                UpdateStatus("Added tenant to settings..");
                cmd = "$settings = New-Object Hashtable" + Environment.NewLine
                      + "$settings['tenant']='" + tenantName + "'" + Environment.NewLine
                      + "Set-AzureWebsite -AppSettings $settings " + siteName;
                ExecutePSCommand(cmd);

                var tempDir = Path.Combine(basePath, siteName);
                Directory.CreateDirectory(tempDir);

                cmd = "Set-Location -Path '" + tempDir + "' -PassThru";
                ExecutePSCommand(cmd);

                cmd = "Git init";
                ExecutePSCommand(cmd);

                UpdateStatus("Copying the template site..");
                cmd = "Git pull https://KirtiSain:password123@ais-dcm.visualstudio.com/DefaultCollection/_git/DigitalContentManagementDeploy";
                ExecutePSCommand(cmd);

                string webConfigPath = Path.Combine(tempDir, @"Source\Ais.Internal.Dcm\Ais.Internal.Dcm.Webv2\web.config");
                cmd = "$webConfig = '" + webConfigPath + "'" + Environment.NewLine;
                cmd += "$doc = (gc $webConfig) -as [xml]" + Environment.NewLine;
                cmd += "$doc.SelectSingleNode('//appSettings/add[@key=\"DefaultAdminUsername\"]/@value').'#text' = '" + adminUsername + "'" + Environment.NewLine;
                cmd += "$doc.SelectSingleNode('//appSettings/add[@key=\"DefaultAdminPassword\"]/@value').'#text' = '" + adminPassword + "'" + Environment.NewLine;
                cmd += "$doc.SelectSingleNode('//appSettings/add[@key=\"MetadataStorageAccountName\"]/@value').'#text' = '" + storageAcctInfo + "'" + Environment.NewLine;
                cmd += "$doc.SelectSingleNode('//appSettings/add[@key=\"MetadataStorageKey\"]/@value').'#text' = '" + primaryKey + "'" + Environment.NewLine;
                cmd += "$doc.Save($webConfig)";
                ExecutePSCommand(cmd);

                string email = "kirti.sain@appliedis.com";
                string name = "mtws-admin";

                cmd = "git config user.email " + email + Environment.NewLine;
                cmd += "git config user.name " + name;
                ExecutePSCommand(cmd);

                cmd = "git add -A";
                ExecutePSCommand(cmd);
                cmd = "git commit -m 'updated web.config'";
                ExecutePSCommand(cmd);

               // cmd = string.Format("Git remote add azure https://{0}:password123@{1}.scm.azurewebsites.net/{1}.git", siteUsername, siteName);
                cmd = string.Format("Git remote add azure https://{0}:password123@{1}.scm.azurewebsites.net/{1}.git", siteAdmin, siteName);
                ExecutePSCommand(cmd);

                UpdateStatus("Pushing to tenant site");

                cmd = "Git push azure master";
                ExecutePSCommand(cmd);

                string siteUrl = "http://" + siteName + ".azurewebsites.net";

                if (messageDisplay != null)
                {
                    messageDisplay.AppendLine("Tenant provisioning completed!");
                    messageDisplay.AppendLine(Environment.NewLine);
                    messageDisplay.AppendLine("Your provisioned site url is " + siteUrl);
                }

                azureTableService.UpdateCustomerInfoTableWithSiteInfo(customerInfo.InvitationId, customerInfo.InvitationId, siteUrl);

                try
                {
                    this.SetReadOnlyAttribute(new DirectoryInfo(tempDir), false);
                    Directory.Delete(tempDir, true);
                }
                catch (Exception ex)
                {
                    messageDisplay.AppendLine("Unable to delete folder " + tempDir);
                    messageDisplay.AppendLine(Environment.NewLine);
                    messageDisplay.AppendLine("Error : " + ex);
                }

               return messageDisplay;
            }
            catch (Exception ex)
            {
                messageDisplay.AppendLine("Tenant provisioning failed!");
                messageDisplay.AppendLine(Environment.NewLine);
                messageDisplay.AppendLine("Error : " + ex);
            }

            return messageDisplay;
        }

        private void ExecutePSCommand(string command)
        {
            var output = command.ExecutePS();
            UpdateStatus(output);
        }

        private void UpdateStatus(string status)
        {
            if (OnUpdateStatus == null) return;

            var args = new ProgressEventArgs(status);
            OnUpdateStatus(this, args);
        }

        void provisioner_OnProvisionComplete(object sender, ProgressEventArgs eventArgs)
        {

            MsgTxt = MsgTxt + eventArgs.Status + Environment.NewLine;
        }

        private void provisioner_OnUpdateStatus(object sender, ProgressEventArgs eventArgs)
        {
            MsgTxt = MsgTxt + eventArgs.Status + Environment.NewLine;
        }

        private void SetReadOnlyAttribute(DirectoryInfo directory, bool value) 
        {
            foreach (FileInfo file in directory.GetFiles())
            {
                file.IsReadOnly = value; 
            }

            foreach (DirectoryInfo subDirectory in directory.GetDirectories())
            {
                SetReadOnlyAttribute(subDirectory, value);
            }
        }

    }

    public class ProgressEventArgs : EventArgs
    {
        public string Status { get; private set; }

        public ProgressEventArgs(string status)
        {
            Status = status;
        }
    }
}

