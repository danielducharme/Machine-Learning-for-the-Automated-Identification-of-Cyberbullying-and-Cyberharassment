IF EXISTS (
	SELECT * FROM dbo.sysobjects 
	WHERE id = object_id(N'[dbo].[Comments]') 
		AND OBJECTPROPERTY(id, N'IsTable') = 1)
BEGIN
	PRINT N'Dropping Table dbo.Comments'
	DROP Table [dbo].[Comments]
END
GO

CREATE TABLE Comments (
	[RecordID] [int] IDENTITY(1,1),
	[Id] [varchar](max) NULL,
	[Comment] [varchar](max) NULL,
	[likeCount] [int] NULL,
	[Source] [varchar](255) NULL,
	[DanielClassification] [smallint] NULL,
	[ToSClassification] [smallint] NULL
)