-- Step 1, Create Statuses table
CREATE TABLE Statuses
(
    Id INT NOT NULL PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL
);

-- Step 2, add predefined statuses
INSERT INTO Statuses (Id, Name) VALUES 
(0, 'New'),
(1, 'InProgress'),
(2, 'Done');

-- Step 3, Update existing Tasks records, set New for all of them
UPDATE Tasks SET Status = 0 WHERE Status IS NULL;

--Step 4, make statuses in Tasks not null
ALTER TABLE Tasks ALTER COLUMN Status INT NOT NULL;

-- Step 5, add FK
ALTER TABLE Tasks WITH CHECK ADD CONSTRAINT FK_Tasks_Statuses 
FOREIGN KEY (Status) REFERENCES Statuses (Id);