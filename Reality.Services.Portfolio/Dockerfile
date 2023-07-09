#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Reality.Services.Portfolio/Reality.Services.Portfolio.csproj", "Reality.Services.Portfolio/"]
COPY ["Reality.Common/Reality.Common.csproj", "Reality.Common/"]
RUN dotnet restore "Reality.Services.Portfolio/Reality.Services.Portfolio.csproj"
COPY . .

FROM build AS publish
WORKDIR "/src/Reality.Services.Portfolio"
RUN dotnet publish "Reality.Services.Portfolio.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Reality.Services.Portfolio.dll"]