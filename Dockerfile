# ---------- Stage 1: Build ----------
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copy csproj from subfolder and restore
COPY AuthBackend/AuthBackend.csproj ./AuthBackend.csproj
RUN dotnet restore AuthBackend.csproj

# Copy the rest of the source code
COPY AuthBackend/. ./ 

# Publish the app
RUN dotnet publish AuthBackend.csproj -c Release -o out

# ---------- Stage 2: Runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app

COPY --from=build /app/out ./
EXPOSE 5000

ENTRYPOINT ["dotnet", "AuthBackend.dll"]