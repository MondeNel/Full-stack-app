# ---------- Stage 1: Build ----------
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy all source code and publish
COPY . ./
RUN dotnet publish -c Release -o out

# ---------- Stage 2: Runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app

# Set environment variable to allow container to listen on all IPs
ENV ASPNETCORE_URLS=http://+:5000

# Copy published files from build stage
COPY --from=build /app/out ./

# Expose the port
EXPOSE 5000

# Run the application
ENTRYPOINT ["dotnet", "AuthBackend.dll"]