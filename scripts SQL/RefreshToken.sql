CREATE TABLE RefreshToken (
    Token NVARCHAR(255) PRIMARY KEY,
    UserId NVARCHAR(255),
    ExpiryDate DATETIME,
    IsRevoked BIT NOT NULL
);
