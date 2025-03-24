using AdminDashboard.Commands.CourseCommand;
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
    public class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "AIzaSyD79T1TuP3_0uEFI7CftLd3bIzmd9PdelE";
        private const int MaxRetries = 3;
        private const int MaxResultsPerPage = 50;

        public CreateCourseCommandHandler(IUnitOfWork unitOfWork, IMapper mapper,
             HttpClient httpClient)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpClient = httpClient;
        }

        public async Task<ApiResponse> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var playlistId = ExtractPlaylistId(request.PlaylistUrl);
                if (string.IsNullOrEmpty(playlistId))
                    return new ApiResponse(404, "Invalid Course URL");

                var playlist = await GetPlaylistDetailsAsync(playlistId, request);
                if (playlist == null)
                    return new ApiResponse(404, "Failed to fetch course details");

                var videos = await GetPlaylistVideosAsync(playlistId, playlist);
                if (videos == null)
                    return new ApiResponse(404, "Failed to fetch course videos");

                playlist.Videos = videos;
                playlist.CourseVideosCount = videos.Count;

                await _unitOfWork.Repository<Course>().AddAsync(playlist);
                await _unitOfWork.Complete();

                return new ApiResponse(200, "Course created successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, ex.Message);
            }
        }

        private async Task<Course> GetPlaylistDetailsAsync(string playlistId, CreateCourseCommand request)
        {
            var playlistApiUrl = $"https://www.googleapis.com/youtube/v3/playlists?part=snippet&id={playlistId}&key={_apiKey}";

            for (int i = 0; i < MaxRetries; i++)
            {
                try
                {
                    using var playlistResponse = await _httpClient.GetAsync(playlistApiUrl);
                    if (!playlistResponse.IsSuccessStatusCode)
                        return null;

                    var playlistContent = await playlistResponse.Content.ReadAsStringAsync();
                    var playlistJson = JObject.Parse(playlistContent);
                    var items = playlistJson["items"] as JArray;

                    if (items == null || !items.Any())
                        return null;

                    var snippet = items[0]["snippet"];
                    if (snippet == null)
                        return null;

                    var category = await _unitOfWork.Repository<Category>().GetEntityAsync(request.CategoryId);
                    if (category == null)
                        return null;

                    var defaultAuthor = snippet["channelTitle"]?.ToString();
                    var defaultLanguage = "العربي";

                    return new Course
                    {
                        Name = snippet["title"]?.ToString(),
                        Description = snippet["description"]?.ToString() ?? snippet["title"]?.ToString(),
                        Author = !string.IsNullOrEmpty(request.Author) ? request.Author : defaultAuthor,
                        CourseLanguage = !string.IsNullOrEmpty(request.CourseLanguage) ? request.CourseLanguage : defaultLanguage,
                        CreatedAt = DateTime.Now,
                        ImageUrl = snippet["thumbnails"]?["high"]?["url"]?.ToString(),
                        CategoryId = request.CategoryId,
                        Videos = new List<Video>()
                    };
                }
                catch (Exception)
                {
                    if (i == MaxRetries - 1)
                        return null;
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
                        return null;

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
                catch (Exception)
                {
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
                    return null;

                var videoDetailsUrl = $"https://www.googleapis.com/youtube/v3/videos?part=contentDetails&id={videoId}&key={_apiKey}";
                var videoDetailsResponse = await _httpClient.GetAsync(videoDetailsUrl);

                if (!videoDetailsResponse.IsSuccessStatusCode)
                    return null;

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
            catch (Exception)
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
            catch (Exception)
            {
                return null;
            }
        }
    }
}