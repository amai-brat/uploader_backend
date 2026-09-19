# Changelog

## 0.3.2 (19.09.2026)
#### Changes:
- Fix sending original filename in `/api/uploads`

## 0.3.1 (14.09.2026)
#### Changes:
- Fix sending uploadId instead of fileId in `/api/uploads`

## 0.3 (14.09.2026)
#### Changes:
- Added endpoint `/api/key` - get api key for user by twitch oauth2.0 token (create user if not exists)
- Added endpoint `/api/uploads` - get user's uploads by `X-Api-Key` header
- Added migration `1_add_user.sql` - adds user entity and nullable userId to upload

## 0.2 (13.07.2026)
- Get uploader's IP address from header `X-Forwarded-For`

## 0.1 (13.07.2026)
- Base app