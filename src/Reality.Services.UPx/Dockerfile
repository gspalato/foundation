FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

COPY ["Reality.Services.UPx/Reality.Services.UPx.csproj", "Reality.Services.UPx/"]
COPY ["Reality.Common/Reality.Common.csproj", "Reality.Common/"]
RUN dotnet restore "Reality.Services.UPx/Reality.Services.UPx.csproj"
COPY . .

RUN dotnet publish "/src/Reality.Services.UPx/Reality.Services.UPx.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Reality.Services.UPx.dll"]