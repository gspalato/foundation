#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Reality.Services.Identity/Reality.Services.Identity.csproj", "Reality.Services.Identity/"]
COPY ["Reality.Common/Reality.Common.csproj", "Reality.Common/"]
RUN dotnet restore "Reality.Services.Identity/Reality.Services.Identity.csproj"
COPY . .
WORKDIR "/src/Reality.Services.Identity"
RUN dotnet build "Reality.Services.Identity.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Reality.Services.Identity.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Reality.Services.Identity.dll"]