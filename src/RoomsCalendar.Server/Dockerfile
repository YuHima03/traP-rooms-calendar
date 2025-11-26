# Generate css files with TailwindCSS
FROM node:alpine AS prepare
WORKDIR /src
COPY . .
RUN apk update \
    && npx -y @go-task/cli tailwind-gen

# Build and Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY --from=prepare /src .
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Base
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
EXPOSE 8080
USER root
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
RUN apk add --upgrade --no-cache icu-data-full icu-libs tzdata

# Final
FROM base AS final
USER $APP_UID
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RoomsCalendar.Server.dll"]
