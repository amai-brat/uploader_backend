create table Uploads
(
    Id               INTEGER not null
        constraint PK_Uploads
            primary key autoincrement,
    UploadTime       TEXT    not null,
    FileId           TEXT    not null,
    OriginalFilename TEXT    not null,
    Key              TEXT    not null,
    ChecksumMd5      TEXT    not null,
    ContentType      TEXT,
    Extension        TEXT,
    Size             INTEGER not null,
    UserAgent        TEXT,
    RemoteIpAddress  TEXT,
    IsDeleted        INTEGER not null
);

create unique index IX_Uploads_FileId
    on Uploads (FileId);