IF EXISTS (
  SELECT * FROM INFORMATION_SCHEMA.ROUTINES 
   WHERE SPECIFIC_SCHEMA = N'dbo'
     AND SPECIFIC_NAME = N'InsertNGramPercent' 
)
BEGIN
 PRINT N'Dropping Procedure dbo.InsertNGramPercent'
 DROP PROCEDURE dbo.InsertNGramPercent
END
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO
-- InsertNGramPercent 'Test'

PRINT 'Creating Procedure dbo.InsertNGramPercent'
GO

CREATE PROCEDURE InsertNGramPercent (@CommentID INT, @NGram VARCHAR(MAX), @Pct DECIMAL(5,4))
AS

SET NOCOUNT ON

--TRUNCATION HACK
SET ANSI_WARNINGS OFF

DECLARE @ID INT = -1

SELECT @ID = ISNULL(ID, -1)
FROM NGram
WHERE NGram = @NGram

IF @ID <> -1
BEGIN
	DELETE FROM NGramPercent 
	WHERE CommentID = @CommentID
		AND NGramID = @ID

	INSERT INTO NGramPercent (CommentID, NGramID, Pct)
	VALUES(@CommentID, @ID, @Pct)
END

GO

GRANT EXECUTE ON InsertNGramPercent to public
GO