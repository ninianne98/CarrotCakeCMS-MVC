CREATE TABLE [dbo].[membership_UserRole] (
    [UserId] NVARCHAR (128) NOT NULL,
    [RoleId] NVARCHAR (128) NOT NULL,
    CONSTRAINT [PK_dbo.membership_UserRole] PRIMARY KEY CLUSTERED ([UserId] ASC, [RoleId] ASC),
    CONSTRAINT [FK_dbo.membership_UserRole_dbo.membership_Role_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[membership_Role] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.membership_UserRole_dbo.membership_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[membership_User] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_UserId]
    ON [dbo].[membership_UserRole]([UserId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_RoleId]
    ON [dbo].[membership_UserRole]([RoleId] ASC);

