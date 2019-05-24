USE [oposeeDb]
GO

CREATE TABLE [dbo].[Bounty](
	[Id] [int] PRIMARY KEY IDENTITY(1,1) NOT NULL,
	[QuestionId] [int] NOT NULL,
	[StartDate] [datetime2](7) NOT NULL,
	[EndDate] [datetime2](7) NOT NULL,
	[IsActive] [bit] NULL,
	[Score] [int] NULL,
	[CreationDate] [datetime] NULL,
 )

