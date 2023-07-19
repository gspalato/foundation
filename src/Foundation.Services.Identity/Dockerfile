FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

COPY ["Foundation.Services.Identity/Foundation.Services.Identity.csproj", "Foundation.Services.Identity/"]
COPY ["Foundation.Common/Foundation.Common.csproj", "Foundation.Common/"]
RUN dotnet restore "Foundation.Services.Identity/Foundation.Services.Identity.csproj"
COPY . .

RUN dotnet publish "/src/Foundation.Services.Identity/Foundation.Services.Identity.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Foundation.Services.Identity.dll"]