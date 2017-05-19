IF EXISTS (
  SELECT * FROM INFORMATION_SCHEMA.ROUTINES 
   WHERE SPECIFIC_SCHEMA = N'dbo'
     AND SPECIFIC_NAME = N'UnlockAnalysis' 
)
BEGIN
 PRINT N'Dropping Procedure dbo.UnlockAnalysis'
 DROP PROCEDURE dbo.UnlockAnalysis
END
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO
-- UnlockAnalysis 1

PRINT 'Creating Procedure dbo.UnlockAnalysis'
GO

CREATE PROCEDURE UnlockAnalysis
(
	@CommentID INT
)
AS

SET NOCOUNT ON

--TRUNCATION HACK
SET ANSI_WARNINGS OFF

UPDATE Comment
SET InAnalysis = 0, StartAnalysis = NULL
WHERE CommentID = @CommentID
GO

GRANT EXECUTE ON LockAnalysis to public
GO