IF EXISTS (
  SELECT * FROM INFORMATION_SCHEMA.ROUTINES 
   WHERE SPECIFIC_SCHEMA = N'dbo'
     AND SPECIFIC_NAME = N'GetProcessedStatus' 
)
BEGIN
 PRINT N'Dropping Procedure dbo.GetProcessedStatus'
 DROP PROCEDURE dbo.GetProcessedStatus
END
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO
-- GetProcessedStatus 1

PRINT 'Creating Procedure dbo.GetProcessedStatus'
GO

CREATE PROCEDURE GetProcessedStatus
(
	@CommentID INT
)
AS

SET NOCOUNT ON

--TRUNCATION HACK
SET ANSI_WARNINGS OFF

SELECT Processed
FROM Comment (NoLock)
WHERE CommentID = @CommentID

GO

GRANT EXECUTE ON GetProcessedStatus to public
GO