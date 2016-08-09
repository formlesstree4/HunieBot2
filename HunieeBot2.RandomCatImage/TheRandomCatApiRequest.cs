using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RandomCatImage
{

    /// <summary>
    /// Provides a URL for GET requests
    /// </summary>
    public interface IApiGetRequest
    {
        /// <summary>
        /// The full URL of this request including arguments
        /// </summary>
        string RequestUrl { get; }
    }

    /// <summary>
    /// This'll get you all the fancy cat Images you can handle
    /// </summary>
    /// <remarks>
    /// Important: To protect yourself from Copyright issues, when you show an Image you MUST have it linked to; or clearly show the 'source_url' the value of the XML,or leave the link in-place with HTML format. This then links back to the image on thecatapi.com, where we then link to where we do the same to the last known Copyright holder.
    /// </remarks>
    /// <example>
    /// This'll return you xml data for 20 random cats.
    /// <code>
    /// http://thecatapi.com/api/images/get?format=xml&results_per_page=20
    /// </code>
    /// </example>
    public class GetCat : IApiGetRequest
    {

        /// <summary>
        /// Passing this gives you access to all the images, voting, favouriting, uploading etc
        /// </summary>
        public string api_key { get; set; }

        /// <summary>
        /// Unique id of the Image to return. Will only ever return one Image
        /// </summary>
        public string image_id { get; set; }

        /// <summary>
        /// The output format, as XML, an HTML img tag, or the src to use in an img tag.	
        /// </summary>
        public string format { get; set; } = "xml";

        /// <summary>
        /// The number of Cats to respond with. If format is set to src then only one will be returned.	
        /// </summary>
        public int results_per_page { get; set; }

        /// <summary>
        /// A comma separated string of file types to return. e.g. jpg,png
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// Filter the Cats returned to those wearing hats, in space, in boxes etc.
        /// </summary>
        public int category { get; set; }

        /// <summary>
        /// Size of returned Images, small = 250x, med = 500x, full = original size.
        /// </summary>
        public string size { get; set; }

        /// <summary>
        /// Passing this will return the value of any Favourite or Votes set with the same sub_id for the image.
        /// </summary>
        public string sub_id { get; set; }

        public string RequestUrl
        {
            get
            {
                var sb = new StringBuilder("http://thecatapi.com/api/images/get?");
                var paramList = new List<string>();

                if(!string.IsNullOrWhiteSpace(api_key)) paramList.Add($"api_key={api_key}");
                if(!string.IsNullOrWhiteSpace(image_id)) paramList.Add($"image_id={image_id}");
                if(!string.IsNullOrWhiteSpace(format)) paramList.Add($"format={format}");
                if(results_per_page > 1) paramList.Add($"results_per_page={results_per_page}");
                if(!string.IsNullOrWhiteSpace(type)) paramList.Add($"type={type}");
                if(category > 0) paramList.Add($"category={category}");
                if(!string.IsNullOrWhiteSpace(size)) paramList.Add($"size={size}");
                if(!string.IsNullOrWhiteSpace(sub_id)) paramList.Add($"sub_id={sub_id}");

                if (!paramList.Any()) return sb.ToString();
                sb.Append(string.Join("&", paramList));

                return sb.ToString();
            }
        }
    }

    /// <summary>
    /// This lets you or one of your users score an image 1-10
    /// </summary>
    /// <example>
    /// This is an example of User 12345 giving Image bC24 a score of 10
    /// <code>
    /// http://thecatapi.com/api/images/vote?api_key=[YOUR-API-KEY]&sub_id=12345&image_id=bC24&score=10
    /// </code>
    /// </example>
    public class Vote : IApiGetRequest
    {
        public string api_key { get; set; }

        /// <summary>
        /// Unique id of the image to vote on.
        /// </summary>
        public string image_id { get; set; }

        /// <summary>
        /// The score you want to give to the image 1 = bad / 10 = good.
        /// </summary>
        public int score { get; set; }

        /// <summary>
        /// Any value you wish to associate with this vote, could be a unique id you have for one of your Users like a facebook id. This allows you to only have one vote on an Image per User.
        /// </summary>
        public string sub_id { get; set; }

        public string RequestUrl
        {
            get
            {
                var sb = new StringBuilder("http://thecatapi.com/api/images/vote?");

                // this is pretty much an all or nothing request. 
                // we're just going to assume all of these properties are set
                var paramList = new List<string>
                {
                    $"api_key={api_key}",
                    $"image_id={image_id}",
                    $"score={score}",
                    $"sub_id={sub_id}"
                };

                if (!paramList.Any()) return sb.ToString();
                sb.Append(string.Join("&", paramList));

                return sb.ToString();
            }
        }
    }

    /// <summary>
    /// This gets all the votes cast with your api_key
    /// </summary>
    /// <example>
    /// This is an example will return all the votes cast by user 12345
    /// <code>
    /// http://thecatapi.com/api/images/getvotes?api_key=[YOUR-API-KEY]&sub_id=12345
    /// </code>
    /// </example>
    public class GetVotes : IApiGetRequest
    {
        public string api_key { get; set; }

        /// <summary>
        /// If this is passed, then only votes cast with this sub_id will be returned. Useful to find out which votes have been cast by which of your Users.
        /// </summary>
        public string sub_id { get; set; }

        public string RequestUrl
        {
            get
            {
                var sb = new StringBuilder("http://thecatapi.com/api/images/getvotes?");

                // this is pretty much an all or nothing request. 
                // we're just going to assume all of these properties are set
                var paramList = new List<string>
                {
                    $"api_key={api_key}",
                    $"sub_id={sub_id}"
                };

                if (!paramList.Any()) return sb.ToString();
                sb.Append(string.Join("&", paramList));

                return sb.ToString();
            }
        }
    }

    /// <summary>
    /// This lets you or one of your Users favourite an Image
    /// </summary>
    /// <example>
    /// This is an example of User 12345 favouriting Image bC24
    /// <code>
    /// http://thecatapi.com/api/images/favourite?api_key=[YOUR-API-KEY]&sub_id=12345&image_id=bC24
    /// </code>
    /// This is an example of User 12345 un-favouriting Image bC24
    /// <code>
    /// http://thecatapi.com/api/images/favourite?api_key=[YOUR-API-KEY]&sub_id=12345&image_id=bC24&action=remove
    /// </code>
    /// </example>
    public class Favourite : IApiGetRequest
    {
        public string api_key { get; set; }

        /// <summary>
        /// Unique id of the image to favourite or un-favourite
        /// </summary>
        public string image_id { get; set; }

        /// <summary>
        /// This allows you to say whether to add a favourite or remove it
        /// </summary>
        public string action { get; set; }

        /// <summary>
        /// Any value you wish to associate with this, 
        /// could be a unique id you have for one of your Users like a facebook id. 
        /// This allows you to favourite this Image once per User.
        /// </summary>
        public string sub_id { get; set; }

        public string RequestUrl
        {
            get
            {
                var sb = new StringBuilder("http://thecatapi.com/api/images/favourite?");

                // this is pretty much an all or nothing request. 
                // we're just going to assume all of these properties are set
                var paramList = new List<string>
                {
                    $"api_key={api_key}",
                    $"image_id={image_id}",
                    $"sub_id={sub_id}",
                    $"action={action}"
                };

                if (!paramList.Any()) return sb.ToString();
                sb.Append(string.Join("&", paramList));

                return sb.ToString();
            }
        }
    }

    /// <summary>
    /// This gets all the favourites set with your api_key
    /// </summary>
    /// <example>
    /// This is an example will return all the Favourites for User 12345
    /// <code>
    /// http://thecatapi.com/api/images/getfavourites?api_key=[YOUR-API-KEY]&sub_id=12345
    /// </code>
    /// </example>
    public class GetFavourites : IApiGetRequest
    {
        public string api_key { get; set; }

        public string sub_id { get; set; }

        public string RequestUrl
        {
            get
            {
                var sb = new StringBuilder("http://thecatapi.com/api/images/getfavourites?");

                // this is pretty much an all or nothing request. 
                // we're just going to assume all of these properties are set
                var paramList = new List<string>
                {
                    $"api_key={api_key}",
                    $"action={sub_id}"
                };

                if (!paramList.Any()) return sb.ToString();
                sb.Append(string.Join("&", paramList));

                return sb.ToString();
            }
        }
    }

    /// <summary>
    /// This allows you to report an Image so it will not show up again when you make a get request with your api_key.
    /// </summary>
    /// <example>
    /// This will report Image 8 so it won't show up again for requests with your api_key.
    /// <code>
    /// http://thecatapi.com/api/images/report?api_key=[YOUR-API-KEY]&sub_id=12345&image_id=8
    /// </code>
    /// </example>
    public class Report : IApiGetRequest
    {
        public string api_key { get; set; }

        /// <summary>
        /// Unique id of the image to report
        /// </summary>
        public string image_id { get; set; }

        /// <summary>
        /// Any value you wish to associate with this, could be a unique id you have for one of your Users like a facebook id. This allows you to favourite this Image once per User.
        /// </summary>
        public string sub_id { get; set; }

        /// <summary>
        /// Any particular reason why it needs reporting, contains nothing but hippos, doesn't work etc
        /// </summary>
        public string reason { get; set; }

        public string RequestUrl
        {
            get
            {
                var sb = new StringBuilder("http://thecatapi.com/api/images/report?");

                // this is pretty much an all or nothing request. 
                // we're just going to assume all of these properties are set
                var paramList = new List<string>
                {
                    $"api_key={api_key}",
                    $"api_key={image_id}",
                    $"api_key={sub_id}",
                    $"action={reason}"
                };

                if (!paramList.Any()) return sb.ToString();
                sb.Append(string.Join("&", paramList));

                return sb.ToString();
            }
        }
    }

    /// <summary>
    /// Returns a list of all the active categories.
    /// </summary>
    /// <example>
    /// <code>
    /// http://thecatapi.com/api/categories/list
    /// </code>
    /// </example>
    public class Categories : IApiGetRequest
    {
        public string RequestUrl => "http://thecatapi.com/api/categories/list";
    }

    /// <summary>
    /// This returns the total get requests/ votes/ favourites/ and images served for your account.
    /// </summary>
    /// <example>
    /// This will return the overview of the stats for your account.
    /// <code>
    /// http://thecatapi.com/api/stats/getoverview?api_key=[YOUR-API-KEY]
    /// </code>
    /// </example>
    public class Stats : IApiGetRequest
    {
        public string api_key { get; set; }

        public string RequestUrl => $"http://thecatapi.com/api/stats/getoverview?api_key={api_key}";
    }
}
