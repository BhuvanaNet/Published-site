using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure;


namespace GuestBook_Data
{
    public class GuestBookDataSource
    {
        private static CloudStorageAccount storageAccount;
        private GuestBookDataContext context;
        static GuestBookDataSource()
        {
            storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
                

            CloudTableClient cloudTableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = cloudTableClient.GetTableReference("GuestBookEntity");
            table.CreateIfNotExists();
        }

        public GuestBookDataSource()
        {
            this.context = new GuestBookDataContext(storageAccount.CreateCloudTableClient());
        }

        public IEnumerable<GuestBookEntity> GetGuestBookEntries()
        {
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("GuestBookEntity");

            TableQuery<GuestBookEntity> query = new TableQuery<GuestBookEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, DateTime.UtcNow.ToString("MMddyyyy")));

            return table.ExecuteQuery(query);
        }

        public void AddGuestBookEntity(GuestBookEntity newItem)
        {
            TableOperation operation = TableOperation.Insert(newItem);
            CloudTable table = context.ServiceClient.GetTableReference("GuestBookEntity");
            table.Execute(operation);
        }

        public void UpdateImageThumbnail(string partitionKey, string rowKey, string thumbUrl)
        {
            CloudTable table = context.ServiceClient.GetTableReference("GuestBookEntity");
            TableOperation retrieveOperation = TableOperation.Retrieve<GuestBookEntity>(partitionKey, rowKey);

            TableResult retrieveResult = table.Execute(retrieveOperation);
            GuestBookEntity updateEntity = (GuestBookEntity)retrieveResult.Result;

            if (updateEntity != null)
            {
                updateEntity.ThumbnailUrl = thumbUrl;

                TableOperation replaceOperation = TableOperation.Replace(updateEntity);
                table.Execute(replaceOperation);
            }
        }
    }
}
