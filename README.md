# Uploader Backend

- Frontend [here](https://github.com/amai-brat/uploader_frontend)  
- **Stack**: ASP.NET Core 10.0 + Dapper + SQLite
- **Docker image**: [amaicock/uploader-api](https://hub.docker.com/r/amaicock/uploader-api)

## Features
- Upload files up to 50 MB (customizable)
- Remove upload by generated key
- Automatic thumbnail generation for images and videos using ffmpeg
- Anonymous uploading / upload by Twitch account to share uploads between browsers
- Static files stored to `ENV_App__StoragePath`, thumbnails are generated using ffmpeg and stored to `ENV_App__StoragePath` + `/t/`

## Quick start
```bash
mkdir -p /tmp/uploader 
sqlite3 /tmp/uploader/uploader.db < ./migrations/0_init.sql
sqlite3 /tmp/uploader/uploader.db < ./migrations/1_add_user.sql # and apply other migrations...

docker compose up # if you don't want to build, you can use `image: amaicock/uploader-api:latest` 
```

## Contributing
Feel free to create issue and make pull requests.  
Branch should be created from `dev` and named like `feature/{issue_number}/{name}`

## License
[AGPL-3.0](./LICENSE)