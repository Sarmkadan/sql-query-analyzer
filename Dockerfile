FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /app
COPY *.csproj ./
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o out
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine
WORKDIR /app
COPY --from=build /app/out/ .
HEALTHCHECK --interval=10s --timeout=5s --retries=3 CMD curl --fail http://localhost:5000/health || exit 1
RUN addgroup -g 1000 appgroup && adduser -G appgroup -u 1000 appuser
USER appuser:appgroup
EXPOSE 5000
ENTRYPOINT ["dotnet", "sql-query-analyzer.dll"]