# ---------- Stage 1: Build ----------
# Use official .NET 7 SDK image to restore dependencies and build
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copy csproj first to leverage caching for dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the source code
COPY . ./

# Publish the app to a folder called 'out' in Release mode
RUN dotnet publish -c Release -o out

# ---------- Stage 2: Runtime ----------
# Use smaller runtime image for final container
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app

# Copy the published app from the build stage
COPY --from=build /app/out ./

# Expose port 5000
EXPOSE 5000

# ---------- Entry Point ----------
# Start the app normally; migrations are handled in docker-compose command
ENTRYPOINT ["dotnet", "AuthBackend.dll"]