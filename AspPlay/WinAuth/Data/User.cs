namespace WinAuth.Data
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
    }

    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}