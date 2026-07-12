FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /source

COPY src/Directory.Build.props src/
COPY src/Directory.Packages.props src/

COPY src/Uploader.Core/Uploader.Core.csproj src/Uploader.Core/
COPY src/Uploader.Feature/Uploader.Feature.csproj src/Uploader.Feature/
COPY src/Uploader.Infrastructure/Uploader.Infrastructure.csproj src/Uploader.Infrastructure/
COPY src/Uploader.Web/Uploader.Web.csproj src/Uploader.Web/

RUN dotnet restore src/Uploader.Web/Uploader.Web.csproj -r linux-musl-x64

COPY src/ src/
WORKDIR /source/src/Uploader.Web

RUN dotnet publish Uploader.Web.csproj \
    -c Release \
    -r linux-musl-x64 \
    --no-restore \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app

RUN apk add --no-cache ffmpeg 

ENV ASPNETCORE_HTTP_PORTS=5000

COPY --from=build /app/publish .

EXPOSE 5000

ENTRYPOINT ["./Uploader.Web"]