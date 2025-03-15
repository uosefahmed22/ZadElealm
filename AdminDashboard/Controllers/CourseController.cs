using AdminDashboard.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Xml;
using ZadElealm.Apis.Dtos;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;

namespace AdminDashboard.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CourseController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;
        private readonly HttpClient _httpClient;

        public CourseController(IUnitOfWork unitOfWork, IMapper mapper, IImageService imageService, HttpClient  httpClient)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _imageService = imageService;
            _httpClient = httpClient;
        }
        public async Task<IActionResult> Index()
        {
            var courses = await _unitOfWork.Repository<Course>().GetAllAsync();
            var mappedCourses = _mapper.Map<IReadOnlyList<Course>, IReadOnlyList<CourseDto>>(courses);

            return View(mappedCourses);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var course = await _unitOfWork.Repository<Course>().GetEntityAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            var model = new CourseDto
            {
                Name = course.Name,
                Description = course.Description,
                Author = course.Author,
                CourseLanguage = course.CourseLanguage,
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Dto.CourseDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var courseExsist = await _unitOfWork.Repository<Course>().GetEntityAsync(model.Id);
            if (courseExsist == null)
            {
                return NotFound();
            }

            courseExsist.Name = model.Name;
            courseExsist.Description = model.Description;
            courseExsist.Author = model.Author;
            courseExsist.CourseLanguage = model.CourseLanguage;


            if (model.Image != null)
            {
                var uploadedImage = await _imageService.UploadImageAsync(model.Image);
                courseExsist.ImageUrl = uploadedImage;
            }

            await _unitOfWork.Complete();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var category = await _unitOfWork.Repository<Course>().GetEntityAsync(id);
            _unitOfWork.Repository<Course>().Delete(category);
            await _unitOfWork.Complete();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _unitOfWork.Repository<Category>().GetAllAsync();
            ViewBag.Categories = categories;
            var model = new PlaylistViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(PlaylistViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var playlistId = ExtractPlaylistId(model.PlaylistUrl);
            if (string.IsNullOrEmpty(playlistId))
            {
                ModelState.AddModelError("PlaylistUrl", "Invalid playlist URL.");
                return View(model);
            }

            var apiKey = "AIzaSyD79T1TuP3_0uEFI7CftLd3bIzmd9PdelE";
            var playlistApiUrl = $"https://www.googleapis.com/youtube/v3/playlists?part=snippet&id={playlistId}&key={apiKey}";
            var playlistResponse = await _httpClient.GetAsync(playlistApiUrl);

            if (!playlistResponse.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Error fetching playlist details.");
                return View(model);
            }

            var playlistContent = await playlistResponse.Content.ReadAsStringAsync();
            var playlistJson = JObject.Parse(playlistContent);
            var snippet = playlistJson["items"]?[0]?["snippet"];

            if (snippet == null)
            {
                ModelState.AddModelError("", "Playlist not found.");
                return View(model);
            }

            var category = await _unitOfWork.Repository<Category>().GetEntityAsync(model.CategoryId);
            if (category == null)
            {
                ModelState.AddModelError("CategoryId", "Category not found.");
                return View(model);
            }

            var defaultAuthor = snippet["channelTitle"]?.ToString();
            var defaultLanguage = "العربي";

            var playlist = new Course
            {
                Name = snippet["title"]?.ToString(),
                Description = snippet["description"]?.ToString() ?? snippet["title"]?.ToString(),
                Author = !string.IsNullOrEmpty(model.Author) ? model.Author : defaultAuthor,
                CourseLanguage = !string.IsNullOrEmpty(model.CourseLanguage) ? model.CourseLanguage : defaultLanguage,
                CreatedAt = DateTime.Now,
                ImageUrl = snippet["thumbnails"]?["high"]?["url"]?.ToString(),
                CategoryId = model.CategoryId,
                Videos = new List<Video>()
            };

            string nextPageToken = "";
            int videoCount = 0;

            do
            {
                var videosApiUrl = $"https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&maxResults=50&playlistId={playlistId}&key={apiKey}&pageToken={nextPageToken}";
                var videosResponse = await _httpClient.GetAsync(videosApiUrl);

                if (!videosResponse.IsSuccessStatusCode)
                {
                    ModelState.AddModelError("", "Error fetching videos from the playlist.");
                    return View(model);
                }

                var videosContent = await videosResponse.Content.ReadAsStringAsync();
                var videosJson = JObject.Parse(videosContent);

                foreach (var item in videosJson["items"])
                {
                    var videoSnippet = item["snippet"];
                    var videoId = videoSnippet["resourceId"]?["videoId"]?.ToString();

                    if (!string.IsNullOrEmpty(videoId))
                    {
                        var videoDetailsUrl = $"https://www.googleapis.com/youtube/v3/videos?part=contentDetails&id={videoId}&key={apiKey}";
                        var videoDetailsResponse = await _httpClient.GetAsync(videoDetailsUrl);

                        if (!videoDetailsResponse.IsSuccessStatusCode)
                        {
                            ModelState.AddModelError("", "Error fetching video details.");
                            return View(model);
                        }

                        var videoDetailsContent = await videoDetailsResponse.Content.ReadAsStringAsync();
                        var videoDetailsJson = JObject.Parse(videoDetailsContent);
                        var videoDurationIso = videoDetailsJson["items"]?[0]?["contentDetails"]?["duration"]?.ToString();

                        TimeSpan videoDuration = TimeSpan.Zero;
                        if (!string.IsNullOrEmpty(videoDurationIso))
                        {
                            videoDuration = XmlConvert.ToTimeSpan(videoDurationIso);
                        }

                        var video = new Video
                        {
                            Title = videoSnippet["title"]?.ToString(),
                            Description = videoSnippet["description"]?.ToString(),
                            VideoUrl = $"https://www.youtube.com/watch?v={videoId}",
                            ThumbnailUrl = videoSnippet["thumbnails"]?["high"]?["url"]?.ToString(),
                            VideoDuration = videoDuration,
                            Course = playlist
                        };

                        playlist.Videos.Add(video);
                        videoCount++;
                    }
                }

                nextPageToken = videosJson["nextPageToken"]?.ToString() ?? "";

            } while (!string.IsNullOrEmpty(nextPageToken));

            playlist.CourseVideosCount = videoCount;

            await _unitOfWork.Repository<Course>().AddAsync(playlist);
            await _unitOfWork.Complete();

            return RedirectToAction("Index");
        }

        private string ExtractPlaylistId(string url)
        {
            var uri = new Uri(url);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            return query["list"];
        }
    }
}
