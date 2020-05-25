FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /app
COPY *.csproj ./
RUN dotnet restore
COPY . .
RUN dotnet publish sql-query-analyzer.csproj -c Release -o out
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine
RUN apk add --no-cache curl
WORKDIR /app
COPY --from=build /app/out/ .
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/api/health || exit 1
ENTRYPOINT ["dotnet", "sql-query-analyzer.dll"]
