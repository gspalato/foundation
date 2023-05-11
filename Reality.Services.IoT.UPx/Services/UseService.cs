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
			using (var client = new GraphQLHttpClient("https://0.0.0.0/", new NewtonsoftJsonSerializer()))
			{
				var request = new GraphQLRequest
				{
					Query = @"
						query {
							isAuthenticated(token: $token)
						}
					",
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
		}
	}
}
