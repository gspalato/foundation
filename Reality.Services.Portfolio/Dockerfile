FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

COPY ["Reality.Services.Portfolio/Reality.Gateway.csproj", "Reality.Services.Portfolio/"]
COPY ["Reality.Common/Reality.Common.csproj", "Reality.Common/"]
RUN dotnet restore "Reality.Services.Portfolio/Reality.Services.Portfolio.csproj"
COPY . .

RUN dotnet publish "/src/Reality.Services.Portfolio/Reality.Services.Portfolio.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Reality.Services.Portfolio.dll"]