using System.Net.Http;
using System.Threading.Tasks;

namespace v4Sample_AADv2
{
    public static class Extensions
    {
        public static async Task<PhotoResponse> GetStreamWithAuthAsync(this HttpClient client, string accessToken, string endpoint)
        {
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            using (var response = await client.GetAsync(endpoint))
            {
                if (response.IsSuccessStatusCode)
                {
                    
                    var stream = await response.Content.ReadAsStreamAsync();
                    byte[] bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, (int)stream.Length);
                    var photoResponse = new PhotoResponse();
                    photoResponse.Bytes = bytes;
                    photoResponse.ContentType = response.Content.Headers.ContentType.ToString();
                    return photoResponse;
                }
                else
                    return null;
            }
        }
    }

    public class PhotoResponse
    {
        public byte[] Bytes { get; set; }
        public string ContentType { get; set; }
        public string Base64string { get; set; }
    }
}
