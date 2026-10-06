# ── Build stage ─────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore: copy every csproj first so restore is cached independently
COPY ["global.json", "./"]
COPY ["E-Commerce.Api/E-Commerce.Api.csproj",           "E-Commerce.Api/"]
COPY ["E-Commerce.Application/E-Commerce.Application.csproj", "E-Commerce.Application/"]
COPY ["E-Commerce.Domain/E-Commerce.Domain.csproj",     "E-Commerce.Domain/"]
COPY ["E-Commerce.Infrastructure/E-Commerce.Infrastructure.csproj", "E-Commerce.Infrastructure/"]
COPY ["E-Commerce.ReadModel/E-Commerce.ReadModel.csproj", "E-Commerce.ReadModel/"]

RUN dotnet restore "E-Commerce.Api/E-Commerce.Api.csproj"

# Copy remaining sources and publish
COPY . .
RUN dotnet publish "E-Commerce.Api/E-Commerce.Api.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ── Runtime stage ───────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "E-Commerce.Api.dll"]