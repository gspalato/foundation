using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using Foundation.Common.Roles;
using Foundation.Common.Entities;
using Foundation.Core.SDK.Database.Mongo;
using Amazon.S3.Model;
using Amazon.S3;
using Foundation.Services.Identity.Configurations;

namespace Foundation.Services.Identity.Services;

public interface IUserService
{
    Task<FullUser?> GetUserAsync(string username);

    Task<FullUser?> GetUserByIdAsync(string id);

    Task<FullUser?> CreateUserAsync(string username, string password);

    Task DeleteUserAsync(string username);

    string GetProfilePictureUrl(string id);

    Task<string?> GetProfilePictureUploadUrlAsync(string id);
}

public class UserService : IUserService
{
    private readonly IAmazonS3 S3Client;
    private readonly IPasswordHasher<string> Hasher;

    private readonly IIdentityConfiguration Configuration;

    private readonly IMongoCollection<FullUser> Users;

    public UserService(IDatabaseContext databaseContext, IAmazonS3 s3Client,
    IPasswordHasher<string> hasher, IIdentityConfiguration configuration)
    {
        S3Client = s3Client;
        Hasher = hasher;

        Configuration = configuration;

        Users = databaseContext.GetCollection<FullUser>(nameof(Users));
    }

    public async Task<FullUser?> CreateUserAsync(string username, string password)
    {
        if (await GetUserAsync(username) is not null)
            return null;

        var hashedPassword = Hasher.HashPassword(username, password);

        var user = new FullUser()
        {
            Username = username,
            PasswordHash = hashedPassword,
            Roles = Array.Empty<Role>()
        };

        await Users.InsertOneAsync(user);

        return user;
    }

    public async Task DeleteUserAsync(string username)
    {
        var filter = Builders<FullUser>.Filter.Where(x => x.Username == username);
        await Users.FindOneAndDeleteAsync(filter);
    }

    public async Task<FullUser?> GetUserAsync(string username)
    {
        var filter = Builders<FullUser>.Filter.Where(x => x.Username == username);
        var found = (await Users.FindAsync(filter)).FirstOrDefault();

        if (found is not null)
            found.ProfilePictureUrl = GetProfilePictureUrl(found.Id);

        return found;
    }

    public async Task<FullUser?> GetUserByIdAsync(string id)
    {
        var filter = Builders<FullUser>.Filter.Where(x => x.Id == id);
        var found = (await Users.FindAsync(filter)).FirstOrDefault();

        if (found is not null)
            found.ProfilePictureUrl = GetProfilePictureUrl(found.Id);

        return found;
    }

    public string GetProfilePictureUrl(string id)
    {
        return string.Format(
            Configuration.AwsFoundationIdentityProfilePictureUrlFormat,
            Configuration.AwsFoundationIdentityProfilePictureBucket,
            id
        );
    }

    public async Task<string?> GetProfilePictureUploadUrlAsync(string id)
    {
        if ((await GetUserByIdAsync(id)) is null)
            return null;

        const int duration = 3;
        string bucketName = Configuration.AwsFoundationIdentityProfilePictureBucket;

        string urlString = string.Empty;
        try
        {
            var request = new GetPreSignedUrlRequest()
            {
                BucketName = bucketName,
                Key = id,
                Expires = DateTime.UtcNow.AddMinutes(duration),
            };

            urlString = S3Client.GetPreSignedURL(request);
        }
        catch (AmazonS3Exception ex)
        {
            Console.WriteLine($"Failed to get presigned URL: {ex.Message}");
        }

        return urlString;
    }
}

