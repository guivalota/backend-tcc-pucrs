CREATE TABLE UsersRecovery (
    Id INT PRIMARY KEY IDENTITY(1,1),
    IdUser INT NOT NULL,
    VerificationToken NVARCHAR(255) NOT NULL,
    IsUsed BIT NOT NULL,
    ExpiryDate DATETIME NOT NULL,
    FOREIGN KEY (IdUser) REFERENCES Users(Id)
);
