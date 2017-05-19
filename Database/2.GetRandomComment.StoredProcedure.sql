IF EXISTS (
  SELECT * FROM INFORMATION_SCHEMA.ROUTINES 
   WHERE SPECIFIC_SCHEMA = N'dbo'
     AND SPECIFIC_NAME = N'GetRandomComment' 
)
BEGIN
 PRINT N'Dropping Procedure dbo.GetRandomComment'
 DROP PROCEDURE dbo.GetRandomComment
END
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO
-- GetRandomComment 'Twitter'

PRINT 'Creating Procedure dbo.GetRandomComment'
GO

CREATE PROCEDURE GetRandomComment
(
	@Name VARCHAR(255)
)
AS

SET NOCOUNT ON

--TRUNCATION HACK
SET ANSI_WARNINGS OFF

DECLARE @Sql VARCHAR(255)

IF @Name <> 'Twitter'
BEGIN
SET @Sql = 'SELECT TOP 1 RecordID, Comment
	FROM Comments
	WHERE Source = ''YouTube'' 
		AND ' + @Name + 'Classification = 0 '
		+ CASE WHEN @Name <> 'Daniel' THEN 'AND DanielClassification <> 0' ELSE '' END + '
	ORDER BY NEWID()'
END
ELSE
BEGIN
SET @Sql = 'SELECT TOP 1 RecordID, Comment
	FROM Comments
	WHERE Source = ''Twitter''
		AND DanielClassification = 0 
	ORDER BY NEWID()'
END

EXEC (@Sql)
GO

GRANT EXECUTE ON GetRandomComment to public
GO