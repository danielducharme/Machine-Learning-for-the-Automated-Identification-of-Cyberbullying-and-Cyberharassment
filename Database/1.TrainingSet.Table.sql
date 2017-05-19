IF EXISTS (
	SELECT * FROM dbo.sysobjects 
	WHERE id = object_id(N'[dbo].[TrainingSet]') 
		AND OBJECTPROPERTY(id, N'IsTable') = 1)
BEGIN
	PRINT N'Dropping Table dbo.TrainingSet'
	DROP Table [dbo].[TrainingSet]
END
GO

CREATE TABLE TrainingSet (
	CommentID INT PRIMARY KEY, 
	Data VARCHAR(MAX), 
	MaxNGramLevel INT,
	NGramPercent INT
)