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
            IDictionary appsettings = Environment.GetEnvironmentVariables();
            List<string> storageConnectionStrings = new List<string> { };
            string azureWebJobsStorageString = String.Empty;
            string azureWebJobsDashboardStorageString = String.Empty;
            string consumptionCodeConfigStorageConnection = String.Empty;
            // Validate AzureWebJobsStorage and AzureWebJobsDashboard connectivity and if it's full purpose storage account

            // AzureWebJobsStorage can not be null. 
            //The Azure Functions runtime uses this storage account connection string for all functions except for HTTP triggered functions.
            if (Environment.GetEnvironmentVariable("AzureWebJobsStorage") == null)
            {
                throw new Exception("AzureWebJobsStorage is not set for your function app.");
            }
            else
            {
                azureWebJobsStorageString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                storageConnectionStrings.Add(azureWebJobsStorageString);
            }

            if (Environment.GetEnvironmentVariable("AzureWebJobsDashboard") != null)
            {
                azureWebJobsDashboardStorageString = Environment.GetEnvironmentVariable("AzureWebJobsDashboard");
                storageConnectionStrings.Add(azureWebJobsDashboardStorageString);
            }

            // For consumption plans only. But this is mandatory settings for consumption plan.
            // Connection string for storage account where the function app code and configuration are stored. 
            if (Environment.GetEnvironmentVariable("WEBSITE_CONTENTAZUREFILECONNECTIONSTRING") != null)
            {
                consumptionCodeConfigStorageConnection = Environment.GetEnvironmentVariable("WEBSITE_CONTENTAZUREFILECONNECTIONSTRING");
                storageConnectionStrings.Add(consumptionCodeConfigStorageConnection);
            }

            foreach (string connectionString in storageConnectionStrings)
            {
                MakeServiceRequestsExpectSuccess(connectionString);
            }

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

            return View(model: fileCount);
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
        internal void MakeServiceRequestsExpectSuccess(string connectionString)
        {
            CloudStorageAccount account = CloudStorageAccount.Parse(connectionString);
            // Make blob service requests
            CloudBlobClient blobClient = account.CreateCloudBlobClient();
            //   blobClient.ListContainersSegmentedAsync();
            blobClient.ListContainers().Count();
            blobClient.GetServiceProperties();

            // Make queue service requests
            CloudQueueClient queueClient = account.CreateCloudQueueClient();
            queueClient.ListQueues().Count();
            queueClient.GetServiceProperties();

            // Make table service requests
            CloudTableClient tableClient = account.CreateCloudTableClient();
            tableClient.ListTables().Count();
            tableClient.GetServiceProperties();

            // Not sure if this is only required for consumption
            //  When using a Consumption plan function definitions are stored in File Storage.
            CloudFileClient fileClient = account.CreateCloudFileClient();
            fileClient.ListShares().Count();
            fileClient.GetServiceProperties();
        }
    }
}