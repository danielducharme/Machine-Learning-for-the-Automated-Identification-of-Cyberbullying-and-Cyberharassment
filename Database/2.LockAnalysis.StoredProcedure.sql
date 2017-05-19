IF EXISTS (
  SELECT * FROM INFORMATION_SCHEMA.ROUTINES 
   WHERE SPECIFIC_SCHEMA = N'dbo'
     AND SPECIFIC_NAME = N'LockAnalysis' 
)
BEGIN
 PRINT N'Dropping Procedure dbo.LockAnalysis'
 DROP PROCEDURE dbo.LockAnalysis
END
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO
-- LockAnalysis 1, 'Test'

PRINT 'Creating Procedure dbo.LockAnalysis'
GO

CREATE PROCEDURE LockAnalysis
(
	@CommentID INT,
	@ProcessID VARCHAR(MAX)
)
AS

SET NOCOUNT ON

--TRUNCATION HACK
SET ANSI_WARNINGS OFF

BEGIN TRANSACTION
UPDATE Comment
SET InAnalysis = @ProcessID, StartAnalysis = GETDATE()
WHERE CommentID = @CommentID
	AND InAnalysis = 0

IF @ProcessID <> (SELECT InAnalysis FROM Comment WHERE CommentID = @CommentID)
	GOTO bad

COMMIT TRANSACTION
GOTO finally

bad:
ROLLBACK TRANSACTION

finally:
SELECT ISNULL(InAnalysis, -1) AS ProcessID
FROM Comment
WHERE InAnalysis = @ProcessID
	AND CommentID = @CommentID

GO

GRANT EXECUTE ON LockAnalysis to public
GO