using ExcursionApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExcursionApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MainSettingController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public MainSettingController(IHttpContextAccessor httpContextAccessor, IAdminService adminService)
        {
            _adminService = adminService;
            _httpContextAccessor = httpContextAccessor;
           
        }
        #region "Main_setting"

        [HttpPost("Get_Currencies")]
        public async Task<IActionResult> Get_Currencies()
        {
            return Ok(await _adminService.Get_Currencies());
        }
        [HttpPost("Get_Languages")]
        public async Task<IActionResult> Get_Languages()
        {
            return Ok(await _adminService.Get_Languages());
        }
        #endregion
    }
}
