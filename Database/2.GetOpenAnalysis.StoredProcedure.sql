IF EXISTS (
  SELECT * FROM INFORMATION_SCHEMA.ROUTINES 
   WHERE SPECIFIC_SCHEMA = N'dbo'
     AND SPECIFIC_NAME = N'GetOpenAnalysis' 
)
BEGIN
 PRINT N'Dropping Procedure dbo.GetOpenAnalysis'
 DROP PROCEDURE dbo.GetOpenAnalysis
END
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO
-- GetOpenAnalysis

PRINT 'Creating Procedure dbo.GetOpenAnalysis'
GO

CREATE PROCEDURE GetOpenAnalysis
AS

SET NOCOUNT ON

--TRUNCATION HACK
SET ANSI_WARNINGS OFF

SELECT CommentID
FROM Comment (nolock)
WHERE Analyzed = 0
	AND InAnalysis = '0'
	AND Processed = 1

GO

GRANT EXECUTE ON GetOpenAnalysis to public
GO