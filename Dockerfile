# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the .csproj file and restore dependencies
COPY BlazesoftMachine/BlazesoftMachine.csproj BlazesoftMachine/
RUN ls -la
RUN dotnet restore BlazesoftMachine/BlazesoftMachine.csproj

# Copy the rest of the source code
COPY BlazesoftMachine/. BlazesoftMachine/
RUN ls -la

# Build the project
WORKDIR /src/BlazesoftMachine
RUN dotnet publish -c Release -o /app/out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Define environment variables 
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Development

EXPOSE 80

ENTRYPOINT ["dotnet", "BlazesoftMachine.dll"]



