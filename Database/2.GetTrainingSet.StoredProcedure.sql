IF EXISTS (
  SELECT * FROM INFORMATION_SCHEMA.ROUTINES 
   WHERE SPECIFIC_SCHEMA = N'dbo'
     AND SPECIFIC_NAME = N'GetTrainingSet' 
)
BEGIN
 PRINT N'Dropping Procedure dbo.GetTrainingSet'
 DROP PROCEDURE dbo.GetTrainingSet
END
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO
-- GetTrainingSet 3, 7, 175

PRINT 'Creating Procedure dbo.GetTrainingSet'
GO

CREATE PROCEDURE GetTrainingSet
(
	@MaxNGramLevel INT,
	@NGramPercent INT,
	@NumberOfComments INT
)
AS

SET NOCOUNT ON

--TRUNCATION HACK
SET ANSI_WARNINGS OFF
SET NUMERIC_ROUNDABORT OFF

CREATE TABLE #TempComment(
	CommentID INT,
	Rating INT
)

--THIS IS FOR HIGH THROUGHPUT OPERATION WITH A FIXED TRAINING SET
/*IF (SELECT COUNT(CommentID) FROM TrainingSet WHERE MaxNGramLevel = @MaxNGramLevel AND NGramPercent = @NGramPercent) >= (@NumberOfComments * 2)
BEGIN
	GOTO finish
END*/

--THIS IS FOR THE ADDING METHOD
/*IF (SELECT COUNT(CommentID) FROM TrainingSet WHERE MaxNGramLevel = @MaxNGramLevel AND NGramPercent = @NGramPercent) >= (@NumberOfComments * 2)
BEGIN
	INSERT INTO #TempComment
	SELECT CommentID, SUBSTRING(Data, 1, CHARINDEX('1:', Data)-1)
	FROM TrainingSet

	TRUNCATE TABLE TrainingSet

	GOTO skipper
END*/

TRUNCATE TABLE TrainingSet
TRUNCATE TABLE TestingSet

UPDATE Comment
SET InUse = 0

--First we enter the proper number of comments asked for
INSERT INTO #TempComment
SELECT TOP (@NumberOfComments) CommentID, -1
FROM Comment
WHERE Comment.TrainValue < 0
	AND Comment.Processed = 1
ORDER BY ABS(TrainValue) DESC, NEWID()

INSERT INTO #TempComment
SELECT TOP (@NumberOfComments) CommentID, 1
FROM Comment
WHERE Comment.TrainValue > 0
	AND Comment.Processed = 1
ORDER BY ABS(TrainValue) DESC, NEWID()

--THIS IS FOR THE ADDING METHOD
/*skipper:

--This will pull in the extra comments we are forcing in
INSERT INTO #TempComment
SELECT CommentID, TrainValue/2
FROM Comment
WHERE Comment.TrainValue = -2
	AND Comment.Processed = 1
	AND CommentID NOT IN (SELECT CommentID FROM #TempComment)

--We use this to balance the training set
DECLARE @Counter INT
SELECT @Counter = SUM(Rating)
FROM #TempComment

INSERT INTO #TempComment
SELECT TOP(ABS(@Counter)) CommentID, TrainValue/2
FROM Comment
WHERE Comment.TrainValue = 2
	AND Comment.Processed = 1
	AND CommentID NOT IN (SELECT CommentID FROM #TempComment)
ORDER BY NEWID()*/

UPDATE Comment
SET InUse = 1
WHERE CommentID IN (SELECT CommentID FROM #TempComment)

INSERT INTO TrainingSet (CommentID, Data, MaxNGramLevel, NGramPercent)
SELECT Comment.CommentID, CAST(CASE WHEN TrainValue > 0 THEN 1 ELSE -1 END AS VARCHAR) + ' 1:' + CAST(PercentCaps AS VARCHAR) AS Data, @MaxNGramLevel, @NGramPercent
FROM #TempComment
	JOIN Comment
		ON #TempComment.CommentID = Comment.CommentID
WHERE Comment.InUse = 1

DROP TABLE #TempComment

UPDATE NGram
SET New = 0, InUse = 0

EXEC UpdateEntropy @MaxNGramLevel

SELECT TOP (@NGramPercent) PERCENT ID
INTO #NGram
FROM NGram
WHERE New = 0
	AND Entropy <> -2
	AND NGramLevel <= @MaxNGramLevel
ORDER BY Entropy DESC, NGramLevel, ID

UPDATE NGram
SET InUse = 1
WHERE ID IN (SELECT ID FROM #NGram)

UPDATE TSS
SET Data = Data + ' ' + NGrams
FROM TrainingSet TSS
	JOIN (
        Select distinct TS.CommentID, 
            (
                Select CAST(NG.ID + 1 AS VARCHAR) + ':' + CAST(CAST(ISNULL(NGP.Pct, 0.0) AS DECIMAL(5,4)) AS VARCHAR) + ' ' AS [text()]
                From #NGram NG
					LEFT JOIN NGramPercent NGP
						ON NGP.NGramID = NG.ID
							AND NGP.CommentID = TS.CommentID
                ORDER BY NG.ID
                For XML PATH ('')
            ) NGrams
        From TrainingSet TS
    ) [Main]
	ON TSS.CommentID = Main.CommentID

DROP TABLE #NGram

finish:
SELECT Data
FROM TrainingSet

GO

GRANT EXECUTE ON GetTrainingSet to public
GO