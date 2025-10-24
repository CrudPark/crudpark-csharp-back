
# Use the official .NET 8 runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the official .NET 8 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["crud-park-back.csproj", "./"]
RUN dotnet restore "crud-park-back.csproj"

# Copy the rest of the source code
COPY . .

# Build the application
RUN dotnet build "crud-park-back.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "crud-park-back.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Create the final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create a non-root user for security
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "crud-park-back.dll"]
