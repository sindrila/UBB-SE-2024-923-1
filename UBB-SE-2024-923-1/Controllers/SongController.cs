﻿using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UBB_SE_2024_923_1.Models;
using UBB_SE_2024_923_1.Services;

namespace UBB_SE_2024_923_1.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SongController : ControllerBase
    {
        private readonly SongService _service;
        public SongController(SongService service)
        {
            _service = service;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Song>>> GetSongs()
        {
            var jwt = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            // Decode the JWT token to extract user information
            var jwtHandler = new JwtSecurityTokenHandler();
            var token = jwtHandler.ReadJwtToken(jwt);

            // Accessing user ID from JWT claims
            var userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "id");
            if (userIdClaim == null)
            {
                // Handle the case where user ID is not found in the JWT
                return Unauthorized("Invalid token");
            }

            // Extracting user ID
            var userId = int.Parse(userIdClaim.Value);

            var user = await _service.GetUserById(userId);
            var userAge = user.Age;

            var songs = await _service.GetAllSongs();

            if (userAge < 18)
            {
                songs = songs.Where(song => !song.IsExplicit).ToList();
            }

            return Ok(songs);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<Song>> GetSongById(int id)
        {
            var jwt = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            // Decode the JWT token to extract user information
            var jwtHandler = new JwtSecurityTokenHandler();
            var token = jwtHandler.ReadJwtToken(jwt);

            // Accessing user ID from JWT claims
            var userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "id");
            if (userIdClaim == null)
            {
                // Handle the case where user ID is not found in the JWT
                return Unauthorized("Invalid token");
            }

            // Extracting user ID
            var userId = int.Parse(userIdClaim.Value);

            var user = await _service.GetUserById(userId);
            var userAge = user.Age;

            var song = await _service.GetSongById(id);

            if (song == null)
            {
                return NotFound();
            }

            if (song.IsExplicit && userAge < 18)
            {
                song = null;
            }

            return Ok(song);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Song>> CreateSong(Song song)
        {
            var jwt = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            // Decode the JWT token to extract user information
            var jwtHandler = new JwtSecurityTokenHandler();
            var token = jwtHandler.ReadJwtToken(jwt);

            // Accessing user ID from JWT claims
            var userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "id");
            var userRoleClaim = token.Claims.FirstOrDefault(c => c.Type == "role");

            if (userIdClaim == null)
            {
                // Handle the case where user ID is not found in the JWT
                return Unauthorized("Invalid token");
            }

            // Extracting user ID
            var userId = int.Parse(userIdClaim.Value);
            var userRole = int.Parse(userRoleClaim.Value);

            if (userRole != 2)
            {
                return Unauthorized("Invalid user role");
            }

            if (song == null)
            {
                return BadRequest("Song object is null");
            }

            try
            {
                await _service.AddSong(song);
                var response = new
                {
                    Message = "Song created successfully",
                    Song = song
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSong(int id)
        {
            var jwt = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            // Decode the JWT token to extract user information
            var jwtHandler = new JwtSecurityTokenHandler();
            var token = jwtHandler.ReadJwtToken(jwt);

            // Accessing user ID from JWT claims
            var userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "id");
            if (userIdClaim == null)
            {
                // Handle the case where user ID is not found in the JWT
                return Unauthorized("Invalid token");
            }

            // Extracting user ID
            var userId = int.Parse(userIdClaim.Value);

            var user = await _service.GetUserById(userId);

            var song = await _service.GetSongById(id);

            if (song.ArtistName != user.UserName)
            {
                return Unauthorized("Invalid user");
            }

            if (song == null)
            {
                return NotFound();
            }

            await _service.DeleteSong(song);

            return NoContent();
        }
    }
}