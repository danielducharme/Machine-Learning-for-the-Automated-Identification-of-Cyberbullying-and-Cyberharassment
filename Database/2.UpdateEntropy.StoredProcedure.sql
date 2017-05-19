IF EXISTS (
  SELECT * FROM INFORMATION_SCHEMA.ROUTINES 
   WHERE SPECIFIC_SCHEMA = N'dbo'
     AND SPECIFIC_NAME = N'UpdateEntropy' 
)
BEGIN
 PRINT N'Dropping Procedure dbo.UpdateEntropy'
 DROP PROCEDURE dbo.UpdateEntropy
END
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO
-- UpdateEntropy 3

PRINT 'Creating Procedure dbo.UpdateEntropy'
GO

CREATE PROCEDURE UpdateEntropy
(
	@MaxNGramLevel INT
)
AS

SET NOCOUNT ON

--TRUNCATION HACK
SET ANSI_WARNINGS OFF
SET NUMERIC_ROUNDABORT OFF

UPDATE NGram
SET Entropy = 0

SELECT ID, NGram, CAST(0.0 AS FLOAT) AS TotalW, CAST(0.0 AS FLOAT) AS PositiveW, CAST(0.0 AS FLOAT) AS NegativeW, CAST(0.0 AS FLOAT) AS TotalWO, CAST(0.0 AS FLOAT) AS PositiveWO, CAST(0.0 AS FLOAT) AS NegativeWO, CAST(0.0 AS FLOAT) AS EntropyP, CAST(0.0 AS FLOAT) AS EntropyW, CAST(0.0 AS FLOAT) AS EntropyWO
INTO #NGram
FROM NGram
WHERE New = 0
	AND NGramLevel <= @MaxNGramLevel

UPDATE #NGram
SET TotalW = (SELECT CAST(COUNT(Comment.CommentID) AS FLOAT)
FROM Comment
	JOIN NGramPercent
		ON Comment.CommentID = NGramPercent.CommentID
			AND NGramPercent.NGramID = ID
WHERE InUse = 1)

UPDATE #NGram
SET PositiveW = (SELECT CAST(COUNT(Comment.CommentID) AS FLOAT)
FROM Comment
	JOIN NGramPercent
		ON Comment.CommentID = NGramPercent.CommentID
			AND NGramPercent.NGramID = ID
WHERE TrainValue > 0
	AND InUse = 1)

UPDATE #NGram
SET NegativeW = (SELECT CAST(COUNT(Comment.CommentID) AS FLOAT)
FROM Comment
	JOIN NGramPercent
		ON Comment.CommentID = NGramPercent.CommentID
			AND NGramPercent.NGramID = ID
WHERE TrainValue < 0
	AND InUse = 1)

SELECT CommentID, TrainValue, ID
INTO #TempComment
FROM Comment
	JOIN #NGram
		ON 1=1
WHERE InUse = 1

MERGE #TempComment AS Target
USING NGramPercent AS Source ON (Target.ID=Source.NGramID)
WHEN NOT MATCHED BY Source THEN DELETE;

UPDATE #NGram
SET TotalWO = (SELECT CAST(COUNT(CommentID) AS FLOAT)
FROM #TempComment
WHERE #NGram.ID = #TempComment.ID)

UPDATE #NGram
SET PositiveWO = (SELECT CAST(COUNT(CommentID) AS FLOAT)
FROM #TempComment
WHERE #NGram.ID = #TempComment.ID
	AND TrainValue > 0)

UPDATE #NGram
SET NegativeWO = (SELECT CAST(COUNT(CommentID) AS FLOAT)
FROM #TempComment
WHERE #NGram.ID = #TempComment.ID
	AND TrainValue < 0)

DROP TABLE #TempComment

UPDATE #NGram
SET EntropyP = 0, EntropyW = 0, EntropyWO = 0

--Formula is the way to do it for decision trees, 1 was shown to run better per experimentation.
UPDATE #NGram
SET EntropyP = 1--(-TotalW/(TotalW + TotalWO))*LOG(TotalW/(TotalW + TotalWO),2)-(TotalWO/(TotalW + TotalWO))*LOG(TotalWO/(TotalW + TotalWO),2)
--WHERE TotalW <> 0 AND TotalWO <> 0

UPDATE #NGram
SET EntropyW = (-PositiveW/TotalW)*LOG(PositiveW/TotalW,2)-(NegativeW/TotalW)*LOG(NegativeW/TotalW,2)
WHERE TotalW <> 0 AND PositiveW <> 0 AND NegativeW <> 0

UPDATE #NGram
SET EntropyWO = (-PositiveWO/TotalWO)*LOG(PositiveWO/TotalWO,2)-(NegativeWO/TotalWO)*LOG(NegativeWO/TotalWO,2)
WHERE TotalWO <> 0 AND PositiveWO <> 0 AND NegativeWO <> 0

UPDATE NGram
SET Entropy = EntropyP-(TotalW/(TotalW + TotalWO))*EntropyW-(TotalWO/(TotalW + TotalWO))*EntropyWO
FROM #NGram
WHERE NGram.ID = #NGram.ID

UPDATE NGram
SET Entropy = -2
FROM #NGram
WHERE NGram.ID = #NGram.ID
	AND PositiveW = 0 AND NegativeW = 0
GO

GRANT EXECUTE ON UpdateEntropy to public
GO