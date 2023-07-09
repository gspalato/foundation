#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
ADD Reality.Common/ Reality.Common/
ADD Reality.Services.UPx/ Reality.Services.UPx/

WORKDIR /src/Reality.Services.UPx
RUN dotnet publish "Reality.Services.UPx.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Reality.Services.UPx.dll"]