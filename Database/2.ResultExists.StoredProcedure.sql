IF EXISTS (
  SELECT * FROM INFORMATION_SCHEMA.ROUTINES 
   WHERE SPECIFIC_SCHEMA = N'dbo'
     AND SPECIFIC_NAME = N'ResultExists' 
)
BEGIN
 PRINT N'Dropping Procedure dbo.ResultExists'
 DROP PROCEDURE dbo.ResultExists
END
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO
-- ResultExists 50, 1, 10, 10, 'C_SVC', -5, 1, 'POLY', 14, 1, 1, 10

PRINT 'Creating Procedure dbo.ResultExists'
GO

CREATE PROCEDURE ResultExists
(
	@TrainingSetSize INT,
	@NGramLevel INT,
	@NGramPercent INT,
	@knnLevel INT,
	@SVMType VARCHAR(255),
	@C FLOAT,
	@Nu FLOAT,
	@SVMKernal VARCHAR(255),
	@G FLOAT,
	@Degree FLOAT,
	@Coef0 FLOAT,
	@MaxRuns INT
)
AS

SET NOCOUNT ON

--TRUNCATION HACK
SET ANSI_WARNINGS OFF

SELECT Accuracy
FROM Results (nolock)
WHERE TrainingSetSize = @TrainingSetSize
	AND NGramLevel = @NGramLevel
	AND NGramPercent = @NGramPercent
	AND knnLevel = @knnLevel
	AND SVMType = @SVMType
	AND SVMKernal = @SVMKernal
	AND ISNULL(C, -99999) = ISNULL(@C, -99999)
	AND ISNULL(Nu, -99999) = ISNULL(@Nu, -99999)
	AND ISNULL(Degree, -99999) = ISNULL(@Degree, -99999)
	AND ISNULL(Coef0, -99999) = ISNULL(@Coef0, -99999)
	AND RunNumber >= @MaxRuns

GO

GRANT EXECUTE ON ResultExists to public
GO