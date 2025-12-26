namespace _0_Framework.Application
{
    public class VerifySendParameterModel
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class VerifySendModel
    {
        public string Mobile { get; set; }
        public int TemplateId { get; set; }
        public VerifySendParameterModel[] Parameters { get; set; }
    }
}
