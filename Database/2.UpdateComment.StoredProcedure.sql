IF EXISTS (
  SELECT * FROM INFORMATION_SCHEMA.ROUTINES 
   WHERE SPECIFIC_SCHEMA = N'dbo'
     AND SPECIFIC_NAME = N'UpdateComment' 
)
BEGIN
 PRINT N'Dropping Procedure dbo.UpdateComment'
 DROP PROCEDURE dbo.UpdateComment
END
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO
-- UpdateComment 'Daniel', 1, 1

PRINT 'Creating Procedure dbo.UpdateComment'
GO

CREATE PROCEDURE UpdateComment
(
	@Name VARCHAR(255),
	@RecordID INT,
	@Result INT
)
AS

SET NOCOUNT ON

--TRUNCATION HACK
SET ANSI_WARNINGS OFF

DECLARE @Sql VARCHAR(255)

SET @Sql = 'UPDATE Comments
	SET ' + @Name + 'Classification = ' + CAST(@Result AS VARCHAR) + '
	WHERE RecordID = ' + CAST(@RecordID AS VARCHAR)

EXEC (@Sql)
GO

GRANT EXECUTE ON UpdateComment to public
GO