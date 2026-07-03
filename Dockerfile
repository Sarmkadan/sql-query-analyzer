FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /app
COPY *.csproj ./
RUN dotnet restore
COPY . .
RUN dotnet publish sql-query-analyzer.csproj -c Release -o out
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine
WORKDIR /app
COPY --from=build /app/out/ .
EXPOSE 8080
ENTRYPOINT ["dotnet", "sql-query-analyzer.dll"]
