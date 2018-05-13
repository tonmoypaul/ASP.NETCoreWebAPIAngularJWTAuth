using System.Collections.Generic;
using System.Threading.Tasks;
using ASP.NETCoreWebAPIAngularJWTAuth.Dtos;
using ASP.NETCoreWebAPIAngularJWTAuth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AuthSystem.Controllers
{
    [Route("/api/[controller]")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;

        public UsersController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            IConfiguration config)
        {
            this._mapper = mapper;
            this._config = config;
            this._userManager = userManager;
            this._context = context;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Authenticate([FromBody] ApplicationUserDto dto)
        {
            // check modelstate
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // check if username or password is empty
            if (string.IsNullOrEmpty(dto.UserName) || string.IsNullOrEmpty(dto.Password))
                return await Task.FromResult<IActionResult>(BadRequest());

            // get the user from the db
            var user = await _userManager.FindByNameAsync(dto.UserName);

            // check if user is null
            if (user == null)
                return await Task.FromResult<IActionResult>(BadRequest());

            // check password - generate jwt token and return along with user info
            if (await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenSigningKey = _config.GetSection("TokenSignningSecretKey");
                var key = Encoding.ASCII.GetBytes(tokenSigningKey.Value);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, user.Id)
                    }),
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);
                return Ok(new
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Token = tokenString
                });
            }

            // credentials are invalid
            return await Task.FromResult<IActionResult>(Unauthorized());
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Register([FromBody] ApplicationUserDto dto)
        {
            // check model state
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // map user from dto and try to create an user
            var user = _mapper.Map<ApplicationUserDto, ApplicationUser>(dto);
            var result = await _userManager.CreateAsync(user, dto.Password);
            
            // not succeed?
            if (!result.Succeeded)
                return BadRequest();
            
            // save db and return success message
            await _context.SaveChangesAsync();
            return new OkObjectResult("User has been created successfully");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _context.Users.ToListAsync();
            var usersDto = _mapper.Map<List<ApplicationUser>, List<ApplicationUserDto>>(users);
            return Ok(usersDto);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var user = await _context.Users.FindAsync(id);
            var userDto = _mapper.Map<ApplicationUser, ApplicationUserDto>(user);
            return Ok(userDto);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody]ApplicationUserDto dto)
        {
            // check model state
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            // check the user from db
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return BadRequest("No such user found");
            
            // check if the requester knows the current password to verify the update request
            var isAuthenticatedRequest = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!isAuthenticatedRequest)
                return BadRequest("Please enter the current password to verify");
            
            // check if the username is unique when changed
            if (dto.UserName != user.UserName)
                if (await _context.Users.AnyAsync(u => u.UserName == dto.UserName))
                    return BadRequest("Username "+dto.UserName+" is already taken");
            
            // updaing manually; as mapping and then updating using usermanager 
            // throws some error about tracking multiple instance of same key value
            user.UserName = dto.UserName;
            user.Email = dto.Email;
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.PhoneNumber = dto.PhoneNumber;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return new OkObjectResult("User has been updated");
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return BadRequest("No such user found");
            
            await _userManager.DeleteAsync(user);
            return Ok("User has been deleted");
        }
    }
}