#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

ADD ./services/upx ./services/upx
ADD ../lib/common ./lib/common
ADD ../lib/sdk ./lib/sdk

RUN echo $(ls services -R)

RUN dotnet restore "/src/services/upx/Foundation.Services.UPx.csproj"

RUN dotnet publish "/src/services/upx/Foundation.Services.UPx.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Foundation.Services.UPx.dll"]