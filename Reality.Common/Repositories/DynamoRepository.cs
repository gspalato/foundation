using Amazon.DynamoDBv2;
using Reality.Common.Configurations;
using Reality.Common.Entities;

namespace Reality.Common.Repositories
{
    public interface IDynamoRepository { }

    public class BaseDynamoRepository<T> : IDynamoRepository
    {
        protected readonly IAmazonDynamoDB Database;

        public BaseDynamoRepository(IAmazonDynamoDB database)
        {
            Database = database;
        }
    }
}
