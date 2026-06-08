FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["ShopHub.Api/ShopHub.Api.csproj", "ShopHub.Api/"]
COPY ["ShopHub.Application/ShopHub.Application.csproj", "ShopHub.Application/"]
COPY ["ShopHub.Domain/ShopHub.Domain.csproj", "ShopHub.Domain/"]
COPY ["ShopHub.Infrastructure/ShopHub.Infrastructure.csproj", "ShopHub.Infrastructure/"]

RUN dotnet restore "ShopHub.Api/ShopHub.Api.csproj"

COPY . .
WORKDIR "/src/ShopHub.Api"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ShopHub.Api.dll"]
