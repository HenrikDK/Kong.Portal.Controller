FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build-env
WORKDIR /app

# Copy everything and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine
RUN apk update
RUN apk add git
RUN apk add openssh-client

RUN adduser --disabled-password \
  --home /app \
  --gecos '' dotnetuser && chown -R dotnetuser /app

WORKDIR /app
COPY --from=build-env /app/out .

USER dotnetuser
ENTRYPOINT ["dotnet", "Kong.Portal.Controller.dll"]