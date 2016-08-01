using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace HunieBot.Danbooru.API
{
    public sealed class BooruApi : IDisposable
    {
        private readonly string _username;
        private readonly string _apiKey;
        private readonly HttpClient _client;
        private bool _disposedValue = false; // To detect redundant calls



        public BooruApi(string name, string key)
        {
            _username = name;
            _apiKey = key;
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_username}:{_apiKey}")));
        }


        public async Task<PostsResponse> Search(int limit, params string[] tags)
        {
            var formattedUrl = $"https://danbooru.donmai.us/posts.json?limit={limit}&page=1&tags={string.Join("%20", tags)}";
            var respObj = await _client.GetAsync(formattedUrl);
            var strResponse = await respObj.Content.ReadAsStringAsync();
            return await Task.Run(() => JsonConvert.DeserializeObject<PostsResponse>(strResponse));
        }


        #region IDisposable Support

        void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~BooruApi() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }



    public class PostsResponse
    {
        public List<Post> Responses { get; set; }
    }

    public class Post
    {
        public int id { get; set; }
        public DateTime created_at { get; set; }
        public int uploader_id { get; set; }
        public int score { get; set; }
        public string source { get; set; }
        public string md5 { get; set; }
        public DateTime? last_comment_bumped_at { get; set; }
        public string rating { get; set; }
        public int image_width { get; set; }
        public int image_height { get; set; }
        public string tag_string { get; set; }
        public bool is_note_locked { get; set; }
        public int fav_count { get; set; }
        public string file_ext { get; set; }
        public DateTime? last_noted_at { get; set; }
        public bool is_rating_locked { get; set; }
        public int? parent_id { get; set; }
        public bool has_children { get; set; }
        public int? approver_id { get; set; }
        public int tag_count_general { get; set; }
        public int tag_count_artist { get; set; }
        public int tag_count_character { get; set; }
        public int tag_count_copyright { get; set; }
        public int file_size { get; set; }
        public bool is_status_locked { get; set; }
        public string fav_string { get; set; }
        public string pool_string { get; set; }
        public int up_score { get; set; }
        public int down_score { get; set; }
        public bool is_pending { get; set; }
        public bool is_flagged { get; set; }
        public bool is_deleted { get; set; }
        public int tag_count { get; set; }
        public DateTime updated_at { get; set; }
        public bool is_banned { get; set; }
        public int? pixiv_id { get; set; }
        public DateTime? last_commented_at { get; set; }
        public bool has_active_children { get; set; }
        public int bit_flags { get; set; }
        public string uploader_name { get; set; }
        public bool has_large { get; set; }
        public string tag_string_artist { get; set; }
        public string tag_string_character { get; set; }
        public string tag_string_copyright { get; set; }
        public string tag_string_general { get; set; }
        public bool has_visible_children { get; set; }
        public string file_url { get; set; }
        public string large_file_url { get; set; }
        public string preview_file_url { get; set; }
    }


}
