namespace MenuDisplayApp.Models
{
    public class MenuGroup
    {
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public List<MenuItem> Items { get; set; }
    }
}
