IF EXISTS (
  SELECT * FROM INFORMATION_SCHEMA.ROUTINES 
   WHERE SPECIFIC_SCHEMA = N'dbo'
     AND SPECIFIC_NAME = N'UpdateResult' 
)
BEGIN
 PRINT N'Dropping Procedure dbo.UpdateResult'
 DROP PROCEDURE dbo.UpdateResult
END
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO
-- UpdateResult 1, 1

PRINT 'Creating Procedure dbo.UpdateResult'
GO

CREATE PROCEDURE UpdateResult
(
	@CommentID INT,
	@Result INT
)
AS

SET NOCOUNT ON

--TRUNCATION HACK
SET ANSI_WARNINGS OFF

UPDATE Comment
SET Analyzed = 1, Bullying = @Result, EndAnalysis = GETDATE()
WHERE CommentID = @CommentID

GO

GRANT EXECUTE ON UpdateResult to public
GO