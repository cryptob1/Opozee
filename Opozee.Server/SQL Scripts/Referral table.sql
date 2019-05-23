USE [oposeeDb]

CREATE TABLE [dbo].[Referral](
	[Id] [int] Primary Key IDENTITY(1,1)  NOT NULL,
	[UserId] [int] NOT NULL,
	[ReferredId] [int] NOT NULL,
	[CreationDate] [datetime] NULL,
	[IsDeleted] [bit] NULL,
 )


ALTER TABLE [dbo].[Users] 
ADD COLUMN [ReferralCode] NVARCHAR(10)