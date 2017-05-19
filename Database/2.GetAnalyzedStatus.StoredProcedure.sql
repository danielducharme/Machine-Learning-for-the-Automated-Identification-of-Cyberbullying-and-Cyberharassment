IF EXISTS (
  SELECT * FROM INFORMATION_SCHEMA.ROUTINES 
   WHERE SPECIFIC_SCHEMA = N'dbo'
     AND SPECIFIC_NAME = N'GetAnalyzedStatus' 
)
BEGIN
 PRINT N'Dropping Procedure dbo.GetAnalyzedStatus'
 DROP PROCEDURE dbo.GetAnalyzedStatus
END
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO
-- GetAnalyzedStatus 1

PRINT 'Creating Procedure dbo.GetAnalyzedStatus'
GO

CREATE PROCEDURE GetAnalyzedStatus
(
	@CommentID INT
)
AS

SET NOCOUNT ON

--TRUNCATION HACK
SET ANSI_WARNINGS OFF

SELECT Analyzed
FROM Comment (NoLock)
WHERE CommentID = @CommentID

GO

GRANT EXECUTE ON GetAnalyzedStatus to public
GO