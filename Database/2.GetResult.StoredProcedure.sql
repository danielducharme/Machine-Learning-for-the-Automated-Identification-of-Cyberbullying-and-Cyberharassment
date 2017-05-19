IF EXISTS (
  SELECT * FROM INFORMATION_SCHEMA.ROUTINES 
   WHERE SPECIFIC_SCHEMA = N'dbo'
     AND SPECIFIC_NAME = N'GetResult' 
)
BEGIN
 PRINT N'Dropping Procedure dbo.GetResult'
 DROP PROCEDURE dbo.GetResult
END
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO
-- GetResult 1

PRINT 'Creating Procedure dbo.GetResult'
GO

CREATE PROCEDURE GetResult
(
	@CommentID INT
)
AS

SET NOCOUNT ON

--TRUNCATION HACK
SET ANSI_WARNINGS OFF

SELECT Bullying
FROM Comment
WHERE CommentID = @CommentID

GO

GRANT EXECUTE ON GetResult to public
GO