IF EXISTS (
  SELECT * FROM INFORMATION_SCHEMA.ROUTINES 
   WHERE SPECIFIC_SCHEMA = N'dbo'
     AND SPECIFIC_NAME = N'InsertResult' 
)
BEGIN
 PRINT N'Dropping Procedure dbo.InsertResult'
 DROP PROCEDURE dbo.InsertResult
END
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO
-- InsertResult 1, 1, 1, 1, '', '', 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1

PRINT 'Creating Procedure dbo.InsertResult'
GO

CREATE PROCEDURE InsertResult
(
	@TrainingSetSize INT,
	@NGramLevel INT,
	@NGramPercent INT,
	@knnLevel INT,
	@SVMType VARCHAR(255),
	@SVMKernal VARCHAR(255),
	@C FLOAT,
	@G FLOAT,
	@Nu FLOAT,
	@Degree FLOAT,
	@Coef0 FLOAT,
	@Accuracy FLOAT,
	@MinAccuracy FLOAT,
	@MaxAccuracy FLOAT,
	@TrainTimeTaken INT,
	@TestTimeTaken INT,
	@CorrectPositive INT,
	@CorrectNegative INT,
	@WrongPositive INT,
	@WrongNegative INT
)
AS

SET NOCOUNT ON

--TRUNCATION HACK
SET ANSI_WARNINGS OFF

CREATE TABLE #Test (Accuracy FLOAT)
INSERT INTO #Test
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

INSERT INTO Results (TrainingSetSize, NGramLevel, NGramPercent, knnLevel, SVMType, SVMKernal, C, G, Nu, Degree, Coef0, RunNumber, Accuracy, MinAccuracy, MaxAccuracy, TrainTimeTaken, TestTimeTaken, CorrectPositive, CorrectNegative, WrongPositive, WrongNegative)
SELECT @TrainingSetSize, @NGramLevel, @NGramPercent, @knnLevel, @SVMType, @SVMKernal, @C, @G, @Nu, @Degree, @Coef0, (SELECT COUNT(Accuracy) FROM #Test) + 1, @Accuracy, @MinAccuracy, @MaxAccuracy, @TrainTimeTaken, @TestTimeTaken, @CorrectPositive, @CorrectNegative, @WrongPositive, @WrongNegative
GO

GRANT EXECUTE ON InsertResult to public
GO