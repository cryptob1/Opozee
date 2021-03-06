USE [oposeeDb]
GO
/****** Object:  StoredProcedure [dbo].[SP_SetBountyQuestions]    Script Date: 24-May-19 12:42:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--EXEC [dbo].[SP_SetBountyQuestions] 108,'2019-05-23','2019-05-25',1
ALTER PROCEDURE [dbo].[SP_SetBountyQuestions]
(
	@QuestionId INT,
	@StartDate DATETIME,
	@EndDate DATETIME,
	@IsActive BIT
)
AS
BEGIN
	
	INSERT INTO [dbo].[Bounty] 
	(
		QuestionId, 
		StartDate, 
		EndDate, 
		IsActive, 
		CreationDate
	)
	VALUES
	(
		@QuestionId, 
		@StartDate, 
		@EndDate, 
		@IsActive, 
		GETDATE() 
	) 
END
