using BookingApp.Controllers.Bases;
using BookingApp.Data;
using BookingApp.Helpers;
using BookingApp.Services;
using BookingApp.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading.Tasks;

namespace BookingApp.Controllers
{
    [Route("api/chat")]
    [ApiController]
    public class ChatController : EntityControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IResourcesService _resourceService;
        public ChatController(ApplicationDbContext applicationDbContext, IResourcesService resourceService)
        {
            this._applicationDbContext = applicationDbContext;
            _resourceService = resourceService;
        }

        [HttpGet("{userId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Authorize(Roles = RoleTypes.Admin)]
        public async Task<IActionResult> GetChats([FromRoute] string userId)
        {
           var chats = await _applicationDbContext.Chats
                .Include(x => x.Resource)
                .Include(x => x.Creator)
                .Where(x => x.Resource.CreatedUserId == userId)
                .Select( x =>  new SimpleChatDto { UserName = x.Creator.UserName , ResourceName = x.Resource.Title, ResourceId = x.Resource.Id })
                .ToListAsync();

            return Ok(chats);
        }
    }

    class SimpleChatDto
    {
        public string UserName { get; set; }
        public string ResourceName { get; set; }
        public int ResourceId { get; set; }
    }
}
