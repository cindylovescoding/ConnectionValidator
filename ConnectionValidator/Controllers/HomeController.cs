using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.File;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ConnectionValidator.Controllers
{
    public class StorageAccountsValidation
    {
        public string AppSettingsKey { get; set; }
        public Boolean Mandatory { get; set; }
        public string AccountName { get; set; }
        public string Message { get; set; }
        public int Status { get; set; }
    }

    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            //string siteFolder;
            //int fileCount;

            // Validate APPINSIGHTS_INSTRUMENTATIONKEY app insights connection
          //string appInsightsKey = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY");

            //if (Environment.GetEnvironmentVariable("home") != null)
            //{
            //    // Maps to the physical path of your site in Azure
            //    siteFolder =
            //        Environment.ExpandEnvironmentVariables(@"%HOME%\site\wwwroot");
            //}
            //else
            //{
            //    // Maps to the current sites root physical path.
            //    // Allows us to run locally.
            //    siteFolder = Server.MapPath("/");
            //}

            //fileCount =
            //    System.IO.Directory.GetFiles(
            //        siteFolder,
            //        "*.*",
            //        SearchOption.AllDirectories).Length;

             ValidateConnectionStrings();

            return View();
        }

        public void ValidateConnectionStrings()
        {
            StorageAccountsValidation example = new StorageAccountsValidation
            {
                AppSettingsKey = "haha",
                AccountName = "cindytest",
                Message = "enen",
                Status = 0
            };

            List<StorageAccountsValidation> storageValidations = new List<StorageAccountsValidation> { };
            //storageValidations.Add(example);

            try
            {
                Dictionary<string, string> storageConnectionStrings = new Dictionary<string, string>();
                string testBlobOnlyAccount = "";
                //string testBlobOnlyAccount = "";
                IDictionary appsettings = Environment.GetEnvironmentVariables();
                foreach (DictionaryEntry setting in appsettings)
                {
                    var settingString = setting.Value.ToString();
                    if (settingString.StartsWith("APPSETTING_") && settingString.Contains("; AccountKey="))
                    {
                        storageConnectionStrings.Add(setting.Key.ToString(), setting.Value.ToString());
                    }
                }

                string azureWebJobsStorageString = String.Empty;
                string azureWebJobsDashboardStorageString = String.Empty;
                string consumptionCodeConfigStorageConnection = String.Empty;
                // Validate AzureWebJobsStorage and AzureWebJobsDashboard connectivity and if it's full purpose storage account

                // AzureWebJobsStorage can not be null. 
                //The Azure Functions runtime uses this storage account connection string for all functions except for HTTP triggered functions.
                //List<string> settingKeys = new List<string> { "AzureWebJobsStorage", "AzureWebJobsDashboard", "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING" };


                //if (Environment.GetEnvironmentVariable("AzureWebJobsStorage") == null)
                //{
                //    //throw new Exception("AzureWebJobsStorage is not set for your function app.");
                //    storageConnectionStrings.Add("AzureWebJobsStorage", testBlobOnlyAccount);

                //}
                //else
                //{
                //    azureWebJobsStorageString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                //    storageConnectionStrings.Add("AzureWebJobsStorage", azureWebJobsStorageString);
                //}

                //if (Environment.GetEnvironmentVariable("AzureWebJobsDashboard") != null)
                //{
                //    azureWebJobsDashboardStorageString = Environment.GetEnvironmentVariable("AzureWebJobsDashboard");
                //    storageConnectionStrings.Add("AzureWebJobsDashboard", azureWebJobsDashboardStorageString);
                //}
                //else
                //{
                //    storageConnectionStrings.Add("AzureWebJobsDashboard", testBlobOnlyAccount);
                //}

                // For consumption plans only. But this is mandatory settings for consumption plan.
                // Connection string for storage account where the function app code and configuration are stored. 
                //if (Environment.GetEnvironmentVariable("WEBSITE_CONTENTAZUREFILECONNECTIONSTRING") != null)
                //{
                //    consumptionCodeConfigStorageConnection = Environment.GetEnvironmentVariable("WEBSITE_CONTENTAZUREFILECONNECTIONSTRING");
                //    storageConnectionStrings.Add("WEBSITE_CONTENTAZUREFILECONNECTIONSTRING", consumptionCodeConfigStorageConnection);
                //}

            //    ViewBag.storageConnectionStrings = storageConnectionStrings;

                foreach (KeyValuePair<string, string> connectionString in storageConnectionStrings)
                {

                    StorageAccountsValidation validationResult = MakeServiceRequestsExpectSuccess(connectionString.Key, connectionString.Value);
                    storageValidations.Add(validationResult);
                }

            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
            }

            ViewBag.storageValidations = storageValidations;


        }

        // The Azure Functions runtime uses this storage account connection string for all functions 
        // except for HTTP triggered functions. 
        // The storage account must be a general-purpose one that supports blobs, queues, and tables. 
        // See Storage account and Storage account requirements.

        // AzureWebJobsDashboard
        // Optional storage account connection string for storing logs and displaying them in the Monitor tab in the portal.
        // The storage account must be a general-purpose one that supports blobs, queues, and tables. 
        // See Storage account and Storage account requirements.

        //Functions uses Storage for operations such as managing triggers and logging function executions.
        internal StorageAccountsValidation MakeServiceRequestsExpectSuccess(string connectionKey, string connectionString)
        {
            string storageSymptoms = String.Empty;
            int status = 0;
            string postfixStatement = "Please make sure this is a general-purpose storage account.";
            //if (connectionKey.Equals("AzureWebJobsStorage", StringComparison.OrdinalIgnoreCase))
            //{

            //}

            try
            {
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    //  return Task.FromResult<StorageAccountsValidation> (new StorageAccountsValidation { });
                    return new StorageAccountsValidation { };
                }


                CloudStorageAccount account = CloudStorageAccount.Parse(connectionString);
               
                // Make blob service requests
                try
                {
                    CloudBlobClient blobClient = account.CreateCloudBlobClient();
                    //   blobClient.ListContainersSegmentedAsync();
                   // blobClient.ListContainers().Count();
                    blobClient.GetServiceProperties();
                }
                catch (Exception ex)
                {
                    storageSymptoms += "Blob endpoint is not reachable. Make sure the firewall on this storage account is not misconfigured.";
                    throw ex;
                }

                try
                {
                    // Make queue service requests
                    CloudQueueClient queueClient = account.CreateCloudQueueClient();
                    queueClient.ListQueues().Count();
                    queueClient.GetServiceProperties();               
                }
                catch (Exception ex)
                {
                    storageSymptoms += "Queue is not enabled. ";
                    throw ex;
                }

                try
                {
                    // Make table service requests
                    CloudTableClient tableClient = account.CreateCloudTableClient();
                    tableClient.ListTables().Count();
                    tableClient.GetServiceProperties();
                }
                catch (Exception ex)
                {
                    storageSymptoms += "Table is not enabled.";
                    throw ex;
                }

                try
                {
                    // Not sure if this is only required for consumption
                    //  When using a Consumption plan function definitions are stored in File Storage.
                    CloudFileClient fileClient = account.CreateCloudFileClient();
                    fileClient.ListShares().Count();
                    fileClient.GetServiceProperties();
                }
                catch (Exception ex)
                {
                    storageSymptoms += "File is not enabled.";
                    throw ex;
                }

                storageSymptoms = "Storage connection string validation passed!";
            }
            catch (Exception ex)
            {
                storageSymptoms += "\n";
                storageSymptoms += postfixStatement;
                status = 1;
            }


            StorageAccountsValidation result =new StorageAccountsValidation
            {
                AppSettingsKey = connectionKey,
                Mandatory = false,
                AccountName = connectionString.Split(new string[] { "AccountName=", ";AccountKey=" }, StringSplitOptions.RemoveEmptyEntries)[1],
                Message = storageSymptoms,
                Status = status
            };

            //return Task.FromResult(result);
            return result;
        }
    }
}