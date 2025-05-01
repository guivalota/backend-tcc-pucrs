using System.Security.Claims;
using Backend.TCC.PUCRS.Model;
using Backend.TCC.PUCRS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.TCC.PUCRS.Controller;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<Users>> Register(Users user)
    {
        try
        {
            var newUser = await _authService.RegisterAsync(user);
            return Ok("Verifique seu e-mail para completar seu registro.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginModel login)
    {
        try
        {
            var token = await _authService.LoginAsync(login.Login, login.Password);
            var user = await _authService.GetUserByUserNameAsync(login.Login);
            var refreshToken = await _authService.GenerateRefreshToken(user!);
            return Ok(new { Token = token, RefreshToken = refreshToken });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized($"{ex.Message}");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("logout")]
    public async Task<ActionResult> Logout([FromBody] RefreshTokenModel request)
    {
        try
        {
            var storedToken = await _authService.GetRefreshTokenFromDatabase(request.Token!);
            if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiryDate < DateTime.UtcNow)
            {
                //Token já estava expirado
                return Ok("Token já estava expirado.");
            }
            var user = await _authService.GetUserByUserNameAsync(storedToken.UserId!);
            _authService.LogoutAsync(storedToken.Token!, user!);
            return Ok("Logout realizado com sucesso.");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized($"{ex.Message}");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult> RefreshToken([FromBody] RefreshTokenModel request)
    {
        var storedToken = await _authService.GetRefreshTokenFromDatabase(request.Token!);
        if (storedToken == null || storedToken.IsRevoked || DateTime.UtcNow < storedToken.ExpiryDate)
        {
            return Unauthorized("Token inválido ou expirado.");
        }
        try
        {
            var user = await _authService.GetUserByUserNameAsync(storedToken.UserId!);
            var token = await _authService.GenerateJwtToken(user!);
            var refreshToken = await _authService.GenerateRefreshToken(user!);
            return Ok(new { Token = token, RefreshToken = refreshToken });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized($"{ex.Message}");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch("validar")]
    public async Task<ActionResult<string>> Validar(string token)
    {
        try
        {
            if (await _authService.VerifyEmailAsync(token))
            {
                return Ok("E-mail validado com sucesso.");
            }
            else
            {
                return BadRequest("Não foi possivel validar o e-mail");
            }
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("register-novo")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Users>> RegistrarNovoAsync(Users user)
    {
        try
        {
            var newUser = await _authService.RegisterNewUserAsync(user);
            return Ok("Um e-mail foi enviado para o novo usuário contendo sua senha e instruções para que seja validado o cadastro.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    [Route("listar-usuarios")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<Users>>> ListarUsuariosAsync()
    {
        var login = User.FindFirst(ClaimTypes.Name)?.Value;
        var usuarios = await _authService.ListarUsuarios(login!);
        return Ok(usuarios);
    }

    [HttpPost("alterar-senha")]
    [Authorize]
    public ActionResult<string> AlterarSenha([FromBody] Users user)
    {
        try
        {
            var login = User.FindFirst(ClaimTypes.Name)?.Value;
            if (user.Login != login)
            {
                return Unauthorized($"O Usuário que você está tentando alterar é diferente do seu.");
            }
            _authService.AlterarSenha(user);
            return Ok("Senha atualizada com sucesso.");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized($"{ex.Message}");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("resetar-senha")]
    [Authorize(Roles = "Admin")]
    public ActionResult<string> ResetarSenha([FromBody] Users user)
    {
        try
        {
            var login = User.FindFirst(ClaimTypes.Name)?.Value;
            if (user.Login == login)
            {
                return Unauthorized($"Você não pode resetar a senha do seu próprio usuário. Altere sua senha.");
            }
            _authService.AlterarSenha(user, login!);
            return Ok("Senha do usuário resetada. Um email foi enviado para ele contendo a nova senha.");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized($"{ex.Message}");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("request-reset-password")]
    public async Task<ActionResult> RequestResetPassword([FromBody] string email)
    {
        try
        {
            Users? user = await _authService.GetUserByEmail(email);
            if (user == null)
            {
                return Ok("Um E-mail de recuperação de senha foi enviada para o e-mail requisitado.");
            }
            else
            {
                _authService.RequestRestarUser(user.Login);
                return Ok("Um E-mail de recuperação de senha foi enviada para o e-mail requisitado.");
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized($"{ex.Message}");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("reset-password-token")]
    public ActionResult ResetPassword([FromBody]string token)
    {
        try
        {
            _authService.ResetUserPassword(token);
            return Ok("Um E-mail com sua nova senha foi enviada par ao seu email.");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized($"{ex.Message}");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
