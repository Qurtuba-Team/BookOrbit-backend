# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy props
COPY Directory.Build.props .
COPY Directory.Packages.props .

# Copy csproj files
COPY BookOrbit.Api/BookOrbit.Api.csproj BookOrbit.Api/
COPY BookOrbit.Application/BookOrbit.Application.csproj BookOrbit.Application/
COPY BookOrbit.Infrastructure/BookOrbit.Infrastructure.csproj BookOrbit.Infrastructure/
COPY BookOrbit.Domain/BookOrbit.Domain.csproj BookOrbit.Domain/

# Restore
RUN dotnet restore BookOrbit.Api/BookOrbit.Api.csproj

# Copy everything
COPY . .

# Publish
RUN dotnet publish BookOrbit.Api/BookOrbit.Api.csproj -c Release -o /app/publish

# Final Stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

# Timezone
ENV TZ=Africa/Cairo

WORKDIR /app

COPY --from=build /app/publish .
RUN cp -r /app/wwwroot /app/wwwroot_backup
COPY docker-entrypoint.sh /docker-entrypoint.sh

RUN chmod +x /docker-entrypoint.sh


ENTRYPOINT ["/docker-entrypoint.sh"]
