using System.Text.Json.Serialization;

namespace WebshopMobileApp.Models
{
    //public class TblPromoPicturesSet
    //{
    //    public int Id { get; set; }
    //    public byte[]? PromoPic { get; set; }
    //    public int SlotNo { get; set; }
    //    public bool isVideo { get; set; }
    //    public int tblPromoPics_TblPromoPicHeaders { get; set; }
    //    public string Url { get; set; } = "";
    //}

    public class TblPromoPicturesSet
    {
        [JsonPropertyName("Id")]
        public int Id { get; set; }

        [JsonPropertyName("PromoPic")]
        public byte[]? PromoPic { get; set; }

        [JsonPropertyName("SlotNo")]
        public int SlotNo { get; set; }

        [JsonPropertyName("isVideo")]
        public bool IsVideo { get; set; }

        [JsonPropertyName("tblPromoPics_TblPromoPicHeaders")]
        public int TblPromoPics_TblPromoPicHeaders { get; set; }

        [JsonPropertyName("Url")]
        public string Url { get; set; } = string.Empty;
    }
}
