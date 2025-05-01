CREATE TABLE Cadastro (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Email NVARCHAR(255),
    Password NVARCHAR(255),
    Login NVARCHAR(255),
    EmailVerificationToken NVARCHAR(255),
    EmailVerified BIT NOT NULL
);
