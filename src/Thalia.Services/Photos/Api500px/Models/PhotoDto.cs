﻿using System;
using System.Runtime.Serialization;
using Thalia.Services.Photos.Api500px.Intefaces;
using System.Linq;

namespace Thalia.Services.Photos.Api500px.Models
{
    [DataContract]
    public class PhotoDto : IPhoto
    {
        [DataMember(Name = "aperture")]
        public string Apeture { get; set; }

        [DataMember(Name = "camera")]
        public string Camera { get; set; }

        [DataMember(Name = "category")]
        public int Category { get; set; }

        [DataMember(Name = "created_at")]
        public DateTime Created { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "favorited")]
        public bool Favorited { get; set; }

        [DataMember(Name = "favorites_count")]
        public int Favorites { get; set; }

        [DataMember(Name = "focal_length")]
        public string FocalLength { get; set; }

        [DataMember(Name = "highest_rating")]
        public double HighestRating { get; set; }

        [DataMember(Name = "id")]
        public long Id { get; set; }

        [DataMember(Name = "images")]
        public ImageCollection Images { get; set; }

        [DataMember(Name = "iso")]
        public string Iso { get; set; }

        [DataMember(Name = "latitude")]
        public double? Latitude { get; set; }

        [DataMember(Name = "lens")]
        public string Lens { get; set; }

        [DataMember(Name = "location")]
        public string Location { get; set; }

        [DataMember(Name = "longitude")]
        public double? Longitude { get; set; }

        [DataMember(Name = "voted")]
        public bool Liked { get; set; }

        [DataMember(Name = "positive_votes_count")]
        public int Likes { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "rating")]
        public double Rating { get; set; }

        [DataMember(Name = "shutter_speed")]
        public string ShutterSpeed { get; set; }

        [DataMember(Name = "taken_at")]
        public DateTime? Taken { get; set; }

        [DataMember(Name = "times_viewed")]
        public int Views { get; set; }

        [DataMember(Name = "user")]
        public User User { get; set; }

        #region Public Methods
        public string GetUrl()
        {
            var photo = Images.LastOrDefault();
            if (photo == null) return string.Empty;
            if (!string.IsNullOrEmpty(photo.Url)) return photo.Url;
            if (!string.IsNullOrEmpty(photo.HttpsUrl)) return photo.Url;
            return string.Empty;
        }

        public string GetAuthorProfilePage()
        {
            return $"https://500px.com/{User.UserName}/";
        }

        public string GetPhotoPage()
        {
            return $"https://500px.com/photo/{Id}/";
        }
        #endregion
    }
}
