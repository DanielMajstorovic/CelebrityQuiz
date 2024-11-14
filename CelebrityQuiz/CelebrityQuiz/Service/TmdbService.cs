using CelebrityQuiz.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using System.Windows;
using System.Net.Http.Headers;
using System.Diagnostics;

namespace CelebrityQuiz.Service
{
    public class TmdbService
    {
        public async Task<Actor> GetRandomActorAsync()
        {
            using HttpClient client = new HttpClient();
            //var response = await client.GetStringAsync($"https://api.themoviedb.org/3/person/popular?api_key={ConfigurationManager.AppSettings["TMDB_API_KEY"]}");

            //var json = JObject.Parse(response);

            client.BaseAddress = new Uri("https://api.themoviedb.org/3/");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
     ConfigurationManager.AppSettings["TMDB_API_KEY"]);

            int randomPage = new Random().Next(1, 30);
            var response = await client.GetAsync($"person/popular?language=en-US&page={randomPage}");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);



            var results = json["results"];

            if (results != null)
            {
                var random = new Random();
                var actorData = results[random.Next(results.Count())];
                return new Actor
                (
                    (int)actorData["id"],
                    (string)actorData["name"],
                    $"https://image.tmdb.org/t/p/w500{actorData["profile_path"]}"
                );
            }

            return null;
        }




        public async Task<List<Actor>> GetRandomActorsAsync(int maxIterations = 300)
        {
            List<Actor> actors = new List<Actor>();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            using HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://api.themoviedb.org/3/");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                ConfigurationManager.AppSettings["TMDB_API_KEY"]);

            int iterations = 0;
            Random random = new Random();

            while (iterations < maxIterations && actors.Count < maxIterations)
            {
                int randomPage = random.Next(1, 501);
                var response = await client.GetAsync($"person/popular?language=en-US&page={randomPage}");

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(content);

                var results = json["results"];

                if (results != null)
                {
                    // Smanji broj slučajnih odabira iz rezultata kako bi se poboljšala brzina
                    int maxResultsToCheck = Math.Min(results.Count(), 20); // Provjeri maksimalno 20 glumaca po stranici
                    for (int i = 0; i < maxResultsToCheck; i++)
                    {
                        var actorData = results[random.Next(maxResultsToCheck)];
                        var actor = new Actor(
                            (int)actorData["id"],
                            (string)actorData["name"],
                            $"https://image.tmdb.org/t/p/w500{actorData["profile_path"]}"
                        );

                        if (!actors.Contains(actor)) // Provjera duplikata
                        {
                            actors.Add(actor);
                            iterations++; // Povećaj brojač iteracija samo ako je glumac dodan
                            if (actors.Count == maxIterations) // Prekini petlju ako je dohvaćeno dovoljno glumaca
                            {
                                break;
                            }
                        }
                    }
                }
            }

            stopwatch.Stop();

            MessageBox.Show($"Dohvaćeno je {actors.Count} glumaca za {stopwatch.ElapsedMilliseconds} milisekundi.");

            return actors;
        }



    }
}
