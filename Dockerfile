# Constract stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

#Restore packages
COPY *.csproj ./
RUN dotnet restore

# Copy all proj files and run the build
COPY . ./
RUN dotnet publish -c Release -o out

# Runing stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app/out .

# Define enviroment variables 
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Development

EXPOSE 80

ENTRYPOINT ["dotnet", "BlazesoftMachine.exe"]
