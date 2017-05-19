IF EXISTS (
  SELECT * FROM INFORMATION_SCHEMA.ROUTINES 
   WHERE SPECIFIC_SCHEMA = N'dbo'
     AND SPECIFIC_NAME = N'UpdateStats' 
)
BEGIN
 PRINT N'Dropping Procedure dbo.UpdateStats'
 DROP PROCEDURE dbo.UpdateStats
END
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO
-- UpdateStats 1, 50.0

PRINT 'Creating Procedure dbo.UpdateStats'
GO

CREATE PROCEDURE UpdateStats
(
	@CommentID INT,
	@PercentCaps DECIMAL(5,4)
)
AS

SET NOCOUNT ON

--TRUNCATION HACK
SET ANSI_WARNINGS OFF

UPDATE Comment
SET Processed = 1, PercentCaps = @PercentCaps, EndProcess = GETDATE()
WHERE CommentID = @CommentID

GO

GRANT EXECUTE ON UpdateStats to public
GO