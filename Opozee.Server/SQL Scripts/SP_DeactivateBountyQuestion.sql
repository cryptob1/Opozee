USE [oposeeDb]
GO
/****** Object:  StoredProcedure [dbo].[SP_DeactivateBountyQuestion]    Script Date: 24-May-19 7:47:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SP_DeactivateBountyQuestion]
(
	@BountyId INT
)
AS
BEGIN
	
	UPDATE [dbo].[Bounty] SET IsActive = 0 WHERE Id=@BountyId
	
END