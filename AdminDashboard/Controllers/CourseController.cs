using AdminDashboard.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Xml;
using ZadElealm.Apis.Dtos.DtosCourse;
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
        private readonly string _apiKey = "AIzaSyD79T1TuP3_0uEFI7CftLd3bIzmd9PdelE";
        private const int MaxRetries = 3;
        private const int MaxResultsPerPage = 50;

        public CourseController(IUnitOfWork unitOfWork, IMapper mapper, IImageService imageService, HttpClient httpClient)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _imageService = imageService;
            _httpClient = httpClient;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var categories = await _unitOfWork.Repository<Category>().GetAllAsync();
                ViewBag.Categories = categories;
                var model = new PlaylistViewModel();
                return View(model);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(PlaylistViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await LoadCategories();
                    return View(model);
                }

                var playlistId = ExtractPlaylistId(model.PlaylistUrl);
                if (string.IsNullOrEmpty(playlistId))
                {
                    ModelState.AddModelError("PlaylistUrl", "Invalid playlist URL. Please provide a valid YouTube playlist URL.");
                    await LoadCategories();
                    return View(model);
                }

                var playlist = await GetPlaylistDetailsAsync(playlistId, model);
                if (playlist == null)
                {
                    await LoadCategories();
                    return View(model);
                }

                var videos = await GetPlaylistVideosAsync(playlistId, playlist);
                if (videos == null)
                {
                    await LoadCategories();
                    return View(model);
                }

                playlist.Videos = videos;
                playlist.CourseVideosCount = videos.Count;

                await _unitOfWork.Repository<Course>().AddAsync(playlist);
                await _unitOfWork.Complete();

                TempData["Success"] = "Course created successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An unexpected error occurred while creating the course.");
                await LoadCategories();
                return View(model);
            }
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
                courseExsist.ImageUrl = uploadedImage.Data as string;
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

        private async Task<Course> GetPlaylistDetailsAsync(string playlistId, PlaylistViewModel model)
        {
            var playlistApiUrl = $"https://www.googleapis.com/youtube/v3/playlists?part=snippet&id={playlistId}&key={_apiKey}";

            for (int i = 0; i < MaxRetries; i++)
            {
                try
                {
                    using var playlistResponse = await _httpClient.GetAsync(playlistApiUrl);
                    if (!playlistResponse.IsSuccessStatusCode)
                    {
                        ModelState.AddModelError("", $"Error fetching playlist details. Status: {playlistResponse.StatusCode}");
                        return null;
                    }

                    var playlistContent = await playlistResponse.Content.ReadAsStringAsync();
                    var playlistJson = JObject.Parse(playlistContent);
                    var items = playlistJson["items"] as JArray;

                    if (items == null || !items.Any())
                    {
                        ModelState.AddModelError("", "Playlist not found or empty.");
                        return null;
                    }

                    var snippet = items[0]["snippet"];
                    if (snippet == null)
                    {
                        ModelState.AddModelError("", "Invalid playlist data.");
                        return null;
                    }

                    var category = await _unitOfWork.Repository<Category>().GetEntityAsync(model.CategoryId);
                    if (category == null)
                    {
                        ModelState.AddModelError("CategoryId", "Selected category not found.");
                        return null;
                    }

                    var defaultAuthor = snippet["channelTitle"]?.ToString();
                    var defaultLanguage = "العربي";

                    return new Course
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
                }
                catch (Exception ex)
                {
                    if (i == MaxRetries - 1)
                    {
                        ModelState.AddModelError("", "Failed to fetch playlist details after multiple attempts.");
                        return null;
                    }
                    await Task.Delay(1000 * (i + 1));
                }
            }

            return null;
        }
        private async Task<List<Video>> GetPlaylistVideosAsync(string playlistId, Course playlist)
        {
            var videos = new List<Video>();
            string nextPageToken = "";

            do
            {
                try
                {
                    var videosApiUrl = $"https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&maxResults={MaxResultsPerPage}&playlistId={playlistId}&key={_apiKey}&pageToken={nextPageToken}";
                    var videosResponse = await _httpClient.GetAsync(videosApiUrl);

                    if (!videosResponse.IsSuccessStatusCode)
                    {
                        ModelState.AddModelError("", "Error fetching videos from the playlist.");
                        return null;
                    }

                    var videosContent = await videosResponse.Content.ReadAsStringAsync();
                    var videosJson = JObject.Parse(videosContent);

                    foreach (var item in videosJson["items"])
                    {
                        var video = await CreateVideoFromJsonAsync(item, playlist);
                        if (video != null)
                        {
                            videos.Add(video);
                        }
                    }

                    nextPageToken = videosJson["nextPageToken"]?.ToString() ?? "";
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error processing playlist videos.");
                    return null;
                }
            } while (!string.IsNullOrEmpty(nextPageToken));

            return videos;
        }
        private async Task<Video> CreateVideoFromJsonAsync(JToken item, Course playlist)
        {
            try
            {
                var videoSnippet = item["snippet"];
                var videoId = videoSnippet["resourceId"]?["videoId"]?.ToString();

                if (string.IsNullOrEmpty(videoId))
                {
                    return null;
                }

                var videoDetailsUrl = $"https://www.googleapis.com/youtube/v3/videos?part=contentDetails&id={videoId}&key={_apiKey}";
                var videoDetailsResponse = await _httpClient.GetAsync(videoDetailsUrl);

                if (!videoDetailsResponse.IsSuccessStatusCode)
                {
                    return null;
                }

                var videoDetailsContent = await videoDetailsResponse.Content.ReadAsStringAsync();
                var videoDetailsJson = JObject.Parse(videoDetailsContent);
                var videoDurationIso = videoDetailsJson["items"]?[0]?["contentDetails"]?["duration"]?.ToString();

                TimeSpan videoDuration = TimeSpan.Zero;
                if (!string.IsNullOrEmpty(videoDurationIso))
                {
                    videoDuration = XmlConvert.ToTimeSpan(videoDurationIso);
                }

                return new Video
                {
                    Title = videoSnippet["title"]?.ToString(),
                    Description = videoSnippet["description"]?.ToString(),
                    VideoUrl = $"https://www.youtube.com/watch?v={videoId}",
                    ThumbnailUrl = videoSnippet["thumbnails"]?["high"]?["url"]?.ToString(),
                    VideoDuration = videoDuration,
                    Course = playlist
                };
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private string ExtractPlaylistId(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                    return null;

                var uri = new Uri(url);
                var query = HttpUtility.ParseQueryString(uri.Query);
                return query["list"];
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private async Task LoadCategories()
        {
            var categories = await _unitOfWork.Repository<Category>().GetAllAsync();
            ViewBag.Categories = categories;
        }

    }
}
