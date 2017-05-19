IF EXISTS (
  SELECT * FROM INFORMATION_SCHEMA.ROUTINES 
   WHERE SPECIFIC_SCHEMA = N'dbo'
     AND SPECIFIC_NAME = N'UnlockProcess' 
)
BEGIN
 PRINT N'Dropping Procedure dbo.UnlockProcess'
 DROP PROCEDURE dbo.UnlockProcess
END
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO
-- UnlockProcess 1

PRINT 'Creating Procedure dbo.UnlockProcess'
GO

CREATE PROCEDURE UnlockProcess
(
	@CommentID INT
)
AS

SET NOCOUNT ON

--TRUNCATION HACK
SET ANSI_WARNINGS OFF

UPDATE Comment
SET InProcess = 0, StartProcess = NULL
WHERE CommentID = @CommentID
GO

GRANT EXECUTE ON UnlockProcess to public
GO