# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
# For more information, please see https://aka.ms/containercompat

# Stage 1: Runtime base image
# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Stage 2: Build SDK image
# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["PartnerIntegrationBff.csproj", "./"]
RUN dotnet restore "PartnerIntegrationBff.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "PartnerIntegrationBff.csproj" -c %BUILD_CONFIGURATION% -o /app/build

# Stage 3: Publish output
# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PartnerIntegrationBff.csproj" -c %BUILD_CONFIGURATION% -o /app/publish /p:UseAppHost=false

# Stage 4: Final container runtime
# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

COPY ["appsettings-staging.json", "./appsettings.json"]

ENTRYPOINT ["dotnet", "PartnerIntegrationBff.dll"]
