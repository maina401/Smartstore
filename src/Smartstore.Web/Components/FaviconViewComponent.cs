﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Smartstore.Core.Content.Media;
using Smartstore.Web.Infrastructure.Hooks;
using Smartstore.Web.Models.Common;

namespace Smartstore.Web.Components
{
    public class FaviconViewComponent : SmartViewComponent
    {
        private readonly IMediaService _mediaService;

        public FaviconViewComponent(IMediaService mediaService)
        {
            _mediaService = mediaService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var store = Services.StoreContext.CurrentStore;
            
            var model = await Services.Cache.GetAsync(ModelCacheInvalidator.STORE_FAVICON_MODEL_KEY.FormatInvariant(store.Id), async (o) =>
            {
                o.ExpiresIn(TimeSpan.FromDays(1));

                var model = new FaviconModel();

                // Favicon
                model.FavIconThumbnail = await _mediaService.GetUrlAsync(store.FavIconMediaFileId, 0, null, false);

                // PngIcon 16x16(browser tabs), 32x32(Taskbar), 96x96(Desktop), 196x196(Android Chrome) 
                var pngIcon = await _mediaService.GetFileByIdAsync(Convert.ToInt32(store.PngIconMediaFileId), MediaLoadFlags.AsNoTracking);
                if (pngIcon != null)
                {
                    var pngSizes = new[] { 16, 32, 96, 196 };
                    AddThumbnails(pngIcon, pngSizes, model.PngIconThumbnails);
                }

                // AppleTouchIcon
                var appleTouchIcon = await _mediaService.GetFileByIdAsync(Convert.ToInt32(store.AppleTouchIconMediaFileId), MediaLoadFlags.AsNoTracking);
                if (appleTouchIcon != null)
                {
                    var appleTouchSizes = new[] { 57, 60, 72, 76, 114, 120, 144, 152, 180 };
                    AddThumbnails(appleTouchIcon, appleTouchSizes, model.AppleTouchIconThumbnails);
                }

                // MS Tiles
                model.MsTileIconThumbnail = await _mediaService.GetUrlAsync(store.MsTileImageMediaFileId, 144, null, false);
                model.MsTileColor = store.MsTileColor;

                return model;
            });

            return View(model);
        }

        private void AddThumbnails(MediaFileInfo file, int[] sizes, List<FaviconThumbnail> urls)
        {
            foreach (var size in sizes)
            {
                urls.Add(new FaviconThumbnail
                {
                    Size = $"{size}x{size}",
                    Url = _mediaService.GetUrl(file, size, null, false)
                });
            }
        }
    }
}
