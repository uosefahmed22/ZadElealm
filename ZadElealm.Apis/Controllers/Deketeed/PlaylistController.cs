using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Xml;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Controllers.Deketeed
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlaylistController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IUnitOfWork _unitOfWork;

        public PlaylistController(IUnitOfWork unitOfWork)
        {
            _httpClient = new HttpClient();
            _unitOfWork = unitOfWork;
        }
        [HttpPost]
        public async Task<IActionResult> Create(string playlistUrl, int categoryId, string? courseLanguage = null, string? author = null)
        {
            if (string.IsNullOrEmpty(playlistUrl))
            {
                return BadRequest("Playlist URL is required.");
            }

            var playlistId = ExtractPlaylistId(playlistUrl);
            if (string.IsNullOrEmpty(playlistId))
            {
                return BadRequest("Invalid playlist URL.");
            }

            var apiKey = "AIzaSyD79T1TuP3_0uEFI7CftLd3bIzmd9PdelE";
            var playlistApiUrl = $"https://www.googleapis.com/youtube/v3/playlists?part=snippet&id={playlistId}&key={apiKey}";
            var playlistResponse = await _httpClient.GetAsync(playlistApiUrl);

            if (!playlistResponse.IsSuccessStatusCode)
            {
                return StatusCode((int)playlistResponse.StatusCode, "Error fetching playlist details.");
            }

            var playlistContent = await playlistResponse.Content.ReadAsStringAsync();
            var playlistJson = JObject.Parse(playlistContent);
            var snippet = playlistJson["items"]?[0]?["snippet"];

            if (snippet == null)
            {
                return NotFound("Playlist not found.");
            }

            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(categoryId);
            if (category == null)
            {
                return NotFound("Category not found.");
            }

            var defaultAuthor = snippet["channelTitle"]?.ToString();
            var defaultLanguage = "العربي";

            var playlist = new Course
            {
                Name = snippet["title"]?.ToString(),
                Description = snippet["description"]?.ToString(),
                Author = !string.IsNullOrEmpty(author) ? author : defaultAuthor,
                CourseLanguage = !string.IsNullOrEmpty(courseLanguage) ? courseLanguage : defaultLanguage,
                CreatedAt = DateTime.Now,
                ImageUrl = snippet["thumbnails"]?["high"]?["url"]?.ToString(),
                CategoryId = categoryId,
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
                    return StatusCode((int)videosResponse.StatusCode, "Error fetching videos from the playlist.");
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
                            return StatusCode((int)videoDetailsResponse.StatusCode, "Error fetching video details.");
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

           return Ok(new ApiResponse(200, "Playlist created successfully"));
        }

        private string ExtractPlaylistId(string url)
        {
            var uri = new Uri(url);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            return query["list"];
        }
    }
}
