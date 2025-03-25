using AdminDashboard.Commands.CourseCommand;
using AdminDashboard.Models;
using AutoMapper;
using MediatR;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Xml;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;

namespace AdminDashboard.Handlers.CourseHandler
{
    public class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, ApiDataResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private const int MaxRetries = 3;
        private const int MaxResultsPerPage = 50;

        public CreateCourseCommandHandler(
            IUnitOfWork unitOfWork,
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<ApiDataResponse> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var playlistId = ExtractPlaylistId(request.playlistViewModel.PlaylistUrl);
                if (string.IsNullOrEmpty(playlistId))
                {
                    return new ApiDataResponse(400, "Invalid playlist URL format");
                }

                var category = await _unitOfWork.Repository<Category>()
                    .GetEntityAsync(request.playlistViewModel.CategoryId);
                if (category == null)
                {
                    return new ApiDataResponse(404, "Category not found");
                }

                var playlist = await GetPlaylistDetailsAsync(playlistId, request.playlistViewModel);
                if (playlist == null)
                {
                    return new ApiDataResponse(404, "Failed to retrieve playlist details from YouTube");
                }

                var videos = await GetPlaylistVideosAsync(playlistId, playlist);
                if (videos == null || !videos.Any())
                {
                    return new ApiDataResponse(404, "No videos found in the playlist");
                }

                playlist.Videos = videos;
                playlist.CourseVideosCount = videos.Count;

                await _unitOfWork.Repository<Course>().AddAsync(playlist);
                var result = await _unitOfWork.Complete();

                return result > 0
                    ? new ApiDataResponse(200, playlist, "Course created successfully")
                    : new ApiDataResponse(500, "Failed to save the course");
            }
            catch (Exception ex)
            {
                return new ApiDataResponse(500, "An error occurred while creating the course");
            }
        }

        private async Task<Course> GetPlaylistDetailsAsync(string playlistId, PlaylistViewModel model)
        {
            var apiKey = _configuration["YouTube:ApiKey"];
            var playlistApiUrl = $"https://www.googleapis.com/youtube/v3/playlists?part=snippet&id={playlistId}&key={apiKey}";

            for (int i = 0; i < MaxRetries; i++)
            {
                try
                {
                    using var playlistResponse = await _httpClient.GetAsync(playlistApiUrl);
                    if (!playlistResponse.IsSuccessStatusCode)
                    {
                        continue;
                    }

                    var playlistContent = await playlistResponse.Content.ReadAsStringAsync();
                    var playlistJson = JObject.Parse(playlistContent);
                    var items = playlistJson["items"] as JArray;

                    if (items == null || !items.Any())
                    {
                        return null;
                    }

                    var snippet = items[0]["snippet"];
                    if (snippet == null)
                    {
                        return null;
                    }

                    return new Course
                    {
                        Name = snippet["title"]?.ToString(),
                        Description = snippet["description"]?.ToString() ?? snippet["title"]?.ToString(),
                        Author = !string.IsNullOrEmpty(model.Author)
                            ? model.Author
                            : snippet["channelTitle"]?.ToString(),
                        CourseLanguage = !string.IsNullOrEmpty(model.CourseLanguage)
                            ? model.CourseLanguage
                            : "العربي",
                        CreatedAt = DateTime.Now,
                        ImageUrl = snippet["thumbnails"]?["high"]?["url"]?.ToString(),
                        CategoryId = model.CategoryId,
                        Videos = new List<Video>()
                    };
                }
                catch (Exception)
                {
                    if (i == MaxRetries - 1)
                    {
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
            var apiKey = _configuration["YouTube:ApiKey"];

            do
            {
                try
                {
                    var videosApiUrl = $"https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&maxResults={MaxResultsPerPage}&playlistId={playlistId}&key={apiKey}&pageToken={nextPageToken}";
                    var videosResponse = await _httpClient.GetAsync(videosApiUrl);

                    if (!videosResponse.IsSuccessStatusCode)
                    {
                        break;
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

                    nextPageToken = videosJson["nextPageToken"]?.ToString();
                }
                catch (Exception)
                {
                    break;
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

                var apiKey = _configuration["YouTube:ApiKey"];
                var videoDetailsUrl = $"https://www.googleapis.com/youtube/v3/videos?part=contentDetails&id={videoId}&key={apiKey}";
                var videoDetailsResponse = await _httpClient.GetAsync(videoDetailsUrl);

                if (!videoDetailsResponse.IsSuccessStatusCode)
                {
                    return null;
                }

                var videoDetailsContent = await videoDetailsResponse.Content.ReadAsStringAsync();
                var videoDetailsJson = JObject.Parse(videoDetailsContent);
                var videoDurationIso = videoDetailsJson["items"]?[0]?["contentDetails"]?["duration"]?.ToString();

                var videoDuration = !string.IsNullOrEmpty(videoDurationIso)
                    ? XmlConvert.ToTimeSpan(videoDurationIso)
                    : TimeSpan.Zero;

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
            catch
            {
                return null;
            }
        }
        private string ExtractPlaylistId(string url)
        {
            if (string.IsNullOrEmpty(url))
                return null;

            try
            {
                var uri = new Uri(url);
                var query = HttpUtility.ParseQueryString(uri.Query);
                return query["list"];
            }
            catch
            {
                return null;
            }
        }
    }
}