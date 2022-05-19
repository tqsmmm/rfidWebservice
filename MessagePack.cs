namespace rfidWebservice
{
    public class MessagePack
    {
        public MessagePack()
        {
            Code = -9999;
            Message = "";
            Content = null;
            Result = false;
        }

        public int Code { get; set; }
        public string Message { get; set; }
        public object Content { get; set; }
        public bool Result { get; set; }
    }
}