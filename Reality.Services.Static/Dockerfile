#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Reality.Services.Static/Reality.Services.Static.csproj", "Reality.Services.Static/"]
COPY ["Reality.Common/Reality.Common.csproj", "Reality.Common/"]
RUN dotnet restore "Reality.Services.Static/Reality.Services.Static.csproj"
COPY . .
WORKDIR "/src/Reality.Services.Static"
RUN dotnet build "Reality.Services.Static.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Reality.Services.Static.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Reality.Services.Static.dll"]