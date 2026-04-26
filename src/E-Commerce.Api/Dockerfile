FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["E-Commerce.Api/E-Commerce.Api.csproj", "E-Commerce.Api/"]
COPY ["E-Commerce.Application/E-Commerce.Application.csproj", "E-Commerce.Application/"]
RUN dotnet restore "E-Commerce.Api/E-Commerce.Api.csproj"
COPY . .
WORKDIR "/src/E-Commerce.Api"
RUN dotnet build "E-Commerce.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "E-Commerce.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "E_Commerce.Api.dll"]
