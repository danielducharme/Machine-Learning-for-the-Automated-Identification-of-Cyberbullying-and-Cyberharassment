IF EXISTS (
	SELECT * FROM dbo.sysobjects 
	WHERE id = object_id(N'[dbo].[Comment]') 
		AND OBJECTPROPERTY(id, N'IsTable') = 1)
BEGIN
	PRINT N'Dropping Table dbo.Comment'
	DROP Table [dbo].[Comment]
END
GO

CREATE TABLE Comment (
	CommentID INT IDENTITY(1, 1) PRIMARY KEY,
	Comment VARCHAR(MAX),
	CleanedComment VARCHAR(MAX),
	PercentCaps DECIMAL(5, 4),
	TrainValue INT,
	StartProcess DATETIME,
	InProcess VARCHAR(MAX),
	EndProcess DATETIME,
	Processed BIT,
	InUse BIT,
	StartAnalysis DATETIME,
	InAnalysis VARCHAR(MAX),
	EndAnalysis DATETIME,
	Analyzed BIT,
	Bullying INT
)