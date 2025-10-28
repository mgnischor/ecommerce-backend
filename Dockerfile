FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src
COPY ["ecommerce-backend.csproj", "./"]
RUN dotnet restore "ecommerce-backend.csproj"
COPY . .
RUN dotnet publish "ecommerce-backend.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
ARG BUILD_DEVELOPMENT=1
RUN apk upgrade --no-cache && \
    apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
WORKDIR /app
RUN if [ "$BUILD_DEVELOPMENT" = "1" ]; then \
        echo '#!/bin/sh' > /app/entrypoint.sh && \
        echo 'export ASPNETCORE_ENVIRONMENT=Development' >> /app/entrypoint.sh && \
        echo 'export ASPNETCORE_URLS=http://+:5049' >> /app/entrypoint.sh && \
        echo 'export ASPNETCORE_HTTP_PORTS=5049' >> /app/entrypoint.sh && \
        echo 'exec dotnet ECommerce.Backend.dll' >> /app/entrypoint.sh; \
    else \
        echo '#!/bin/sh' > /app/entrypoint.sh && \
        echo 'export ASPNETCORE_ENVIRONMENT=Production' >> /app/entrypoint.sh && \
        echo 'export ASPNETCORE_URLS=http://+:80' >> /app/entrypoint.sh && \
        echo 'export ASPNETCORE_HTTP_PORTS=80' >> /app/entrypoint.sh && \
        echo 'exec dotnet ECommerce.Backend.dll' >> /app/entrypoint.sh; \
    fi && \
    chmod +x /app/entrypoint.sh
RUN addgroup -g 1000 appgroup && \
    adduser -u 1000 -G appgroup -s /bin/sh -D appuser && \
    chown -R appuser:appgroup /app
COPY --from=build --chown=appuser:appgroup /app/publish .
USER appuser

ENTRYPOINT ["/app/entrypoint.sh"]

# Development stage - includes SDK and EF tools for migrations
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS development
WORKDIR /app
RUN apk upgrade --no-cache && \
    apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Install EF Core tools
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"

# Copy project files for EF migrations
COPY --from=build /src /src
WORKDIR /src

# Copy published app
COPY --from=build /app/publish /app
WORKDIR /app

# Create entrypoint
RUN echo '#!/bin/sh' > /app/entrypoint.sh && \
    echo 'export ASPNETCORE_ENVIRONMENT=Development' >> /app/entrypoint.sh && \
    echo 'export ASPNETCORE_URLS=http://+:5049' >> /app/entrypoint.sh && \
    echo 'export ASPNETCORE_HTTP_PORTS=5049' >> /app/entrypoint.sh && \
    echo 'exec dotnet ECommerce.Backend.dll' >> /app/entrypoint.sh && \
    chmod +x /app/entrypoint.sh

EXPOSE 5049
ENTRYPOINT ["/app/entrypoint.sh"]

# Production stage  
FROM final AS production
EXPOSE 80
