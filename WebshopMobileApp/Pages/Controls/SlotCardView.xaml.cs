using WebshopMobileApp.Models;

namespace WebshopMobileApp.Pages.Controls;

public partial class SlotCardView : ContentView
{
	public SlotCardView()
	{
		InitializeComponent();
        this.BindingContextChanged += VideoCardView_BindingContextChanged;
    }

    private void LoadVideo(string url)
    {
        // Simple HTML5 video player
        string html = $@"
        <html>
            <body style='margin:0;padding:0;'>
                <video  poster='https://img.youtube.com/vi/alg9ydZDre0/maxresdefault.jpg' style='width: 100%; height: 300px; object-fit: cover;' controls preload='metadata'>
                    <source src='{url}' type='video/mp4'>
                </video>
            </body>
        </html>";

        VideoWebView.Source = new HtmlWebViewSource { Html = html };
    }

    private void VideoCardView_BindingContextChanged(object sender, EventArgs e)
    {
        if (BindingContext is TblPromoPicturesSet item)
            LoadVideo($"https://orders.lumarfoods.co.za:20603/Video/ProBan{item.SlotNo}.mp4");
    }
}