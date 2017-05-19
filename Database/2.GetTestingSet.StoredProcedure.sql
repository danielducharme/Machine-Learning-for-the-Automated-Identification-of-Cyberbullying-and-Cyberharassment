IF EXISTS (
  SELECT * FROM INFORMATION_SCHEMA.ROUTINES 
   WHERE SPECIFIC_SCHEMA = N'dbo'
     AND SPECIFIC_NAME = N'GetTestingSet' 
)
BEGIN
 PRINT N'Dropping Procedure dbo.GetTestingSet'
 DROP PROCEDURE dbo.GetTestingSet
END
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO
-- GetTestingSet

PRINT 'Creating Procedure dbo.GetTestingSet'
GO

CREATE PROCEDURE GetTestingSet
AS

SET NOCOUNT ON

--TRUNCATION HACK
SET ANSI_WARNINGS OFF
SET NUMERIC_ROUNDABORT OFF

IF (SELECT COUNT(CommentID) FROM TestingSet) > 0
BEGIN
	GOTO finish
END

INSERT INTO TestingSet (CommentID, Data)
SELECT Comment.CommentID, CAST(TrainValue AS VARCHAR) + ' 1:' + CAST(PercentCaps AS VARCHAR) AS Data
FROM Comment
WHERE Comment.Processed = 1

SELECT ID
INTO #NGram
FROM NGram
WHERE InUse = 1
ORDER BY Entropy DESC, NGramLevel, ID

UPDATE TSS
SET Data = Data + ' ' + NGrams
FROM TestingSet TSS
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
        From TestingSet TS
    ) [Main]
	ON TSS.CommentID = Main.CommentID

DROP TABLE #NGram

finish:
SELECT Data
FROM TestingSet

GO

GRANT EXECUTE ON GetTestingSet to public
GO