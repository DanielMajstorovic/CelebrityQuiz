using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CelebrityQuiz.Model
{
    public class Actor
    {
        public Actor(int id, string name, string imageUrl)
        {
            Id = id;
            Name = name;
            ImageUrl = imageUrl;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
    }
}
