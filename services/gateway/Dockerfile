#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

ADD ./Foundation.Services.Gateway ./Foundation.Services.Gateway
ADD ./Foundation.Common ./Foundation.Common
ADD ./Foundation.Core.SDK ./Foundation.Core.SDK

RUN dotnet restore "Foundation.Services.Gateway/Foundation.Services.Gateway.csproj"

RUN dotnet publish "/src/Foundation.Services.Gateway/Foundation.Services.Gateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Foundation.Services.Gateway.dll"]
