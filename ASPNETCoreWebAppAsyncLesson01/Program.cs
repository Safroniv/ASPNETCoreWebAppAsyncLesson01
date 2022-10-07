using System.Text.Json;
Console.WriteLine("Start\n");

PostService.ReWriteFile();

IList<Task<Post>> tasks = new List<Task<Post>>();

CancellationTokenSource tokenSource = new CancellationTokenSource(2000);

for (int id = 4; id < 14; id++)
{
    var task = PostService.GetPostAsync(id, tokenSource.Token);
    tasks.Add(task);
}

var result = Task.WhenAll(tasks);
foreach (var post in result.Result)
{
    if (post != null)
    {
        PostService.Save(post);
    }
}

Console.WriteLine("\nEnd");

class Post
{
    public int UserId { get; set; }
    public int Id { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }

    public IEnumerable<string> Content => new string[] { $"{UserId}", $"{Id}", Title, Body };
}

public static class PostService
{
    static private string _baseUrl = "https://jsonplaceholder.typicode.com/posts/";
    static private string _path = "result.txt";

    internal static async Task<Post> GetPostAsync(int postId, CancellationToken token)
    {
        Console.WriteLine($"Get post {postId}");

        HttpClient _httpClient = new HttpClient();

        Random random = new Random();
        int rnd = random.Next(1, 5) * 100;
        await Task.Delay(rnd);

        int i = 0;

        try
        {
            CheckToken(ref i);

            HttpResponseMessage response = await _httpClient.GetAsync($"{_baseUrl}{postId}");
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();

            CheckToken(ref i);

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            var post = JsonSerializer.Deserialize<Post>(json, options);

            CheckToken(ref i);

            Console.WriteLine($"End Get post {postId}. Timeout {rnd}");

            return post;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }

        void CheckToken(ref int count)
        {
            count++;
            if (token.IsCancellationRequested)
            {
                throw new TaskCanceledException(
                    $"Task Canceled. Post {postId}. Timeout {rnd}. Count {count}");
            }
        }
    }

    internal static void Save(Post post)
    {
        File.AppendAllLines(_path, post.Content);
        File.AppendAllText(_path, Environment.NewLine);
    }

    internal static void ReWriteFile()
    {
        if (File.Exists(_path))
        {
            File.Delete(_path);
        }
    }
}