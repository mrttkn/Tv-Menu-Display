namespace MenuDisplayApp.Dtos
{
    public class MenuGroupDto
    {
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public List<MenuItemDto> Items { get; set; }
    }
}
