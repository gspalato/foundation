using System.Net;
using Foundation.Common.Entities;
using Foundation.Core.SDK.Database.Mongo;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Services.Portfolio.Controllers;

public class ProjectController : Controller
{
    private IRepository<Project> ProjectRepository { get; }

    public ProjectController(IRepository<Project> projectRepository)
    {
        ProjectRepository = projectRepository;
    }

    [HttpGet]
    [Route("projects")]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(List<Project>))]
    public Task<List<Project>> GetProjectsAsync()
    {
        return ProjectRepository.GetAllAsync();
    }
}