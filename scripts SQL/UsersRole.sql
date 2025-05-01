CREATE TABLE Users_Role (
    IdUser INT NOT NULL,
    IdRole INT NOT NULL,
    PRIMARY KEY (IdUser, IdRole),
    FOREIGN KEY (IdUser) REFERENCES Users(Id),
    FOREIGN KEY (IdRole) REFERENCES Role(Id)
);
