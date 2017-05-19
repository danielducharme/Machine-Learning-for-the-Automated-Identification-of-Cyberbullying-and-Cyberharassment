IF EXISTS (
  SELECT * FROM INFORMATION_SCHEMA.ROUTINES 
   WHERE SPECIFIC_SCHEMA = N'dbo'
     AND SPECIFIC_NAME = N'InsertComment' 
)
BEGIN
 PRINT N'Dropping Procedure dbo.InsertComment'
 DROP PROCEDURE dbo.InsertComment
END
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO
-- InsertComment 1, 'Test', 1, 'Test'

PRINT 'Creating Procedure dbo.InsertComment'
GO

CREATE PROCEDURE InsertComment
(
	@Id VARCHAR(MAX),
	@Comment VARCHAR(MAX),
	@likeCount INT,
	@Source VARCHAR(255)
)
AS

SET NOCOUNT ON

--TRUNCATION HACK
SET ANSI_WARNINGS OFF

IF @Id NOT IN (SELECT Id FROM Comments)
BEGIN
	INSERT INTO Comments (Id, Comment, likeCount, [Source], DanielClassification, TwitterClassification)
	VALUES (@Id, @Comment, @likeCount, @Source, 0, 0)
END
GO

GRANT EXECUTE ON InsertComment to public
GO