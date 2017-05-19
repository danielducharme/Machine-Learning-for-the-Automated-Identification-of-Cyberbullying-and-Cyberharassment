IF EXISTS (
  SELECT * FROM INFORMATION_SCHEMA.ROUTINES 
   WHERE SPECIFIC_SCHEMA = N'dbo'
     AND SPECIFIC_NAME = N'ClearNGramPercent' 
)
BEGIN
 PRINT N'Dropping Procedure dbo.ClearNGramPercent'
 DROP PROCEDURE dbo.ClearNGramPercent
END
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO
-- ClearNGramPercent 1

PRINT 'Creating Procedure dbo.ClearNGramPercent'
GO

CREATE PROCEDURE ClearNGramPercent (@CommentID INT)
AS

SET NOCOUNT ON

--TRUNCATION HACK
SET ANSI_WARNINGS OFF

DELETE FROM NGramPercent 
WHERE CommentID = @CommentID

GO

GRANT EXECUTE ON ClearNGramPercent to public
GO