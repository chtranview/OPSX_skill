using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MyWeb.Models;
using MyWeb.Services;
using MyWeb.DTOs;

namespace MyWeb.Controllers;

public class HomeController : Controller
{
    private readonly IUbikeService _ubikeService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IUbikeService ubikeService, ILogger<HomeController> logger)
    {
        _ubikeService = ubikeService;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet]
    public IActionResult AreaQry()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AreaQry(string area)
    {
        if (string.IsNullOrWhiteSpace(area))
        {
            ViewBag.Error = "请输入行政区域";
            return View();
        }

        try
        {
            var result = await _ubikeService.AreaQryAsync(area);
            ViewBag.Result = result;
            ViewBag.Area = area;
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查询 YouBike 数据时发生错误，行政区域: {Area}", area);
            ViewBag.Error = $"查询数据时发生错误: {ex.Message}";
            return View();
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
