﻿using Twitch.Api.Models;

namespace Jadisco.UWP.ViewModels
{
    public class StreamQualityViewModel : BaseViewModel
    {
        private string _name;

        public string Name
        {
            get => _name;
            set { _name = value; NotifyPropertyChanged(); }
        }

        public HLSStream HLSStream { get; set; }
    }
}