IF EXISTS (
  SELECT * FROM INFORMATION_SCHEMA.ROUTINES 
   WHERE SPECIFIC_SCHEMA = N'dbo'
     AND SPECIFIC_NAME = N'InsertNGram' 
)
BEGIN
 PRINT N'Dropping Procedure dbo.InsertNGram'
 DROP PROCEDURE dbo.InsertNGram
END
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO
-- InsertNGram 'Test'

PRINT 'Creating Procedure dbo.InsertNGram'
GO

CREATE PROCEDURE InsertNGram (@NGram VARCHAR(MAX))
AS

SET NOCOUNT ON

--TRUNCATION HACK
SET ANSI_WARNINGS OFF

DECLARE @ID INT = -1

SELECT @ID = ISNULL(ID, -1)
FROM NGram
WHERE NGram = @NGram

IF @ID = -1
BEGIN
	INSERT INTO NGram (NGram, NGramLevel, New)
	VALUES(@NGram, LEN(@NGram) - LEN(REPLACE(@NGram, '|', '')) + 1, 1)
END

GO

GRANT EXECUTE ON InsertNGram to public
GO