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

    /// <summary>
    /// Gets Information of node by nodeId
    /// </summary>
    /// <param name="nodeId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns><see cref="LightningNodeDto"/></returns>
    [HttpGet("node/{nodeId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<LightningNodeDto>> GetNodeInformationByNodeId(string nodeId, CancellationToken cancellationToken)
    {
        var nodeInformation = await _nodeService.GetNodeInformationByNodeId(nodeId, cancellationToken);

        if (nodeInformation == null)
        {
            return NotFound($"Node with nodeId {nodeId} not found");
        }

        var result = _mapper.Map<LightningNodeDto>(nodeInformation);
        return Ok(result);
    }
}