namespace GridTactics
{
    public static class Extensions
    {
        public static Rectangle Lerp(this Rectangle startWindow, Rectangle endWindow, float transitionProgress)
        {
            return new Rectangle((int)MathHelper.Lerp(startWindow.X, endWindow.X, transitionProgress),
                                                      (int)MathHelper.Lerp(startWindow.Y, endWindow.Y, transitionProgress),
                                                      (int)MathHelper.Lerp(startWindow.Width, endWindow.Width, transitionProgress),
                                                      (int)MathHelper.Lerp(startWindow.Height, endWindow.Height, transitionProgress));
        }
    }
}
