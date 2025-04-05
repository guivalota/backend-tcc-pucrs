using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.TCC.PUCRS.Controller;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class PessoaController : ControllerBase
{
    private readonly IPessoaService _pessoaService;
    private readonly ILogger<PessoaController> _logger;

    public PessoaController(IPessoaService pessoaService, ILogger<PessoaController> logger)
    {
        _pessoaService = pessoaService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize]
    [Route("getPessoa")]
    public async Task<ActionResult> GetPessoa()
    {
        _logger.LogInformation("Pegando informações do token.");
        string? login = User.FindFirst(ClaimTypes.Name)?.Value;
        var pessoa = await _pessoaService.GetPessoaByLoginAsync(login!);
        if (pessoa == null)
        {
            _logger.LogWarning($"Pessoa com o login {login} não encontrada.");
            return NotFound(new { Message = "Pessoa não encontrada." });
        }

        return Ok(pessoa);
    }

    [HttpPost("atualizar")]
    [Authorize]
    public async Task<ActionResult> UpdatePessoa([FromBody] Pessoa pessoa)
    {
        if(!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        try
        {
            await _pessoaService.UpdatePessoa(pessoa);
            return Ok("Informações atualizadas com sucesso.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}