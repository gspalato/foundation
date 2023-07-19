FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

COPY ["Foundation.Services.UPx/Foundation.Services.UPx.csproj", "Foundation.Services.UPx/"]
COPY ["Foundation.Common/Foundation.Common.csproj", "Foundation.Common/"]
RUN dotnet restore "Foundation.Services.UPx/Foundation.Services.UPx.csproj"
COPY . .

RUN dotnet publish "/src/Foundation.Services.UPx/Foundation.Services.UPx.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Foundation.Services.UPx.dll"]