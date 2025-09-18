using Microsoft.AspNetCore.Mvc;
using SocialMedia.Api.Models;
using System;
using System.Collections.Generic;

namespace SocialMedia.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        /// <summary>
        /// Gets all users.
        /// </summary>
        /// <returns>List of users.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<User>), 200)]
        public IActionResult GetAll()
        {
            // TODO: Replace with DB fetch
            var users = new List<User>
            {
                new User { Id = Guid.NewGuid(), Username = "john", Email = "john@example.com" }
            };
            return Ok(users);
        }

        /// <summary>
        /// Gets a user by ID.
        /// </summary>
        /// <param name="id">User's unique identifier.</param>
        /// <returns>User details.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(User), 200)]
        [ProducesResponseType(404)]
        public IActionResult GetById(Guid id)
        {
            // TODO: Replace with DB lookup
            var user = new User { Id = id, Username = "john", Email = "john@example.com" };
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="user">User details.</param>
        /// <returns>Created user.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(User), 201)]
        [ProducesResponseType(400)]
        public IActionResult Create([FromBody] User user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            user.Id = Guid.NewGuid();
            user.RegisteredAt = DateTime.UtcNow;
            // TODO: Save to DB

            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }

        /// <summary>
        /// Updates a user profile.
        /// </summary>
        /// <param name="id">User's unique identifier.</param>
        /// <param name="user">Updated user details.</param>
        /// <returns>No content.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult Update(Guid id, [FromBody] User user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // TODO: DB lookup/update
            // If user not found: return NotFound();

            // Update user logic
            return NoContent();
        }

        /// <summary>
        /// Deletes a user.
        /// </summary>
        /// <param name="id">User's unique identifier.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult Delete(Guid id)
        {
            // TODO: DB lookup/delete
            // If user not found: return NotFound();

            // Delete user logic
            return NoContent();
        }
    }
}