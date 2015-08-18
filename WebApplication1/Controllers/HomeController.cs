using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            var account = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            var client = account.CreateCloudQueueClient();
            var q = client.GetQueueReference("queue");

            await q.CreateIfNotExistsAsync();

            await q.AddMessageAsync(new CloudQueueMessage("Hello World!"));

            return View();
        }
    }
}