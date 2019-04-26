
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 04/25/2019 23:18:48
-- Generated from EDMX file: C:\code\Opozee\Opozee.Models\Models\Opozee.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [oposeeDb];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[BookMarks]', 'U') IS NOT NULL
    DROP TABLE [dbo].[BookMarks];
GO
IF OBJECT_ID(N'[dbo].[Notifications]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Notifications];
GO
IF OBJECT_ID(N'[dbo].[Opinions]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Opinions];
GO
IF OBJECT_ID(N'[dbo].[Questions]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Questions];
GO
IF OBJECT_ID(N'[dbo].[Tokens]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Tokens];
GO
IF OBJECT_ID(N'[dbo].[Users]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Users];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'BookMarks'
CREATE TABLE [dbo].[BookMarks] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [QuestionId] int  NOT NULL,
    [IsBookmark] bit  NULL,
    [UserId] int  NOT NULL,
    [CreationDate] datetime  NULL,
    [ModifiedDate] datetime  NULL
);
GO

-- Creating table 'Notifications'
CREATE TABLE [dbo].[Notifications] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [CommentedUserId] int  NOT NULL,
    [CommentId] int  NOT NULL,
    [questId] int  NULL,
    [Like] bit  NULL,
    [Dislike] bit  NULL,
    [Comment] bit  NULL,
    [SendNotification] bit  NULL,
    [CreationDate] datetime  NULL,
    [ModifiedDate] datetime  NULL
);
GO

-- Creating table 'Opinions'
CREATE TABLE [dbo].[Opinions] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [QuestId] int  NOT NULL,
    [Comment] varchar(max)  NOT NULL,
    [CommentedUserId] int  NOT NULL,
    [IsAgree] bit  NULL,
    [Likes] int  NULL,
    [Dislikes] int  NULL,
    [CreationDate] datetime  NULL,
    [ModifiedDate] datetime  NULL
);
GO

-- Creating table 'Questions'
CREATE TABLE [dbo].[Questions] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [PostQuestion] nvarchar(max)  NULL,
    [OwnerUserID] int  NOT NULL,
    [IsDeleted] bit  NULL,
    [TaggedUser] varchar(max)  NULL,
    [HashTags] varchar(1000)  NULL,
    [CreationDate] datetime  NULL,
    [ModifiedDate] datetime  NULL,
    [IsSlider] bit  NULL
);
GO

-- Creating table 'Tokens'
CREATE TABLE [dbo].[Tokens] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [TotalToken] int  NULL,
    [BalanceToken] int  NULL,
    [UserId] int  NOT NULL
);
GO

-- Creating table 'Users'
CREATE TABLE [dbo].[Users] (
    [UserID] int IDENTITY(1,1) NOT NULL,
    [UserName] nvarchar(100)  NOT NULL,
    [FirstName] varchar(20)  NULL,
    [LastName] varchar(20)  NULL,
    [Email] varchar(100)  NULL,
    [Password] nvarchar(50)  NULL,
    [IsAdmin] bit  NULL,
    [SocialID] varchar(50)  NULL,
    [SocialType] varchar(50)  NULL,
    [ImageURL] varchar(300)  NULL,
    [DeviceType] varchar(50)  NULL,
    [DeviceToken] varchar(200)  NULL,
    [RecordStatus] varchar(20)  NULL,
    [ModifiedDate] datetime  NULL,
    [CreatedDate] datetime  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'BookMarks'
ALTER TABLE [dbo].[BookMarks]
ADD CONSTRAINT [PK_BookMarks]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Notifications'
ALTER TABLE [dbo].[Notifications]
ADD CONSTRAINT [PK_Notifications]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Opinions'
ALTER TABLE [dbo].[Opinions]
ADD CONSTRAINT [PK_Opinions]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Questions'
ALTER TABLE [dbo].[Questions]
ADD CONSTRAINT [PK_Questions]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Tokens'
ALTER TABLE [dbo].[Tokens]
ADD CONSTRAINT [PK_Tokens]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [UserID] in table 'Users'
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [PK_Users]
    PRIMARY KEY CLUSTERED ([UserID] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------