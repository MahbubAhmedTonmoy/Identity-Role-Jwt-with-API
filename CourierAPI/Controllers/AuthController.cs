using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CourierAPI.DTOs;
using CourierAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CourierAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _config;

        public AuthController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<AppUser> signInManager, IConfiguration config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _config = config;
        }

        
        [HttpPost("register")]
        public async Task<IActionResult> Registration(UserForRegistrationDTO userForRegistrationDTO)
        {
            //SuperAdmin
            if(!await _roleManager.RoleExistsAsync(Role.SuperAdmin))
            {
                await _roleManager.CreateAsync(new IdentityRole(Role.SuperAdmin));
            }
            //CourierOwner
            if(!await _roleManager.RoleExistsAsync(Role.CourierOwner))
            {
                await _roleManager.CreateAsync(new IdentityRole(Role.CourierOwner));
            }
            //BookingOfficer
            if(!await _roleManager.RoleExistsAsync(Role.BookingOfficer))
            {
                await _roleManager.CreateAsync(new IdentityRole(Role.BookingOfficer));
            }
            //Marchant
            if(!await _roleManager.RoleExistsAsync(Role.Marchant))
            {
                await _roleManager.CreateAsync(new IdentityRole(Role.Marchant));
            }
            //DelivaryMan
            if(!await _roleManager.RoleExistsAsync(Role.DelivaryMan))
            {
                await _roleManager.CreateAsync(new IdentityRole(Role.DelivaryMan));
            }
            //Client            
            if(!await _roleManager.RoleExistsAsync(Role.Client))
            {
                await _roleManager.CreateAsync(new IdentityRole(Role.Client));
            }

            //Create User Object
            var userToCreate = new AppUser
            {
                UserName = userForRegistrationDTO.UserName,
                Email = userForRegistrationDTO.UserName
                
            };

            if(await _userManager.FindByNameAsync(userForRegistrationDTO.UserName) != null)
                return BadRequest("User name already exists!");    

            var createdUser = await _userManager.CreateAsync(userToCreate, userForRegistrationDTO.Password);
            if(createdUser.Succeeded)
            {
                if(userForRegistrationDTO.Role == "SuperAdmin"){
                await _userManager.AddToRoleAsync(userToCreate,Role.SuperAdmin);
                }
                if(userForRegistrationDTO.Role == "CourierOwner"){
                await _userManager.AddToRoleAsync(userToCreate,Role.CourierOwner);
                }
                if(userForRegistrationDTO.Role == "Marchant"){
                await _userManager.AddToRoleAsync(userToCreate,Role.Marchant);
                }
                if(userForRegistrationDTO.Role == "BookingOfficer"){
                await _userManager.AddToRoleAsync(userToCreate,Role.BookingOfficer);
                }
                if(userForRegistrationDTO.Role == "DelivaryMan"){
                await _userManager.AddToRoleAsync(userToCreate,Role.DelivaryMan);
                }
                if(userForRegistrationDTO.Role == "Client"){
                await _userManager.AddToRoleAsync(userToCreate,Role.Client);
                }

                return Ok(createdUser);
            }
            //Create User
            return BadRequest(ModelState);                   
        }
    
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDTO userForLoginDTO)
        {
            // throw new Exception("noooooo");
            var user = await _userManager.FindByNameAsync(userForLoginDTO.UserName);
            if(user == null)
                return Unauthorized(); 

            var result = await _signInManager
                .CheckPasswordSignInAsync(user, userForLoginDTO.Password, false); //Please use better option
            if(result.Succeeded){
            var role = await _userManager.GetRolesAsync(user);
            string roleAssigned = role[0];
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, roleAssigned)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(_config.GetSection("AppSettings:Token").Value)); //Set Secret value
            
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            
            //insert information to token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            
            var token = tokenHandler.CreateToken(tokenDescriptor);
            

            return Ok(new {
                token = tokenHandler.WriteToken(token)
            });
            }
            return Unauthorized();
            
        }
        
        [Authorize]
        [HttpGet("test")]
        
        public IActionResult Test()
        {
            return Ok("done");
        }

    }
}