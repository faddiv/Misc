#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
# RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
# USER appuser

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS project
WORKDIR /src
COPY ["ViteCommerce.Api/ViteCommerce.Api.csproj", "ViteCommerce.Api/"]
RUN dotnet restore "ViteCommerce.Api/ViteCommerce.Api.csproj"
COPY . .
WORKDIR "/src/ViteCommerce.Api"

FROM project AS build
RUN dotnet build "ViteCommerce.Api.csproj" -c Debug -o /app/build
WORKDIR /app/build

FROM project AS publish
RUN dotnet publish "ViteCommerce.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ViteCommerce.Api.dll"]
