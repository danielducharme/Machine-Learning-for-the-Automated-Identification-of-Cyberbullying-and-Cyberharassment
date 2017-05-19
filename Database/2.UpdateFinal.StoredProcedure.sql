IF EXISTS (
  SELECT * FROM INFORMATION_SCHEMA.ROUTINES 
   WHERE SPECIFIC_SCHEMA = N'dbo'
     AND SPECIFIC_NAME = N'UpdateFinal' 
)
BEGIN
 PRINT N'Dropping Procedure dbo.UpdateFinal'
 DROP PROCEDURE dbo.UpdateFinal
END
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO
-- UpdateFinal 

PRINT 'Creating Procedure dbo.UpdateFinal'
GO

CREATE PROCEDURE UpdateFinal
AS

SET NOCOUNT ON

--TRUNCATION HACK
SET ANSI_WARNINGS OFF

UPDATE Results
SET TimeTaken = (SELECT AVG(DATEDIFF(ms,StartAnalysis,EndAnalysis)) FROM Comment)
WHERE TimeTaken IS NULL

UPDATE Results
SET CorrectPositive = (SELECT COUNT(CommentID) FROM Comment WHERE TrainValue = 1 AND TrainValue = Bullying)
WHERE CorrectPositive IS NULL
	
UPDATE Results
SET CorrectNegative = (SELECT COUNT(CommentID) FROM Comment WHERE TrainValue = -1 AND TrainValue = Bullying)
WHERE CorrectNegative IS NULL

UPDATE Results
SET WrongPositive = (SELECT COUNT(CommentID) FROM Comment WHERE TrainValue = 1 AND TrainValue <> Bullying)
WHERE WrongPositive IS NULL

UPDATE Results
SET WrongNegative = (SELECT COUNT(CommentID) FROM Comment WHERE TrainValue = -1 AND TrainValue <> Bullying)
WHERE WrongNegative IS NULL

GO

GRANT EXECUTE ON UpdateFinal to public
GO