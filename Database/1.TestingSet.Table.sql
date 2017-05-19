IF EXISTS (
	SELECT * FROM dbo.sysobjects 
	WHERE id = object_id(N'[dbo].[TestingSet]') 
		AND OBJECTPROPERTY(id, N'IsTable') = 1)
BEGIN
	PRINT N'Dropping Table dbo.TestingSet'
	DROP Table [dbo].[TestingSet]
END
GO

CREATE TABLE TestingSet (
	CommentID INT PRIMARY KEY, 
	Data VARCHAR(MAX)
)