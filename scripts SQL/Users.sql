CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Email NVARCHAR(255) NOT NULL,
    Password NVARCHAR(255) NOT NULL,
    Login NVARCHAR(255) NOT NULL,
    EmailVerificationToken NVARCHAR(255),
    EmailVerified BIT NOT NULL
);
