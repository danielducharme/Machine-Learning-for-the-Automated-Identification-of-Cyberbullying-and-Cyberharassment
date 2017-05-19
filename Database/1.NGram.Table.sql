IF EXISTS (
	SELECT * FROM dbo.sysobjects 
	WHERE id = object_id(N'[dbo].[NGram]') 
		AND OBJECTPROPERTY(id, N'IsTable') = 1)
BEGIN
	PRINT N'Dropping Table dbo.NGram'
	DROP Table [dbo].[NGram]
END
GO

CREATE TABLE NGram (
	ID INT IDENTITY(1,1),
	NGram VARCHAR(450) NOT NULL,
	NGramLevel INT,
	New BIT,
	Entropy FLOAT,
	InUse BIT
) 
GO

ALTER TABLE NGram ADD CONSTRAINT PK_NGram PRIMARY KEY CLUSTERED
(
	NGram ASC
) WITH (IGNORE_DUP_KEY = ON)
