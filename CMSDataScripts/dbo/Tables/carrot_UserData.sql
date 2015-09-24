CREATE TABLE [dbo].[carrot_UserData] (
    [UserId]       UNIQUEIDENTIFIER NOT NULL,
    [UserNickName] NVARCHAR (64)    NULL,
    [FirstName]    NVARCHAR (64)    NULL,
    [LastName]     NVARCHAR (64)    NULL,
    [UserBio]      NVARCHAR (MAX)   NULL,
    [UserKey]      NVARCHAR (128)   NULL,
    CONSTRAINT [PK_carrot_UserData] PRIMARY KEY NONCLUSTERED ([UserId] ASC),
    CONSTRAINT [carrot_UserData_UserKey] FOREIGN KEY ([UserKey]) REFERENCES [dbo].[membership_User] ([Id])
);

