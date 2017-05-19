IF EXISTS (
  SELECT * FROM INFORMATION_SCHEMA.ROUTINES 
   WHERE SPECIFIC_SCHEMA = N'dbo'
     AND SPECIFIC_NAME = N'GetCommentData' 
)
BEGIN
 PRINT N'Dropping Procedure dbo.GetCommentData'
 DROP PROCEDURE dbo.GetCommentData
END
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO
-- GetCommentData 2

PRINT 'Creating Procedure dbo.GetCommentData'
GO

CREATE PROCEDURE GetCommentData
(
	@CommentID INT
)
AS

SET NOCOUNT ON

--TRUNCATION HACK
SET ANSI_WARNINGS OFF

CREATE TABLE #Trainer (CommentID INT PRIMARY KEY, Data VARCHAR(MAX))

INSERT INTO #Trainer
SELECT Comment.CommentID, CAST(TrainValue AS VARCHAR) + ' 1:' + CAST(PercentCaps AS VARCHAR) AS Data
FROM Comment
WHERE Comment.CommentID = @CommentID
	AND Comment.Processed = 1

SELECT ID
INTO #NGram
FROM NGram
WHERE New = 0
	AND InUse = 1

UPDATE TSS
SET Data = Data + ' ' + NGrams
FROM #Trainer TSS
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
        From #Trainer TS
    ) [Main]
	ON TSS.CommentID = Main.CommentID

DROP TABLE #NGram

SELECT Data
FROM #Trainer

GO

GRANT EXECUTE ON GetCommentData to public
GO