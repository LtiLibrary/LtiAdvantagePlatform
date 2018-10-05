namespace AdvantagePlatform.Data
{
    public class Deployment
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Client Client { get; set; }
        public Tool Tool { get; set; }
    }
}
