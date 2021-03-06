USE [oposeeDb]
GO
/****** Object:  StoredProcedure [dbo].[SP_GetBountyQuestions]    Script Date: 24-May-19 7:06:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[SP_GetBountyQuestions]
(
	@StartDate DATETIME = NULL,
	@EndDate DATETIME = NULL
)
AS
BEGIN
	
	IF(@StartDate IS NOT NULL AND @EndDate IS NOT NULL)
	BEGIN
		SELECT B.Id AS BountyId, B.QuestionId, B.StartDate, B.EndDate, B.IsActive,
			B.CreationDate AS BountyCreatedOn, B.QuestionId AS QuestionId1,
			Q.PostQuestion, Q.HashTags, Q.TaggedUser, Q.CreationDate AS QuestionCreatedOn, 
			Q.OwnerUserID AS UserId, U.UserName, U.Email, U.SocialID, U.ImageURL,
			(SELECT COUNT(*) FROM [dbo].[Opinion] O WHERE O.QuestId= B.QuestionId AND O.IsAgree=1 AND O.CreationDate >= @StartDate AND O.CreationDate <= @EndDate) AS YesCount,
			(SELECT COUNT(*) FROM [dbo].[Opinion] O WHERE O.QuestId=B.QuestionId AND O.IsAgree=0 AND O.CreationDate >= @StartDate AND O.CreationDate <= @EndDate) AS NoCount,
			(SELECT COUNT(*) FROM [dbo].[Notification] N WHERE N.QuestId=B.QuestionId AND N.[Like]=1 AND N.CreationDate >= @StartDate AND N.CreationDate <= @EndDate) AS TotalLikes,
			(SELECT COUNT(*) FROM [dbo].[Notification] N WHERE N.QuestId=B.QuestionId AND N.Dislike=1 AND N.CreationDate >= @StartDate AND N.CreationDate <= @EndDate) AS TotalDisLikes,
			(
				(SELECT COUNT(*) FROM [dbo].[Opinion] O WHERE O.QuestId= B.QuestionId AND O.IsAgree=1 AND O.CreationDate >= @StartDate AND O.CreationDate <= @EndDate) +
				(SELECT COUNT(*) FROM [dbo].[Opinion] O WHERE O.QuestId=B.QuestionId AND O.IsAgree=0 AND O.CreationDate >= @StartDate AND O.CreationDate <= @EndDate) +
				(SELECT COUNT(*) FROM [dbo].[Notification] N WHERE N.QuestId=B.QuestionId AND N.[Like]=1 AND N.CreationDate >= @StartDate AND N.CreationDate <= @EndDate) +
				(SELECT COUNT(*) FROM [dbo].[Notification] N WHERE N.QuestId=B.QuestionId AND N.Dislike=1 AND N.CreationDate >= @StartDate AND N.CreationDate <= @EndDate)
			) AS Score
		FROM [dbo].[Bounty] B
		JOIN [dbo].[Question] Q ON B.[QuestionId] = Q.Id
		JOIN [dbo].[Users] U ON Q.[OwnerUserID] = U.UserID
		WHERE B.[StartDate] >= @StartDate AND B.[EndDate] <= @EndDate 
			AND B.[IsActive] = 1 AND Q.IsDeleted = 0
	END
	ELSE
	BEGIN
		SELECT B.Id AS BountyId, B.QuestionId, B.StartDate, B.EndDate, B.IsActive,
			B.CreationDate AS BountyCreatedOn,B.QuestionId AS QuestionId1,
			Q.PostQuestion, Q.HashTags, Q.TaggedUser, Q.CreationDate AS QuestionCreatedOn,
			Q.OwnerUserID AS UserId, U.UserName, U.Email, U.SocialID, U.ImageURL,
			(SELECT COUNT(*) FROM [dbo].[Opinion] O WHERE O.QuestId= B.QuestionId AND O.IsAgree=1 AND O.CreationDate >= B.StartDate AND O.CreationDate <= B.EndDate) AS YesCount,
			(SELECT COUNT(*) FROM [dbo].[Opinion] O WHERE O.QuestId=B.QuestionId AND O.IsAgree=0 AND O.CreationDate >= B.StartDate AND O.CreationDate <= B.EndDate) AS NoCount,
			(SELECT COUNT(*) FROM [dbo].[Notification] N WHERE N.QuestId=B.QuestionId AND N.[Like]=1 AND N.CreationDate >= B.StartDate AND N.CreationDate <= B.EndDate) AS TotalLikes,
			(SELECT COUNT(*) FROM [dbo].[Notification] N WHERE N.QuestId=B.QuestionId AND N.Dislike=1 AND N.CreationDate >= B.StartDate AND N.CreationDate <= B.EndDate) AS TotalDisLikes,
			(
				(SELECT COUNT(*) FROM [dbo].[Opinion] O WHERE O.QuestId= B.QuestionId AND O.IsAgree=1 AND O.CreationDate >= B.StartDate AND O.CreationDate <= B.EndDate) +
				(SELECT COUNT(*) FROM [dbo].[Opinion] O WHERE O.QuestId=B.QuestionId AND O.IsAgree=0 AND O.CreationDate >= B.StartDate AND O.CreationDate <= B.EndDate) +
				(SELECT COUNT(*) FROM [dbo].[Notification] N WHERE N.QuestId=B.QuestionId AND N.[Like]=1 AND N.CreationDate >= B.StartDate AND N.CreationDate <= B.EndDate) +
				(SELECT COUNT(*) FROM [dbo].[Notification] N WHERE N.QuestId=B.QuestionId AND N.Dislike=1 AND N.CreationDate >= B.StartDate AND N.CreationDate <= B.EndDate)
			) AS Score
		FROM [dbo].[Bounty] B
		JOIN [dbo].[Question] Q ON B.[QuestionId] = Q.Id
		JOIN [dbo].[Users] U ON Q.[OwnerUserID] = U.UserID
		WHERE  B.[IsActive] = 1 AND Q.IsDeleted = 0
	END
END


--EXEC [dbo].[SP_GetBountyQuestions] '2019-05-23','2019-05-25'

--EXEC [dbo].[SP_SetBountyQuestions] 112,'2019-05-26','2019-05-28',1

--SELECT * FROM  [dbo].[Notification]


