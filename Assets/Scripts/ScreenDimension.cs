public class ScreenDimension
{
    private float _valueInMillimeters;

    public ScreenDimension(int pixels)
    {
        this.pixels = pixels;
    }

    public ScreenDimension(float millimeters)
    {
        this.millimeters = millimeters;
    }

    public float millimeters
    {
        get { return _valueInMillimeters; }
        set { _valueInMillimeters = value; }
    }

    public int pixels
    {
        get { return ToPixels(_valueInMillimeters);  }
        set { _valueInMillimeters = ToMillimeters(value); }
    }               

    public static float ToMillimeters(int pixels)
    {
        return pixels / SharedData.currentConfiguration.screenPixelsPerMillimeter;
    }

    public static int ToPixels(float millimeters)
    {
        return (int)(millimeters / SharedData.currentConfiguration.screenPixelsPerMillimeter);
    }
}