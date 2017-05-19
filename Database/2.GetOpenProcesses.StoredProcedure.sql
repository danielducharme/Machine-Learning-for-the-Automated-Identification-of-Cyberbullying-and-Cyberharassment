IF EXISTS (
  SELECT * FROM INFORMATION_SCHEMA.ROUTINES 
   WHERE SPECIFIC_SCHEMA = N'dbo'
     AND SPECIFIC_NAME = N'GetOpenProcesses' 
)
BEGIN
 PRINT N'Dropping Procedure dbo.GetOpenProcesses'
 DROP PROCEDURE dbo.GetOpenProcesses
END
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO
-- GetOpenProcesses

PRINT 'Creating Procedure dbo.GetOpenProcesses'
GO

CREATE PROCEDURE GetOpenProcesses
AS

SET NOCOUNT ON

--TRUNCATION HACK
SET ANSI_WARNINGS OFF

SELECT CommentID
FROM Comment (nolock)
WHERE Processed = 0
	AND InProcess = '0'

GO

GRANT EXECUTE ON GetOpenProcesses to public
GO