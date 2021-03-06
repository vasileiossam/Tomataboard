﻿using System.Runtime.Serialization;

namespace Tomataboard.Services.Photos.Api500px.Models
{
    [DataContract]
    public class Image
    {
        #region Public Properties

        [DataMember(Name = "format")]
        public string Format { get; set; }

        [DataMember(Name = "https_url")]
        public string HttpsUrl { get; set; }

        [DataMember(Name = "size")]
        public int Size { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }

        #endregion Public Properties
    }
}