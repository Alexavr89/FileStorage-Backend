﻿using FileStorageDAL.Entities;
using FileStorageDAL.Models;
using FileStorageDAL.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace File_Storage.Controllers
{
    // [Authorize(Roles = "Admin")]
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        public UsersController(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _roleManager = roleManager;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] ApplicationUser user)
        {
            if (user == null)
                return BadRequest("Please use a valid user");
            await _userManager.CreateAsync(user);
            await _unitOfWork.SaveAsync();
            return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }

        [HttpGet]
        public IEnumerable<UserRole> GetUsers()
        {
            var roles = _roleManager.Roles;
            var users = _userManager.Users;
            var userroles = new List<UserRole>();
            foreach (var user in users)
            {
                foreach (var role in roles)
                {
                    if (_userManager.IsInRoleAsync(user, role.Name).Result)
                    {
                        var ur = new UserRole()
                        {
                            UserId = user.Id,
                            RoleName = role.Name,
                            UserName = user.UserName
                        };
                        userroles.Add(ur);
                    }
                }
            }
            return userroles;
        }

        [HttpGet("{id}")]
        public object GetUser(string id)
        {
            var user = _userManager.FindByIdAsync(id).Result;
            var role = _userManager.GetRolesAsync(user).Result;
            return new { user, role };
        }

        [HttpPut("{id}/{role}")]
        public async Task<IActionResult> EditUser(string id, string role, [FromBody] ApplicationUser newuser)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("There is no user with such Id");
            }
            user.Email = newuser.Email;
            user.UserName = newuser.UserName;
            await _userManager.UpdateAsync(user);
            await _userManager.RemoveFromRoleAsync(user, _userManager.GetRolesAsync(user).Result[0]);
            await _userManager.AddToRoleAsync(user, role);
            await _unitOfWork.SaveAsync();
            return CreatedAtAction("GetUser", new { id = newuser.Id }, newuser);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("There is no user with such Id");
            }
            await _userManager.DeleteAsync(user);
            await _unitOfWork.SaveAsync();
            return NoContent();
        }
    }
}
