IF EXISTS (
  SELECT * FROM INFORMATION_SCHEMA.ROUTINES 
   WHERE SPECIFIC_SCHEMA = N'dbo'
     AND SPECIFIC_NAME = N'LockProcess' 
)
BEGIN
 PRINT N'Dropping Procedure dbo.LockProcess'
 DROP PROCEDURE dbo.LockProcess
END
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO
-- LockProcess 1, 'Test'

PRINT 'Creating Procedure dbo.LockProcess'
GO

CREATE PROCEDURE LockProcess
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
SET InProcess = @ProcessID, StartProcess = GETDATE()
WHERE CommentID = @CommentID
	AND InProcess = 0

IF @ProcessID <> (SELECT InProcess FROM Comment WHERE CommentID = @CommentID)
	GOTO bad

COMMIT TRANSACTION
GOTO finally

bad:
ROLLBACK TRANSACTION

finally:
SELECT ISNULL(InProcess, -1) AS ProcessID
FROM Comment
WHERE InProcess = @ProcessID
	AND CommentID = @CommentID

GO

GRANT EXECUTE ON LockProcess to public
GO