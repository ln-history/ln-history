using AutoMapper;
using LN_history.Api.Authorization;
using LN_history.Api.Dto;
using LN_history.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LN_history.Api.Controllers;

[ApiController]
[Route("ln-history/[controller]")]
[ApiKeyAuthorize]
public class NodeController : ControllerBase
{
    private readonly INodeService _nodeService;
    private readonly IMapper _mapper;

    public NodeController(INodeService nodeService, IMapper mapper)
    {
        _nodeService = nodeService;
        _mapper = mapper;
    }

    // [HttpGet("node_id/{timestamp}")]
    // public async Task<IActionResult> GetNodeGossipByNodeId(string nodeId)
    // {
    //     try
    //     {
    //         var fileName = $"snapshot_{timestamp:yyyyMMdd_HHmmss}.bin";
    //
    //         return File(, fileName);
    //     }
    //     catch (Exception ex)
    //     {
    //         return StatusCode(StatusCodes.Status500InternalServerError, ex);
    //     }
    // }

    // /// <summary>
    // /// Gets Information of node by nodeId
    // /// </summary>
    // /// <param name="nodeId"></param>
    // /// <param name="cancellationToken"></param>
    // /// <returns><see cref="LightningNodeDto"/></returns>
    // [HttpGet("node/{nodeId}")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // public async Task<ActionResult<LightningNodeDto>> GetNodeInformationByNodeId(string nodeId, CancellationToken cancellationToken)
    // {
    //     throw new NotImplementedException();
    // }
}