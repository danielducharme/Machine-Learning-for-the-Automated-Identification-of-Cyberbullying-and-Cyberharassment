IF EXISTS (
	SELECT * FROM dbo.sysobjects 
	WHERE id = object_id(N'[dbo].[NGramPercent]') 
		AND OBJECTPROPERTY(id, N'IsTable') = 1)
BEGIN
	PRINT N'Dropping Table dbo.NGramPercent'
	DROP Table [dbo].[NGramPercent]
END
GO

CREATE TABLE NGramPercent (
	CommentID INT,
	NGramID INT,
	Pct FLOAT
)