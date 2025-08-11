namespace EventRegistrator.Application
{
    public class AntwortDTO
    {
        public long ChatId { get; set; }
        public string Text { get; set; }
        public int? MessageToEditId { get; set; }
        public int? MessageToReplyId { get; set; }
        public (string, string)? ButtonData { get; set; }
        public bool Like { get; set; }
        public bool UnLike { get; set; }
        public Action<int> SaveMessageIdCallback { get; set; }
    }
}
