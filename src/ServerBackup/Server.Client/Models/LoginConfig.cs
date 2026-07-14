namespace OroIdentityServer.Server.Client.Models;

public sealed class LoginConfig
{
    public string BackgroundType { get; set; } = "color";
    public string BackgroundValue { get; set; } = "#F7F7F6";
    public string FormBackground { get; set; } = "#FFFFFF";
    public string FormPosition { get; set; } = "center";
    public string AccentColor { get; set; } = "#111111";
    public string TextColor { get; set; } = "#111111";
    public string TextSecondaryColor { get; set; } = "#777777";
    public string FormBorderColor { get; set; } = "#ECECEC";
    public int FormWidth { get; set; } = 400;

    public string ToCss()
    {
        var bg = BackgroundType switch
        {
            "gradient" => $"linear-gradient(135deg, {BackgroundValue})",
            "image" => $"url({BackgroundValue}) center/cover no-repeat fixed",
            _ => BackgroundValue
        };
        var justify = FormPosition switch
        {
            "left" => "flex-start",
            "right" => "flex-end",
            _ => "center"
        };
        return $"--login-bg:{bg};--login-form-bg:{FormBackground};--login-justify:{justify};--login-accent:{AccentColor};--login-text:{TextColor};--login-text-secondary:{TextSecondaryColor};--login-border:{FormBorderColor};--login-width:{FormWidth}px;";
    }
}
