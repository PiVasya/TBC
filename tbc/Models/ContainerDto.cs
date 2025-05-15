namespace TBC.Models
{
    public class ContainerDto
    {
        public string Id { get; set; }      // Docker ID
        public string Name { get; set; }    // имя контейнера (без ведущего '/')
        public string Image { get; set; }   // образ
        public string Status { get; set; }  // состояние: running, exited и т.п.
    }
}
