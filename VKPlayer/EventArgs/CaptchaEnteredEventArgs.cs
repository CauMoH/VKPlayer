namespace VKPlayer.EventArgs
{
    public sealed class CaptchaEnteredEventArgs : System.EventArgs
    {
        public CaptchaEnteredEventArgs(long captchaSid, string captchaKey)
        {
            CaptchaSid = captchaSid;
            CaptchaKey = captchaKey;
        }

        public long CaptchaSid { get; }

        public string CaptchaKey { get; }
    }
}
