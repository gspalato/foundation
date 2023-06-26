using System.Net;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Reality.Common.Configurations;
using Reality.Common.Entities;
using Reality.Common.Repositories;

namespace Reality.Services.Identity.Repositories
{
    public class UserRepository : BaseDynamoRepository<FullUser>, IDynamoRepository
    {
        public string TableName { get; } = "reality-users";

        private readonly Reality.Services.Identity.Configuration Configuration;
        private readonly IAmazonDynamoDB Database;

        public UserRepository(IAmazonDynamoDB database, Reality.Services.Identity.Configuration configuration) : base(database)
        {
            Configuration = configuration;
            Database = database;
        }

        public async Task<bool> CreateUserAsync(FullUser user)
        {
            var serialized = JsonSerializer.Serialize(user);
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

        public async Task<FullUser> GetUserAsync(string username)
        {
            try {
                var request = Database.QueryAsync(new() {
                    TableName = TableName,
                    IndexName = "UsernameIndex",
                    KeyConditionExpression = "Username = :username",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>() {
                        { ":username", new AttributeValue(username) }
                    }
                });

                var response = await request;

                if (response.HttpStatusCode != HttpStatusCode.OK)
                    throw new Exception($"Failed to get user {username} from database.");

                if (response.Items.Count == 0)
                    throw new Exception($"User {username} does not exist.");

                var document = Document.FromAttributeMap(response.Items[0]);
                var user = JsonSerializer.Deserialize<FullUser>(document.ToJson());

                Console.WriteLine($"Got user {user.Username} from database.");
                Console.WriteLine(document.ToJsonPretty());

                return user;
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return null;
            }
        }

        public async Task<bool> DeleteUserAsync(string username)
        {
            var request = Database.DeleteItemAsync(new DeleteItemRequest()
            {
                TableName = TableName,
                Key = new Dictionary<string, AttributeValue>()
                {
                    { "Username", new AttributeValue(username) }
                }
            });

            var response = await request;

            return response.HttpStatusCode == HttpStatusCode.OK;
        }

        public async Task<bool> DeleteUserByIdAsync(string id)
        {
            var request = Database.DeleteItemAsync(new DeleteItemRequest()
            {
                TableName = TableName,
                Key = new Dictionary<string, AttributeValue>()
                {
                    { "Id", new AttributeValue(id) }
                }
            });

            var response = await request;

            return response.HttpStatusCode == HttpStatusCode.OK;
        }
    }
}
