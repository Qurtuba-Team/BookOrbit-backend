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

RUN apt-get update && apt-get install -y tzdata openssl && \
    ln -fs /usr/share/zoneinfo/$TZ /etc/localtime && \
    dpkg-reconfigure -f noninteractive tzdata && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY --from=build /app/publish .
COPY docker-entrypoint.sh /docker-entrypoint.sh

RUN chmod +x /docker-entrypoint.sh

EXPOSE 80
EXPOSE 443

ENTRYPOINT ["/docker-entrypoint.sh"]
