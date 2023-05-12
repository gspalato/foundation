using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.IdentityModel.Tokens;
using Reality.Common.Payloads;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Reality.Services.IoT.UPx.Services
{
	public interface IUseService
	{
		Task<bool> CheckAuthenticationAsync(string jwt);
	}

	public class UseService : IUseService
	{
		private readonly SymmetricSecurityKey JwtSecurityKey;
		private readonly JwtSecurityTokenHandler JwtTokenHandler;

		public UseService(JwtSecurityTokenHandler jwtTokenHandler)
		{
			JwtSecurityKey = new SymmetricSecurityKey(
				Encoding.UTF8.GetBytes("ADD_SECRET_HERE_WHEN_POSSIBLE_128BITS"));
			JwtTokenHandler = jwtTokenHandler;
		}

		public async Task<bool> CheckAuthenticationAsync(string jwt)
		{
			/*
			using (var client = new GraphQLHttpClient("http://upx_service/", new NewtonsoftJsonSerializer()))
			{
				var request = new GraphQLRequest
				{
					Query = @"
					query CheckIdentity($token: String!) {
						isAuthenticated(token: $token)
					}
					",
					OperationName = "CheckIdentity",
					Variables = new
					{
						token = jwt
					},
				};

				var response = await client.SendMutationAsync<bool>(request);
				if (response.Errors is not null)
				{
                    response = await client.SendMutationAsync<bool>(request);
					if (response.Errors is not null)
						return false;
                }

				return response.Data;
			}
			*/

			var result = await JwtTokenHandler.ValidateTokenAsync(jwt, new TokenValidationParameters()
            {
                ValidIssuer = "realityapibyunreaalism",
                ValidAudience = "unreaalism",
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = JwtSecurityKey
            });

            if (result.Exception != null)
                Console.WriteLine(result.Exception.Message);

            return result!.IsValid;
		}
	}
}
