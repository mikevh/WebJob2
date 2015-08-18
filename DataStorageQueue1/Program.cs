using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Threading.Tasks;

namespace DataStorageQueue1Sample
{
    public class Program
    {
        public static void Main(string[] args) {
            var q = CreateQueueAsync().Result;

            q.AddMessageAsync(new CloudQueueMessage("Hello World!")).Wait();

        }

        public static void Main1(string[] args) {
            CloudQueue queue = CreateQueueAsync().Result;

            BasicQueueOperationsAsync(queue).Wait();

            UpdateEnqueuedMessageAsync(queue).Wait();

            ProcessBatchOfMessagesAsync(queue).Wait();

            //DeleteQueueAsync(queue).Wait();

            Console.WriteLine("Press any key to exit");
            Console.Read();
        }

        private static async Task<CloudQueue> CreateQueueAsync() {
            CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            
            CloudQueue queue = queueClient.GetQueueReference("queue");
            try {
                await queue.CreateIfNotExistsAsync();
            }
            catch (StorageException ex) {
                Console.WriteLine("If you are running with the default configuration please make sure you have started the storage emulator. Press the Windows key and type Azure Storage to select and run it from the list of applications - then restart the sample.");
                Console.ReadLine();
                throw;
            }

            return queue;
        }

        private static async Task BasicQueueOperationsAsync(CloudQueue queue) {
            await queue.AddMessageAsync(new CloudQueueMessage("Hello World!"));

            CloudQueueMessage peekedMessage = await queue.PeekMessageAsync();
            if (peekedMessage != null) {
                Console.WriteLine("The peeked message is: {0}", peekedMessage.AsString);
            }

            // You de-queue a message in two steps. Call GetMessage at which point the message becomes invisible to any other code reading messages 
            // from this queue for a default period of 30 seconds. To finish removing the message from the queue, you call DeleteMessage. 
            // This two-step process ensures that if your code fails to process a message due to hardware or software failure, another instance 
            // of your code can get the same message and try again. 
            CloudQueueMessage message = await queue.GetMessageAsync();
            if (message != null)
            {
                Console.WriteLine("Processing & deleting message with content: {0}", message.AsString);
                await queue.DeleteMessageAsync(message);
            }
        }

        private static async Task UpdateEnqueuedMessageAsync(CloudQueue queue) {
            await queue.AddMessageAsync(new CloudQueueMessage("Hello World Again!"));

            CloudQueueMessage message = await queue.GetMessageAsync();
            message.SetMessageContent("Updated contents.");
            await queue.UpdateMessageAsync(
                message,
                TimeSpan.Zero,  // For the purpose of the sample make the update visible immediately
                MessageUpdateFields.Content |
                MessageUpdateFields.Visibility);
        }

        private static async Task ProcessBatchOfMessagesAsync(CloudQueue queue) {
            for (int i = 0; i < 20; i++) {
                await queue.AddMessageAsync(new CloudQueueMessage(string.Format("{0} - {1}", i, "Hello World")));
            }

            // The FetchAttributes method asks the Queue service to retrieve the queue attributes, including an approximation of message count 
            queue.FetchAttributes();
            int? cachedMessageCount = queue.ApproximateMessageCount;
            Console.WriteLine("Number of messages in queue: {0}", cachedMessageCount);

            // Dequeue a batch of 21 messages (up to 32) and set visibility timeout to 5 minutes. Note we are dequeuing 21 messages because the earlier
            // UpdateEnqueuedMessage method left a message on the queue hence we are retrieving that as well. 
            foreach (CloudQueueMessage msg in await queue.GetMessagesAsync(21, TimeSpan.FromMinutes(5), null, null)) {
                Console.WriteLine("Processing & deleting message with content: {0}", msg.AsString);
                // Process all messages in less than 5 minutes, deleting each message after processing.
                await queue.DeleteMessageAsync(msg);
            }
        }

        private static CloudStorageAccount CreateStorageAccountFromConnectionString(string storageConnectionString) {
            CloudStorageAccount storageAccount;
            try {
                storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            }
            catch (FormatException) {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                Console.ReadLine();
                throw;
            }
            catch (ArgumentException) {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                Console.ReadLine();
                throw;
            }
            return storageAccount;
        }

        private static async Task DeleteQueueAsync(CloudQueue queue) {
            Console.WriteLine("10. Delete the queue");
            await queue.DeleteAsync();
        }
    }
}
