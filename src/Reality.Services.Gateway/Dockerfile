#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

ADD ./Reality.Services.Gateway ./Reality.Services.Gateway
ADD ./Reality.Common ./Reality.Common
ADD ./Reality.SDK ./Reality.SDK

RUN dotnet restore "Reality.Services.Gateway/Reality.Services.Gateway.csproj"

RUN dotnet publish "/src/Reality.Services.Gateway/Reality.Services.Gateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Reality.Services.Gateway.dll"]
