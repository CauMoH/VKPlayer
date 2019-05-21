using VKPlayer.Enums;

namespace VKPlayer.Data
{
    public class Genre
    {
        public AudioGenreExt AudioGenreExt { get; set; }
        public string Description { get; set; }

        public Genre(AudioGenreExt audioGenreExt, string description)
        {
            AudioGenreExt = audioGenreExt;
            Description = description;
        }
    }
}
