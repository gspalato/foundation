using System.Globalization;
using HotChocolate;
using Reality.Common.Entities;
using Reality.Services.Static;

namespace Reality.Services.Static.Types
{
    public class Mutation
    {
        public async Task<string> UploadStaticFile(IFile file)
        {
            var fileName = file.Name;
            var fileSize = file.Length;

            await using Stream stream = file.OpenReadStream();

            return "";
        }
    }
}