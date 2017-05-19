IF EXISTS (
  SELECT * FROM INFORMATION_SCHEMA.ROUTINES 
   WHERE SPECIFIC_SCHEMA = N'dbo'
     AND SPECIFIC_NAME = N'GetComment' 
)
BEGIN
 PRINT N'Dropping Procedure dbo.GetComment'
 DROP PROCEDURE dbo.GetComment
END
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO
-- GetComment 1

PRINT 'Creating Procedure dbo.GetComment'
GO

CREATE PROCEDURE GetComment
(
	@CommentID INT
)
AS

SET NOCOUNT ON

--TRUNCATION HACK
SET ANSI_WARNINGS OFF

SELECT Comment, CleanedComment
FROM Comment
WHERE CommentID = @CommentID

GO

GRANT EXECUTE ON GetComment to public
GO