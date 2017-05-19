IF EXISTS (
	SELECT * FROM dbo.sysobjects 
	WHERE id = object_id(N'[dbo].[Results]') 
		AND OBJECTPROPERTY(id, N'IsTable') = 1)
BEGIN
	PRINT N'Dropping Table dbo.Results'
	DROP Table [dbo].[Results]
END
GO

CREATE TABLE Results (
	ResultID INT IDENTITY,
	TrainingSetSize INT,
	NGramLevel INT,
	NGramPercent INT,
	knnLevel INT,
	SVMType VARCHAR(255),
	SVMKernal VARCHAR(255),
	C FLOAT,
	G FLOAT,
	Nu FLOAT,
	Degree FLOAT,
	Coef0 FLOAT,
	RunNumber INT,
	Accuracy FLOAT,
	MinAccuracy FLOAT,
	MaxAccuracy FLOAT,
	TrainTimeTaken INT,
	TestTimeTaken INT,
	CorrectPositive INT,
	CorrectNegative INT,
	WrongPositive INT,
	WrongNegative INT
)