CREATE TABLE Pessoa (
    Id INT PRIMARY KEY IDENTITY(1,1),
    IdUser INT,
    Nome NVARCHAR(255),
    Sobrenome NVARCHAR(255),
    Documento NVARCHAR(50),
    FOREIGN KEY (IdUser) REFERENCES Users(Id)
);
