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
using System.Web;
using System.Web.Mvc;

namespace ConnectionValidator.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            string siteFolder;
            int fileCount;

            // Validate APPINSIGHTS_INSTRUMENTATIONKEY app insights connection
            string appInsightsKey = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY");

            if (Environment.GetEnvironmentVariable("home") != null)
            {
                // Maps to the physical path of your site in Azure
                siteFolder =
                    Environment.ExpandEnvironmentVariables(@"%HOME%\site\wwwroot");
            }
            else
            {
                // Maps to the current sites root physical path.
                // Allows us to run locally.
                siteFolder = Server.MapPath("/");
            }

            fileCount =
                System.IO.Directory.GetFiles(
                    siteFolder,
                    "*.*",
                    SearchOption.AllDirectories).Length;

            Dictionary<string, string> storageResults = ValidateConnectionStrings();

            return View(model: storageResults);
        }

        public Dictionary<string, string> ValidateConnectionStrings()
        {
            IDictionary appsettings = Environment.GetEnvironmentVariables();
            Dictionary<string, string> storageResults = new Dictionary<string, string>();
            Dictionary<string, string> storageConnectionStrings = new Dictionary<string, string>();
            string azureWebJobsStorageString = String.Empty;
            string azureWebJobsDashboardStorageString = String.Empty;
            string consumptionCodeConfigStorageConnection = String.Empty;
            // Validate AzureWebJobsStorage and AzureWebJobsDashboard connectivity and if it's full purpose storage account

            // AzureWebJobsStorage can not be null. 
            //The Azure Functions runtime uses this storage account connection string for all functions except for HTTP triggered functions.
            List<string> settingKeys = new List<string> { "AzureWebJobsStorage", "AzureWebJobsDashboard", "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING" };
            string storageSymptoms = "Storage account is healthy";


            if (Environment.GetEnvironmentVariable("AzureWebJobsStorage") == null)
            {
                //throw new Exception("AzureWebJobsStorage is not set for your function app.");
                storageConnectionStrings.Add("AzureWebJobsStorage", "");

            }
            else
            {
                azureWebJobsStorageString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                 storageConnectionStrings.Add("AzureWebJobsStorage", azureWebJobsStorageString);
            }

            if (Environment.GetEnvironmentVariable("AzureWebJobsDashboard") != null)
            {
                azureWebJobsDashboardStorageString = Environment.GetEnvironmentVariable("AzureWebJobsDashboard");
                storageConnectionStrings.Add("AzureWebJobsDashboard", azureWebJobsDashboardStorageString);
            }
            else
            {
                storageConnectionStrings.Add("AzureWebJobsDashboard", "");
            }

            // For consumption plans only. But this is mandatory settings for consumption plan.
            // Connection string for storage account where the function app code and configuration are stored. 
            if (Environment.GetEnvironmentVariable("WEBSITE_CONTENTAZUREFILECONNECTIONSTRING") != null)
            {
                consumptionCodeConfigStorageConnection = Environment.GetEnvironmentVariable("WEBSITE_CONTENTAZUREFILECONNECTIONSTRING");
                storageConnectionStrings.Add("WEBSITE_CONTENTAZUREFILECONNECTIONSTRING", consumptionCodeConfigStorageConnection);
            }

            foreach (KeyValuePair<string, string> connectionString in storageConnectionStrings)
            {
                storageResults.Add(connectionString.Key, MakeServiceRequestsExpectSuccess(connectionString.Value));
            }
            return storageResults;
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
        internal string MakeServiceRequestsExpectSuccess(string connectionString)
        {
            string storageSymptoms = connectionString;
         
            CloudStorageAccount account = CloudStorageAccount.Parse(connectionString);
            // Make blob service requests
            try
            {
                CloudBlobClient blobClient = account.CreateCloudBlobClient();
                //   blobClient.ListContainersSegmentedAsync();
                blobClient.ListContainers().Count();
                blobClient.GetServiceProperties();
            }
            catch (Exception)
            {
                storageSymptoms += "   Blob is not enabled.";
            }

            try
            {
                // Make queue service requests
                CloudQueueClient queueClient = account.CreateCloudQueueClient();
                queueClient.ListQueues().Count();
                queueClient.GetServiceProperties();
            }
            catch (Exception)
            {
                storageSymptoms += "   Queue is not enabled.";
            }

            try
            {
                // Make table service requests
                CloudTableClient tableClient = account.CreateCloudTableClient();
                tableClient.ListTables().Count();
                tableClient.GetServiceProperties();
            }
            catch (Exception)
            {
                storageSymptoms += "  Table is not enabled.";
            }

            try
            {
                // Not sure if this is only required for consumption
                //  When using a Consumption plan function definitions are stored in File Storage.
                CloudFileClient fileClient = account.CreateCloudFileClient();
                fileClient.ListShares().Count();
                fileClient.GetServiceProperties();
            }
            catch (Exception)
            {
                storageSymptoms += "   File is not enabled.";
            }

            return storageSymptoms;
        }
    }
}