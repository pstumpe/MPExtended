﻿#region Copyright (C) 2011-2012 MPExtended
// Copyright (C) 2011-2012 MPExtended Developers, http://mpextended.github.com/
// 
// MPExtended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MPExtended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MPExtended. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Web.Mvc;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Applications.WebMediaPortal.Models;
using MPExtended.Libraries.Client;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;
using MPExtended.Services.StreamingService.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Controllers
{
    [ServiceAuthorize]
    public class TVShowsLibraryController : BaseController
    {
        public ActionResult Index()
        {
            var series = MPEServices.MAS.GetAllTVShowsBasic(Settings.ActiveSettings.TVShowProvider);
            if (series != null)
            {
                return View(series);
            }
            return null;
        }

        public ActionResult Seasons(string show)
        {
            var showObj = MPEServices.MAS.GetTVShowBasicById(Settings.ActiveSettings.TVShowProvider, show);
            var seasons = MPEServices.MAS.GetTVSeasonsBasicForTVShow(Settings.ActiveSettings.TVShowProvider, show, SortBy.TVSeasonNumber, OrderBy.Asc);
            return View(new TVShowViewModel()
            {
                Show = showObj,
                Seasons = seasons
            });
        }

        public ActionResult Episodes(string season)
        {
            var seasonObj = MPEServices.MAS.GetTVSeasonBasicById(Settings.ActiveSettings.TVShowProvider, season);
            var showObj = MPEServices.MAS.GetTVShowBasicById(Settings.ActiveSettings.TVShowProvider, seasonObj.ShowId);
            var episodes = MPEServices.MAS.GetTVEpisodesBasicForSeason(Settings.ActiveSettings.TVShowProvider, season, SortBy.TVEpisodeNumber, OrderBy.Asc);
            return View(new TVSeasonViewModel()
            {
                Season = seasonObj,
                Show = showObj,
                Episodes = episodes
            });
        }

        public ActionResult Image(string season, int width = 0, int height = 0)
        {
            var image = MPEServices.MASStream.GetArtworkResized(WebStreamMediaType.TVSeason, Settings.ActiveSettings.TVShowProvider, season, WebArtworkType.Banner, 0, width, height);
            if (image != null)
            {
                return File(image, "image/jpg");
            }
            return null;
        }

        public ActionResult EpisodeImage(string episode, int width = 0, int height = 0)
        {
            var image = MPEServices.MASStream.GetArtworkResized(WebStreamMediaType.TVEpisode, Settings.ActiveSettings.TVShowProvider, episode, WebArtworkType.Banner, 0, width, height);
            if (image != null)
            {
                return File(image, "image/jpg");
            }
            return null;
        }

        public ActionResult SeriesFanart(string show, int width = 0, int height = 0)
        {
            var image = MPEServices.MASStream.GetArtworkResized(WebStreamMediaType.TVShow, Settings.ActiveSettings.TVShowProvider, show, WebArtworkType.Backdrop, 0, width, height);
            if (image != null)
            {
                return File(image, "image/jpg");
            }
            return null;
        }

        public ActionResult Details(string episode)
        {
            var fullEpisode = MPEServices.MAS.GetTVEpisodeDetailedById(Settings.ActiveSettings.TVShowProvider, episode);
            if (fullEpisode == null)
                return HttpNotFound();

            var fileInfo = MPEServices.MAS.GetFileInfo(fullEpisode.PID, WebMediaType.TVEpisode, WebFileType.Content, fullEpisode.Id, 0);
            var mediaInfo = MPEServices.MASStreamControl.GetMediaInfo(WebStreamMediaType.TVEpisode, fullEpisode.PID, fullEpisode.Id);
            ViewBag.ShowPlay = fullEpisode.Path != null;
            ViewBag.Quality = MediaInfoFormatter.GetFullInfoString(mediaInfo, fileInfo);
            return View(fullEpisode);
        }

        public ActionResult Play(string episode)
        {
            var fullEpisode = MPEServices.MAS.GetTVEpisodeDetailedById(Settings.ActiveSettings.TVShowProvider, episode);
            if (fullEpisode != null)
            {
                return View(new TVEpisodeViewModel()
                {
                    Episode = fullEpisode,
                    Show = MPEServices.MAS.GetTVShowDetailedById(fullEpisode.PID, fullEpisode.ShowId),
                    Season = MPEServices.MAS.GetTVSeasonDetailedById(fullEpisode.PID, fullEpisode.SeasonId)
                });
            }
            return null;
        }
    }
}
