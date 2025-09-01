using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SocialMedia.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAllPosts()
        {
            // TODO: Fetch posts from Cosmos DB
            return Ok(new[] { "Post1", "Post2" });
        }

        [HttpGet("{id}")]
        public IActionResult GetPost(string id)
        {
            // TODO: Fetch post by ID
            return Ok($"Post {id}");
        }

        [HttpPost]
        public IActionResult CreatePost([FromBody] object post)
        {
            // TODO: Add new post to Cosmos DB
            return CreatedAtAction(nameof(GetPost), new { id = "newId" }, post);
        }

        [HttpPut("{id}")]
        public IActionResult UpdatePost(string id, [FromBody] object post)
        {
            // TODO: Update post in Cosmos DB
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeletePost(string id)
        {
            // TODO: Delete post from Cosmos DB
            return NoContent();
        }
    }
}
