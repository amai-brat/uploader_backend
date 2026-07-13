# Uploader Backend

- Frontend [here](https://github.com/amai-brat/uploader_frontend)  
- **Stack**: ASP.NET Core 10.0 + Dapper + SQLite
- **Docker image**: [amaicock/uploader-api](https://hub.docker.com/repository/docker/amaicock/uploader-api/general)

Static files stored to `ENV_App__StoragePath`, thumbnails are generated using ffmpeg and stored to `ENV_App__StoragePath` + `/t/`

## Quick start
```bash
mkdir -p /tmp/uploader 
sqlite3 /tmp/uploader/uploader.db < ./init.sql

docker compose up # if you don't want to build, you can use `image: amaicock/uploader-api:latest` 
```
