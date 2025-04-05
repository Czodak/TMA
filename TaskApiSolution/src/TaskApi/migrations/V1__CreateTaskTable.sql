CREATE TABLE Tasks
(
Id int NOT NULL IDENTITY(1,1) PRIMARY KEY,
Title NVARCHAR(255),
Description NVARCHAR(MAX),
CreatorId INT NOT NULL,
CurrentlyAssignedUserId INT,
CreatedAt DATETIME2,
LastUpdatedAt DATETIME2,
Status int,
Priority INT
)