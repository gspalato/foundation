using Reality.Common.Data;
using Reality.Common.Repositories;
using Reality.Common.Entities;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using System.Net;
using System.Text.Json;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;

namespace Reality.Services.IoT.UPx.Repositories
{
    public interface IUseRepository : IDynamoRepository
    {
        Task<bool> CreateUseAsync(Use use);
        Task<List<Use>> GetUsesAsync();
    }

    public class UseRepository : BaseDynamoRepository<Use>, IUseRepository
    {
        public string TableName { get; } = "reality-upx-refillstation";

        public UseRepository(IAmazonDynamoDB database) : base(database)
        {

        }

        public async Task<bool> CreateUseAsync(Use use)
        {
            var serialized = JsonSerializer.Serialize(use);
            var document = Document.FromJson(serialized);
            var item = document.ToAttributeMap();

            var request = Database.PutItemAsync(new PutItemRequest()
            {
                TableName = TableName,
                Item = item
            });

            var response = await request;

            return response.HttpStatusCode == HttpStatusCode.OK;
        }

        public async Task<List<Use>> GetUsesAsync()
        {
            var request = Database.ScanAsync(new() {
                TableName = TableName,
                ScanFilter = new() { }
            });

            var response = await request;

            if (response.HttpStatusCode != HttpStatusCode.OK)
                    throw new Exception($"Failed to fetch uses from database.");

            return response.Items.Select(x => JsonSerializer.Deserialize<Use>(Document.FromAttributeMap(x).ToJson())!).ToList();
        }
    }
}