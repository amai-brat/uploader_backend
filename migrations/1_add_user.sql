create table Users
(
    Id             INTEGER not null
        constraint PK_Users
            primary key autoincrement,
    TwitchUserId   TEXT    not null,
    TwitchUsername TEXT    not null,
    ApiKey         TEXT
);

create unique index IX_Users_TwitchUserId
    on Users (TwitchUserId);

alter table Uploads add column UserId INTEGER references Users (Id);