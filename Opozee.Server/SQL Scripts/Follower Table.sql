CREATE TABLE [dbo].[Followers](
	[Id] [int] PRIMARY KEY IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[FollowedId] [int] NOT NULL,
	[CreationDate] [datetime] NULL,
	[IsFollowing] [bit] NULL
)