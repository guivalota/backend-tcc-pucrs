using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Backend.TCC.PUCRS.Model;
using Backend.TCC.PUCRS.Services.Interfaces;
using Backend.TCC.PUCRS.Utils;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace Backend.TCC.PUCRS.Services;
public class AuthService
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly IConfiguration _configuration;
        private readonly ILogGeral _logGeral;
        private string SecretKey;
        private const int ExpirationMinutes = 5;
        private readonly EmailService _emailService;
        private readonly Utilidades _utils;

        private const string host = "http://localhost:5000/api/auth";

        public AuthService(DatabaseHelper dbHelper, IConfiguration configuration, EmailService emailService, ILogGeral logGeral, Utilidades utils)
        {
            _dbHelper = dbHelper;
            _configuration = configuration;
            _emailService = emailService;
            _logGeral = logGeral;
            SecretKey = _configuration["JwtSettings:SecretKey"] ?? "";
            _utils = utils;
        }

        public async Task<Users> RegisterAsync(Users user)
        {
            // Verificar se o email ou login já existem
            var checkQuery = "SELECT COUNT(*) FROM Users WHERE Email = @Email OR Login = @Login";
            var checkParams = new[]
            {
                new SqlParameter("@Email", user.Email),
                new SqlParameter("@Login", user.Login)
            };
            var result = await _dbHelper.ExecuteReaderAsync(checkQuery, checkParams);
            if (result.Read() && (int)result[0] > 0)
            {
                throw new Exception("Email ou Login já existe.");
            }
            // Criptografar a senha antes de armazenar
            user.Password = _utils.GerarHash(user.Password);
            string emailVerificationToken = Guid.NewGuid().ToString();
            user.EmailVerificationToken = emailVerificationToken;
            user.EmailVerified = false;
            // Inserir o novo usuário no banco
            var insertQuery = "INSERT INTO Users (Email, Password, Login, EmailVerificationToken, EmailVerified) VALUES (@Email, @Password, @Login, @EmailVerificationToken, @EmailVerified)";
            var insertParams = new[]
            {
                new SqlParameter("@Email", user.Email),
                new SqlParameter("@Password", user.Password),
                new SqlParameter("@Login", user.Login),
                new SqlParameter("@EmailVerificationToken", user.EmailVerificationToken),
                new SqlParameter("@EmailVerified", user.EmailVerified)
            };


            await _dbHelper.ExecuteCommandAsync(insertQuery, insertParams);
            // Pegar o id do usuário que acabou de ser inserido
            Users? newUser = await GetUserByUserNameAsync(user.Login);
            // Inserir a primeira role do usuário com User
            var insertQueryRole = " INSERT INTO Users_Role (IdUser, IdRole) VALUES(@IdUser, @IdRole)";
            var insertParamsRole = new[]
            {
                new SqlParameter("@IdUser", newUser!.Id),
                new SqlParameter("@IdRole", 2)
            };
            await _dbHelper.ExecuteCommandAsync(insertQueryRole, insertParamsRole);
            //Enviar o email para o usuário
            //_emailService.SendEmailAsync(user.Email, "Confirmação de Cadastro", $"Por favor, confirme seu email clicando no link: {host}/verify-email?token={emailVerificationToken}");
            _emailService.SendEmail2(user.Email, "Confirmação de Cadastro", $"Por favor, confirme seu email clicando no link: {host}/verify-email?token={emailVerificationToken}");
            return user;
        }

        public async Task<Users> RegisterNewUserAsync(Users user)
        {
            user.Password = _utils.GeneratePassword(10);
            string storedPassword = user.Password;
            var retorno = await RegisterAsync(user);
            _logGeral.AddLogGeral(_utils.CreateLog($"Usuario {user.Login} cadastrado, senha enviada para {user.Email}", "Users", 0));
            _emailService.SendEmailAsync(user.Email, "Bem vindo ao sistema", $"Um usuário foi cadastrado no sistema para você com o login: {user.Login} e senha: {storedPassword} .");
            return retorno;
        }

        public async Task<string> LoginAsync(string login, string password)
        {
            var query = "SELECT Id, Email, Password, Login, EmailVerified FROM Users WHERE Login = @Login";
            var parameters = new[]
            {
                new SqlParameter("@Login", login)
            };
            using var reader = await _dbHelper.ExecuteReaderAsync(query, parameters);
            if (!reader.Read())
            {
                _logGeral.AddLogGeral(_utils.CreateLog($"Tentativa de Login do usuario {login} - Usuário não encontrado", "Users", 0));
                throw new UnauthorizedAccessException("Credenciais inválidas.");
            }
            var storedPassword = reader["Password"].ToString();
            if (!_utils.VerificarSenha(password, storedPassword!))
            {
                _logGeral.AddLogGeral(_utils.CreateLog($"Tentativa de Login do usuario {login} - Senha Incorreta", "Users", 0));
                throw new UnauthorizedAccessException("Credenciais inválidas.");
            }
            var user = new Users
            {
                Id = (int)reader["Id"],
                Login = reader["Login"].ToString()!,
                Email = reader["Email"].ToString()!,
                EmailVerified = (bool)reader["EmailVerified"]
            };
            if (!user.EmailVerified)
            {
                _logGeral.AddLogGeral(_utils.CreateLog($"Tentativa de Login do usuario {login} - Usuário ainda não validado", "Users", 0));
                throw new UnauthorizedAccessException("E-mail não validado.");
            }
            if (await BloqueVerify(user.Id))
            {
                _logGeral.AddLogGeral(_utils.CreateLog($"Tentativa de Login do usuario {login} - Usuário está bloqueado.", "Users", 0));
                throw new UnauthorizedAccessException("Usuário bloqueado.");
            }
            _logGeral.AddLogGeral(_utils.CreateLog($"Login do usuario {login}", "Users", 0));
            return await GenerateJwtToken(user);
        }

        private async Task<bool> BloqueVerify(int idUsuario)
        {
            var query = "SELECT 1 FROM UsersBloqued WHERE IdUser = @IdUser and IsBloqued = @IsBloqued";
            var parameters = new[]
            {
                new SqlParameter("@IdUser", idUsuario),
                new SqlParameter("@IsBloqued", true)
            };

            using var reader = await _dbHelper.ExecuteReaderAsync(query, parameters);
            if (reader.Read())
            {
                return true;
            }
            return false;
        }

        public async Task<string> GenerateRefreshToken(Users user)
        {
            var token = Guid.NewGuid().ToString();
            var expiryDate = DateTime.UtcNow.AddDays(7);
            var insertQuery = "INSERT INTO RefreshToken (Token, UserId, ExpiryDate, IsRevoked) VALUES (@token, @UserId, @ExpiryDate, @IsRevoked)";
            var insertParams = new[]
            {
                new SqlParameter("@Token", token),
                new SqlParameter("@userId", user.Login),
                new SqlParameter("@ExpiryDate", expiryDate),
                new SqlParameter("@IsRevoked", false)
            };
            await _dbHelper.ExecuteCommandAsync(insertQuery, insertParams);
            return token;
        }

        public async void LogoutAsync(string token, Users user)
        {
            var updateQuery = "UPDATE RefreshToken set IsRevoked = @IsRevoked WHERE Token = @Token";
            var insertParams = new[]
            {
                new SqlParameter("@Token", token),
                new SqlParameter("@IsRevoked", true)
            };
            await _dbHelper.ExecuteCommandAsync(updateQuery, insertParams);
            _logGeral.AddLogGeral(_utils.CreateLog($"Usuario {user.Login} deslogado do sistema.", "RefreshToken", 0));
        }

        public async Task<RefreshTokenModel> GetRefreshTokenFromDatabase(string token)
        {
            try
            {
                var query = "SELECT Token, UserId, ExpiryDate, IsRevoked from Refreshtoken where Token = @Token";
                var parameters = new[]
                {
                new SqlParameter("@Token", token)
            };
                using var reader = await _dbHelper.ExecuteReaderAsync(query, parameters);
                if (reader.Read())
                {
                    return new RefreshTokenModel
                    {
                        Token = reader["Token"].ToString()!,
                        UserId = reader["UserId"].ToString(),
                        ExpiryDate = (DateTime)reader["ExpiryDate"],
                        IsRevoked = (bool)reader["IsRevoked"]
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro:{ex.Message}");
            }
            throw new UnauthorizedAccessException("Token não localizado");
        }

        public async Task<Users?> GetUserByUserNameAsync(string login)
        {
            var query = "SELECT Id, Email, Password, EmailVerified FROM Users WHERE Login = @Login";
            var parameters = new[]
            {
                new SqlParameter("@Login", login)
            };

            using var reader = await _dbHelper.ExecuteReaderAsync(query, parameters);
            if (reader.Read())
            {
                return new Users
                {
                    Id = (int)reader["Id"],
                    Login = login,
                    Email = reader["Email"].ToString()!,
                    EmailVerified = (bool)reader["EmailVerified"]
                };
            }
            _logGeral.AddLogGeral(_utils.CreateLog($"Usuario {login} não localizado.", "Users", 0));
            return null;
        }

        public async Task<Users?> GetUserById(int id)
        {
            var query = "SELECT Id, Login, Email, Password, EmailVerified FROM Users WHERE Id = @Id";
            var parameters = new[]
            {
                new SqlParameter("@Id", id)
            };

            using var reader = await _dbHelper.ExecuteReaderAsync(query, parameters);
            if (reader.Read())
            {
                return new Users
                {
                    Id = (int)reader["Id"],
                    Login = reader["Login"].ToString()!,
                    Email = reader["Email"].ToString()!,
                    EmailVerified = (bool)reader["EmailVerified"]
                };
            }
            _logGeral.AddLogGeral(_utils.CreateLog($"Usuario com id {id} não localizado.", "Users", 0));
            return null;
        }
        public async Task<Users?> GetUserByEmail(string email)
        {
            var query = "SELECT Id, Login, Email, Password, EmailVerified FROM Users WHERE Email = @Email";
            var parameters = new[]
            {
                new SqlParameter("@Email", email)
            };

            using var reader = await _dbHelper.ExecuteReaderAsync(query, parameters);
            if (reader.Read())
            {
                return new Users
                {
                    Id = (int)reader["Id"],
                    Login = reader["Login"].ToString()!,
                    Email = reader["Email"].ToString()!,
                    EmailVerified = (bool)reader["EmailVerified"]
                };
            }
            _logGeral.AddLogGeral(_utils.CreateLog($"Usuario com email {email} não localizado.", "Users", 0));
            return null;
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            var query = "SELECT COUNT(*) FROM Users WHERE EmailVerificationToken = @Token AND EmailVerified = 0";
            var parameters = new[] { new SqlParameter("@Token", token) };

            var result = await _dbHelper.ExecuteReaderAsync(query, parameters);
            if (result.Read() && (int)result[0] > 0)
            {
                var updateQuery = "UPDATE Users SET EmailVerified = 1 WHERE EmailVerificationToken = @Token";
                var updateParameters = new[] { new SqlParameter("@Token", token) };
                await _dbHelper.ExecuteCommandAsync(updateQuery, updateParameters);
                return true;
            }

            return false;
        }

        private async Task<List<Role>> GetUserRole(Users users)
        {
            var retorno = new List<Role>();
            var query = " SELECT ur.IdRole, r.Descricao from Users_Role ur inner join Role r on(r.Id = ur.IdRole) where IdUser = @IdUser";
            var parameters = new[] { new SqlParameter("@IdUser", users.Id) };

            using var reader = await _dbHelper.ExecuteReaderAsync(query, parameters);
            while (reader.Read())
            {
                retorno.Add(new Role
                {
                    Id = reader.GetInt32(0),
                    Descricao = reader.GetString(1)
                });
            }
            return retorno;
        }

        public async Task<string> GenerateJwtToken(Users user)
        {
            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email), // Identificador do usuário
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Login), // Nome do usuário
            new Claim(JwtRegisteredClaimNames.Name, user.Login), // Nome do usuário
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Identificador único do token
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64) // Timestamp do token
        };
            var roles = await GetUserRole(user);
            //Adicionar roles
            foreach (var role in roles)
            {
                claims.Add(new Claim("roles", role.Descricao));
            }

            // 2. Geração da chave secreta
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 3. Configuração do token
            var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"] ?? "aplicacao", // Emissor do token
                audience: _configuration["JwtSettings:Audience"] ?? "cliente", // Destinatário do token
                claims: claims, // Informações contidas no token
                expires: DateTime.UtcNow.AddMinutes(ExpirationMinutes), // Tempo de expiração
                signingCredentials: credentials // Credenciais de assinatura
            );

            // 4. Geração do token
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(tokenDescriptor); // Retorna o token como string
        }

        public async Task<List<Users>> ListarUsuarios(string login)
        {
            var retorno = new List<Users>();
            var query = " SELECT Email, Login, EmailVerified From Users where Login <> @Login";
            var parameters = new[] { new SqlParameter("@Login", login) };

            using var reader = await _dbHelper.ExecuteReaderAsync(query, parameters);
            while (reader.Read())
            {
                retorno.Add(new Users
                {
                    Email = reader.GetString(0),
                    Login = reader.GetString(1),
                    EmailVerified = reader.GetBoolean(2)
                });
            }
            return retorno;
        }

        public async void AlterarSenha(Users user, string login = "")
        {
            try
            {
                string password;
                if (login == "")
                {
                    user.Password = _utils.GeneratePassword(10);
                    password = user.Password;
                }

                user.Password = _utils.GerarHash(user.Password);
                var insertQuery = "UPDATE Users set Password = @Password where Login = @Login";
                var insertParams = new[]
                {
                new SqlParameter("@Password", user.Password),
                new SqlParameter("@Login", user.Login)
            };

                await _dbHelper.ExecuteCommandAsync(insertQuery, insertParams);
                if (login == "")
                {
                    _logGeral.AddLogGeral(_utils.CreateLog($"Usuario {user.Login} alterou a senha", "Users", 0));
                }
                else
                {
                    _emailService.SendEmailAsync(user.Email, "Alteração de senha", $"Um administrador do sistema requisitou a alteração da sua senha. Sua nova senha é : {user.Password} . Acesse o sistema para alterar sua senha caso deseje.");
                    _logGeral.AddLogGeral(_utils.CreateLog($"Senha do usuario {user.Login} foi resetada pelo usuario {login}", "Users", 0));
                }
            }
            catch (Exception ex)
            {
                _logGeral.AddLogGeral(_utils.CreateLog($"Erro ao alterar a senha do usuario {user.Login}.Erro: {ex.Message}", "Users", 0));
                throw new Exception(ex.Message);
            }
        }

        public async void RequestRestarUser(string login)
        {
            var user = await GetUserByUserNameAsync(login);

            var token = Guid.NewGuid().ToString();
            var expiryDate = DateTime.UtcNow.AddDays(1);
            var insertQuery = "INSERT INTO UsersRecovery (IdUser, VerificationToken, IsUsed, ExpiryDate) VALUES (@IdUser, @VerificationToken, @IsUsed, @ExpiryDate)";
            var insertParams = new[]
            {
                new SqlParameter("@IdUser", user!.Id),
                new SqlParameter("@VerificationToken", token),
                new SqlParameter("@IsUsed", false),
                new SqlParameter("@ExpiryDate", expiryDate)
            };
            await _dbHelper.ExecuteCommandAsync(insertQuery, insertParams);
            _emailService.SendEmailAsync(user.Email, "Recuperação de senha", $"Uma requisição de recuperação de senha foi identificada. Pare confirmar o reset da senha acesse o link {host}/reset-password?token={token} .");
        }

        public async void ResetUserPassword(string token)
        {
            var query = "SELECT Id, IdUser,  IsUsed, ExpiryDate FROM UsersRecovery WHERE VerificationToken = @VerificationToken";
            var parameters = new[]
            {
                new SqlParameter("@VerificationToken", token)
            };
            using var reader = await _dbHelper.ExecuteReaderAsync(query, parameters);
            if (reader.Read())
            {
                var _userRecovery = new UsersRecovery
                {
                    Id = (int)reader["Id"],
                    IdUser = (int)reader["IdUser"],
                    IsUsed = (bool)reader["IsUsed"],
                    ExpiryDate = (DateTime)reader["ExpiryDate"]
                };
                if (_userRecovery.IsUsed) { throw new Exception("Esse token já foi utilizado."); }
                if (_userRecovery.ExpiryDate > DateTime.UtcNow) { throw new Exception("Esse token não já está expirado."); }

                var user = await GetUserById(_userRecovery.IdUser);
                string password;
                user!.Password = _utils.GeneratePassword(10);
                password = user.Password;

                user.Password = _utils.GerarHash(user.Password);
                var insertQuery = "UPDATE Users set Password = @Password where Login = @Login";
                var insertParams = new[]
                {
                    new SqlParameter("@Password", user.Password),
                    new SqlParameter("@Login", user.Login)
                };

                await _dbHelper.ExecuteCommandAsync(insertQuery, insertParams);

                _emailService.SendEmailAsync(user.Email, "Alteração de senha", $"Sua nova senha é : {user.Password} . Acesse o sistema para alterar sua senha caso deseje.");
                _logGeral.AddLogGeral(_utils.CreateLog($"Senha do usuario {user.Login} foi resetada pelo usuario proprio usuário.", "Users", 0));

            }
            throw new Exception("Token não localizado");
        }
    }