IF EXISTS (
  SELECT * FROM INFORMATION_SCHEMA.ROUTINES 
   WHERE SPECIFIC_SCHEMA = N'dbo'
     AND SPECIFIC_NAME = N'InsertComment' 
)
BEGIN
 PRINT N'Dropping Procedure dbo.InsertComment'
 DROP PROCEDURE dbo.InsertComment
END
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO
-- InsertComment 'This is Test to see how well it handles F&$#ING   Data'

PRINT 'Creating Procedure dbo.InsertComment'
GO
CREATE PROCEDURE [dbo].[InsertComment]
(
	@Comment VARCHAR(MAX),
	@TrainValue INT = NULL
)
AS

SET NOCOUNT ON

--TRUNCATION HACK
SET ANSI_WARNINGS OFF

DECLARE @CleanComment VARCHAR(MAX)

SET @CleanComment = '|' + LOWER(@Comment) + '|'

SET @CleanComment = REPLACE(@CleanComment, '.', '|')
SET @CleanComment = REPLACE(@CleanComment, ' ', '|')
SET @CleanComment = REPLACE(@CleanComment, '!', '|')
SET @CleanComment = REPLACE(@CleanComment, '*', '|')
SET @CleanComment = REPLACE(@CleanComment, '>', '|')
SET @CleanComment = REPLACE(@CleanComment, '<', '|')
SET @CleanComment = REPLACE(@CleanComment, '%', '|')
SET @CleanComment = REPLACE(@CleanComment, '(', '|')
SET @CleanComment = REPLACE(@CleanComment, ')', '|')
SET @CleanComment = REPLACE(@CleanComment, ':', '|')
SET @CleanComment = REPLACE(@CleanComment, '&', '|')
SET @CleanComment = REPLACE(@CleanComment, '@', '|')
SET @CleanComment = REPLACE(@CleanComment, '#', '|')
SET @CleanComment = REPLACE(@CleanComment, '$', '|')
SET @CleanComment = REPLACE(@CleanComment, '^', '|')
SET @CleanComment = REPLACE(@CleanComment, '`', '|')
SET @CleanComment = REPLACE(@CleanComment, '~', '|')
SET @CleanComment = REPLACE(@CleanComment, ';', '|')
SET @CleanComment = REPLACE(@CleanComment, '[', '|')
SET @CleanComment = REPLACE(@CleanComment, ']', '|')
SET @CleanComment = REPLACE(@CleanComment, '{', '|')
SET @CleanComment = REPLACE(@CleanComment, '}', '|')
SET @CleanComment = REPLACE(@CleanComment, '\', '|')
SET @CleanComment = REPLACE(@CleanComment, '-', '|')
SET @CleanComment = REPLACE(@CleanComment, '_', '|')
SET @CleanComment = REPLACE(@CleanComment, '=', '|')
SET @CleanComment = REPLACE(@CleanComment, '+', '|')
SET @CleanComment = REPLACE(@CleanComment, ',', '|')
SET @CleanComment = REPLACE(@CleanComment, '.', '|')
SET @CleanComment = REPLACE(@CleanComment, '?', '|')
SET @CleanComment = REPLACE(@CleanComment, '/', '|')
SET @CleanComment = REPLACE(@CleanComment, '"', '|')
SET @CleanComment = REPLACE(@CleanComment, '''', '|')
SET @CleanComment = REPLACE(@CleanComment, CHAR(10), '|')
SET @CleanComment = REPLACE(@CleanComment, CHAR(13), '|')

WHILE @CleanComment IN (SELECT @CleanComment WHERE CHARINDEX('||', @CLeanComment) <> 0)
BEGIN 
	SET @CleanComment = REPLACE(@CleanComment, '||', '|')
END

IF @Comment NOT IN (SELECT Comment FROM Comment)
BEGIN
	INSERT INTO Comment (Comment, CleanedComment, TrainValue, InProcess, Processed, InUse, InAnalysis, Analyzed, Bullying)
	VALUES (@Comment, @CleanComment, ISNULL(@TrainValue, 0), 0, 0, 0, 0, 0, 0)
END

SELECT CommentID
FROM Comment
WHERE Comment = @Comment


GO

GRANT EXECUTE ON InsertComment to public
GO
