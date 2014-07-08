wams-manager
------------
------------
This readme describes the steps for setting up WAMS Manager, and how to use this application:

1. Create an Azure website ([click here](http://azure.microsoft.com/en-us/documentation/articles/web-sites-dotnet-get-started/))
2. Create an Azure Storage ([click here] (http://azure.microsoft.com/en-us/documentation/articles/storage-create-storage-account/))
3. Open the __Source/Ais.Internal.Dcm/Ais.Internal.Dcm.sln__ solution, and goto __web.config__ file of the __Ais.Internal.Dcm.WebV2__ project. Make the following changes to this web.config file:

  * _MetadataStorageAccountName_  - Azure storage account name, you created in Step-2 above.
  * _MetadataStorageKey_          -Azure storage account Access Key, you created in Step-2 above.
  * _DefaultAdminUsername_        - WAMS Manager Administrator username.
  * _DefaultAdminPassword_        - WAMS Manager Administrator password.
  * _DataConnectionString_        - Replace the _AccountName_ and _AccountKey_ with the Storage Account Name and Storage Key (obtained in Step 2 above) in the connection string.
  * **Note**: section of the web.config file that needs to be updated as above instructions are copied below for reference:  
`````
<!-- Storage Specific Settings-->     
<add key="MetadataStorageAccountName" value="storage_account_name_here" />     
<add key="MetadataStorageKey" value="storage_account_access_key_here" />     
<add key="DefaultAdminUsername" value="wams_manager_admininstrator_username_here"/>     
<add key="DefaultAdminPassword" value="wams_manager_admininstrator_password_here"/>    
<add key="DataConnectionString" value="DefaultEndpointsProtocol=https;AccountName=storage_account_name_here;AccountKey=storage_account_access_key_here" /> 
````

4. Proceed to the Project __Ais.Internal.Dcm.ModernUIV2__, and open __Common/Config.cs__
5. Replace the _BitlyUsername_ and _BitlyKey_ with your bit.ly credentials.
6. Build the solution and publish the website using the Publishing Profile of the azure website created in Step-1 above.
7. Visit the website created in Step-1, and login with the _DefaultAdminUsername_ and _DefaultAdminPassword_ you specified for the _web.config_ file in Step-3.
8. Follow instructions in **Docs/WAMSAdminPortal.pdf** to complete the setup of the application.
9. Finally go through **Docs/WAMSManager.pdf** (a manual on how to use the application).

 
