using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Glass.Mapper.Sc;
using Glass.Mapper.Sc.Web.Mvc;
using Newtonsoft.Json.Linq;
using RAC.Feature.LearnArticles.Models.Articles;
using RAC.Feature.LearnArticles.Models.Learn;
using RAC.Feature.LearnArticles.ViewModels;
using RAC.Foundation.GlassMapper.Extensions;
using RAC.Foundation.Infrastructure.CustomAttributes;
using RAC.Foundation.SitecoreModels.Sitecore.Templates.RAC_Website.Base;
using RAC.Foundation.SitecoreModels.Sitecore.Templates.RAC_Website.Data_Items.Navigation_Tiles;
using RAC.Foundation.SitecorePageModels.Sitecore.Templates.RAC_Website.Base;
using RAC.Foundation.SitecorePageModels.Sitecore.Templates.RAC_Website.Pages;
using RAC.Foundation.Tagging.Models.Sitecore.Templates.RAC_Website.Data_Items;
using RAC.Foundation.Tagging.Models.Sitecore.Templates.RAC_Website.Base;
using RAC.Foundation.Tagging.Services;
using RAC.Foundation.Tagging.ViewModels;
using Sitecore.Data;
using Sitecore.Mvc.Presentation;
using System.Collections;
using Sitecore.Data.Items;

namespace RAC.Feature.LearnArticles.Controllers
{
    public class ArticleApiController : Controller
    {
        private readonly IMvcContext _mvcContext;
        private MapperConfiguration cfg;
        private IMapper mapper;
        private readonly ITagService _tagService;

        public ArticleApiController(IMvcContext mvcContext, ITagService tagService)
        {
            _mvcContext = mvcContext;
            _tagService = tagService;

            #region Auto Mapper
            cfg = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Tag, TagViewModel>();

                cfg.CreateMap<ArticlePage, ArticleNavigationTile>()
                    .ForMember(x => x.Title, y => y.MapFrom(z => z.RawTitle))
                    .ForMember(x => x.ShortDescription, y => y.MapFrom(z => z.ShortDescription))
                    .ForMember(x => x.BannerImage, y => y.MapFrom(z => z.BannerImage))
                    .ForMember(x => x.TileImage, y => y.MapFrom(z => z.TileImage))
                    .ForMember(x => x.SortOrder, y => y.MapFrom(z => z.Sortorder))
                    .ForMember(x => x.Type, y => y.UseValue("Article"))
                    .ForMember(x => x.Url, y => y.MapFrom(z => z.Url))
                    .ForMember(x => x.Price, y => y.Ignore())
                    .ForMember(x => x.PriceIntroText, y => y.Ignore())
                    .ForMember(x => x.PriceOutroText, y => y.Ignore())
                    .ForMember(x => x.StarRating, y => y.Ignore())
                    .ForMember(x => x.QuickInfo, y => y.Ignore())
                    .ForMember(x => x.CTAText, y => y.Ignore())
                    .ForMember(x => x.NumberOfSeries, y => y.Ignore())
                    .ForMember(x => x.Tags, y => y.MapFrom(m => m.Tags));

                cfg.CreateMap<LearnPage, ArticleNavigationTile>()
                    .ForMember(x => x.Title, y => y.MapFrom(z => z.RawTitle))
                    .ForMember(x => x.ShortDescription, y => y.MapFrom(z => z.ShortDescription))
                    .ForMember(x => x.BannerImage, y => y.Ignore())
                    .ForMember(x => x.TileImage, y => y.Ignore())
                    .ForMember(x => x.SortOrder, y => y.MapFrom(z => z.Sortorder))
                    .ForMember(x => x.Type, y => y.UseValue("Learn"))
                    .ForMember(x => x.Url, y => y.MapFrom(z => z.Url))
                    .ForMember(x => x.Price, y => y.Ignore())
                    .ForMember(x => x.PriceIntroText, y => y.Ignore())
                    .ForMember(x => x.PriceOutroText, y => y.Ignore())
                    .ForMember(x => x.StarRating, y => y.Ignore())
                    .ForMember(x => x.QuickInfo, y => y.Ignore())
                    .ForMember(x => x.CTAText, y => y.Ignore())
                    .ForMember(x => x.NumberOfSeries, y => y.Ignore())
                    .ForMember(x => x.Tags, y => y.MapFrom(m => m.Tags));

                cfg.CreateMap<SeriesItemPageBase, ArticleNavigationTile>()
                .ForMember(x => x.Title, y => y.MapFrom(z => z.RawTitle))
                .ForMember(x => x.ShortDescription, y => y.MapFrom(z => z.ShortDescription))
                .ForMember(x => x.BannerImage, y => y.MapFrom(z => z.BannerImage))
                .ForMember(x => x.TileImage, y => y.MapFrom(z => z.TileImage))
                .ForMember(x => x.SortOrder, y => y.MapFrom(z => z.Sortorder))
                .ForMember(x => x.Type, y => y.MapFrom(z => GetSeriesItemType(z.TemplateId.ToString())))
                .ForMember(x => x.Url, y => y.MapFrom(z => z.Url))
                .ForMember(x => x.Price, y => y.Ignore())
                .ForMember(x => x.PriceIntroText, y => y.Ignore())
                .ForMember(x => x.PriceOutroText, y => y.Ignore())
                .ForMember(x => x.StarRating, y => y.Ignore())
                .ForMember(x => x.QuickInfo, y => y.Ignore())
                .ForMember(x => x.CTAText, y => y.Ignore())
                .ForMember(x => x.NumberOfSeries, y => y.Ignore())
                .ForMember(x => x.Tags, y => y.MapFrom(m => m.Tags));

                cfg.CreateMap<SeriesPage, ArticleNavigationTile>()
                    .ForMember(x => x.Title, y => y.MapFrom(z => z.RawSeriesTitle))
                    .ForMember(x => x.ShortDescription, y => y.MapFrom(z => z.ShortDescription))
                    .ForMember(x => x.BannerImage, y => y.MapFrom(z => z.BannerImage))
                    .ForMember(x => x.TileImage, y => y.MapFrom(z => z.TileImage))
                    .ForMember(x => x.SortOrder, y => y.MapFrom(z => z.Sortorder))
                    .ForMember(x => x.Type, y => y.UseValue("Series"))
                    .ForMember(x => x.Url, y => y.MapFrom(z => z.Url))
                    .ForMember(x => x.Price, y => y.Ignore())
                    .ForMember(x => x.PriceIntroText, y => y.Ignore())
                    .ForMember(x => x.PriceOutroText, y => y.Ignore())
                    .ForMember(x => x.StarRating, y => y.Ignore())
                    .ForMember(x => x.QuickInfo, y => y.Ignore())
                    .ForMember(x => x.CTAText, y => y.Ignore())
                    .ForMember(x => x.NumberOfSeries, y => y.MapFrom(z => z.NumberOfArticlesInSeries))
                    .ForMember(x => x.Tags, y => y.MapFrom(m => m.Tags));

                cfg.CreateMap<NavigationTilesModule, ArticleNavigationTilesView>()
                    .ForMember(x => x.ModuleTitle, y => y.MapFrom(z => z.ModuleTitle))
                    .ForMember(x => x.ModuleSubTitle, y => y.MapFrom(z => z.ModuleSubTitle))
                    .ForMember(x => x.ModuleFooter, y => y.MapFrom(z => z.ModuleFooter))
                    .ForMember(x => x.BackgroundColour, y => y.MapFrom(z => z.BackgroundColour))
                    .ForMember(x => x.ModuleNarrow, y => y.MapFrom(z => z.ModuleNarrow))
                    .ForMember(x => x.BreakoutBox, y => y.MapFrom(z => z.BreakoutBox))
                    .ForMember(x => x.ItemId, y => y.MapFrom(z => z.ItemId))
                    .ForMember(x => x.ItemName, y => y.MapFrom(z => z.ItemName))
                    .ForMember(x => x.ItemName, y => y.MapFrom(z => z.ItemName))
                    .ForMember(x => x.Path, y => y.MapFrom(z => z.Path))
                    .ForMember(x => x.Tiles, y => y.Ignore())
                    .ForMember(x => x.PageSize, y => y.MapFrom(z => int.Parse(z.PageSize)))
                    .ForMember(x => x.ShowTags, y => y.MapFrom(z => z.ShowTags))
                    .ForMember(x => x.NumberTags, y => y.MapFrom(z => z.NumberTags));
            });
            #endregion
            mapper = cfg.CreateMapper();

        }

        public ActionResult RenderArticleLearn()
        {
            var datasource = RenderingContext.Current.Rendering.DataSource;
            return View(new ArticleNavigationTilesViewModel() { Datasource = datasource });
        }

        [DisableTracking]
        public JObject GetTabById(string tagId)
        {
            ID result;
            if (ID.TryParse(tagId, out result))
            {
                var tag = _mvcContext.SitecoreService.GetItem<Tag>(tagId);
                return JObject.FromObject(tag);
            }
            return null;
        }

        [DisableTracking]
        public JArray GetArticlesByTagId(string tagId)
        {
            ID result;
            if (ID.TryParse(tagId, out result))
            {
                return JArray.FromObject(_tagService.GetItemsByTagId<ArticlePage>(result, ID.Parse(_mvcContext.GetHomeItem<HomePage>().ItemId)).ToList());
            }
            return null;
        }

        [DisableTracking]
        public JObject GetTilesPerPage(string datasource, string mode, int currentPage, int itemsPerPage)
        {
            if (string.IsNullOrEmpty(mode))
            {
                return null;
            }
            if (string.IsNullOrEmpty(datasource))
            {
                return null;
            }
            var item = _mvcContext.SitecoreService.GetItem<Item>(datasource);
            if (item != null)
            {
                string templateId = item.TemplateID.ToString().ToLower();
                var navigationTilesDto = new ArticleNavigationTilesView() { PageSize = 3, NumberTags = 1000 };
                if (templateId == GlassMapperHelper.GetSitecoreTemplateId<SeriesPage>().ToLower())
                {
                    var series = _mvcContext.SitecoreService.GetItem<SeriesPage>(item, x => x.InferType());
                    AddToNavigationTiles(series.SeriesItems, navigationTilesDto, currentPage, itemsPerPage);
                }
                else if (templateId == GlassMapperHelper.GetSitecoreTemplateId<NavigationTilesModule>().ToLower())
                {
                    var navigationTiles = _mvcContext.SitecoreService.GetItem<NavigationTilesModule>(item, x => x.InferType());
                    if (navigationTiles != null)
                    {
                        navigationTilesDto = mapper.Map<ArticleNavigationTilesView>(navigationTiles);
                        if (navigationTiles.ParentTiles != null)
                        {
                            var allTilesReturned = AddToAllTiles(navigationTiles.ParentTiles, new List<DataItemBase>());
                            AddToNavigationTiles(allTilesReturned, navigationTilesDto, currentPage, itemsPerPage);
                        }
                    }
                }
                if (navigationTilesDto.Tiles != null)
                {
                    navigationTilesDto.Tiles = navigationTilesDto.Tiles.OrderBy(x => int.Parse(x.SortOrder)).ToList();

                    return JObject.FromObject(navigationTilesDto);
                }

            }
            return null;
        }


        [DisableTracking]
        public JObject GetTiles(string datasource, string mode)
        {
            if (string.IsNullOrEmpty(mode))
            {
                return null;
            }
            if (string.IsNullOrEmpty(datasource))
            {
                return null;
            }
            var item = _mvcContext.SitecoreService.GetItem<Sitecore.Data.Items.Item>(datasource);
            if (item != null)
            {
                string templateId = item.TemplateID.ToString().ToLower();
                var navigationTilesDto = new ArticleNavigationTilesView() { PageSize = 3, NumberTags = 1000 };
                if (templateId == GlassMapperHelper.GetSitecoreTemplateId<SeriesPage>().ToLower())
                {
                    var series = _mvcContext.SitecoreService.GetItem<SeriesPage>(item);
                    AddToNavigationTiles(series.SeriesItems, navigationTilesDto);
                }
                else if (templateId == GlassMapperHelper.GetSitecoreTemplateId<NavigationTilesModule>().ToLower())
                {
                    var navigationTiles = _mvcContext.SitecoreService.GetItem<NavigationTilesModule>(item, x => x.InferType());
                    if (navigationTiles != null)
                    {
                        navigationTilesDto = mapper.Map<ArticleNavigationTilesView>(navigationTiles);
                        if (navigationTiles.ParentTiles != null)
                        {
                            foreach (var parentTile in navigationTiles.ParentTiles)
                            {
                                AddToNavigationTiles(parentTile.ChildrenTiles, navigationTilesDto);
                            }
                        }
                    }

                }
                if (navigationTilesDto.Tiles != null)
                {
                    navigationTilesDto.Tiles = navigationTilesDto.Tiles.OrderBy(x => int.Parse(x.SortOrder)).ToList();

                    return JObject.FromObject(navigationTilesDto);
                }

            }
            return null;
        }

        private void AddToNavigationTiles<T>(IEnumerable<T> list, ArticleNavigationTilesView navigationTilesDto) where T : DataItemBase
        {
            if (list != null)
            {
                foreach (T item in list)
                {
                    if (item is ArticlePage)
                    {
                        var articlePageItem = item as ArticlePage;

                        if (!articlePageItem.HideFromNav)
                            navigationTilesDto.Tiles.Add(mapper.Map<ArticleNavigationTile>(item));
                    }
                    if (item is LearnPage)
                    {
                        var learnPageItem = item as LearnPage;

                        if (!learnPageItem.HideFromNav)
                            navigationTilesDto.Tiles.Add(mapper.Map<ArticleNavigationTile>(item));
                    }
                    if (item is SeriesPage)
                    {
                        var seriesPage = item as SeriesPage;

                        if (!seriesPage.HideFromNav)
                            navigationTilesDto.Tiles.Add(mapper.Map<ArticleNavigationTile>(item));
                    }
                }
            }
        }

        private List<DataItemBase> AddToAllTiles<T>(IEnumerable<T> list, List<DataItemBase> allTiles) where T : NavigationTiles
        {
            if (list != null)
            {
                foreach (T item in list)
                {
                    if (item.ChildrenTiles != null)
                    {
                        foreach (DataItemBase childItem in item.ChildrenTiles)
                        {
                            allTiles.Add(childItem);
                        }
                    }
                }
            }
            return allTiles;
        }

        private void AddToNavigationTiles<T>(IEnumerable<T> list, ArticleNavigationTilesView navigationTilesDto, int currentPage, int itemsPerPage) where T : DataItemBase
        {
            if (list != null)
            {
                var firstIndex = (currentPage - 1) * itemsPerPage;

                var allPages = list.Where(i => i is ArticlePage || i is LearnPage || i is SeriesPage)
                            .Cast<PageBase>()
                            .Where(i => !i.HideFromNav);

                var totalPages = (int)Math.Ceiling((decimal)allPages.Count() / itemsPerPage);

                if (currentPage > totalPages)
                {
                    return;
                }

                var allPagesRange = allPages.Skip(firstIndex).Take(itemsPerPage);

                foreach (PageBase item in allPagesRange)
                {
                    navigationTilesDto.Tiles.Add(mapper.Map<ArticleNavigationTile>(item));
                }
            }
        }

        private string GetSeriesItemType(string id)
        {
            string templateId = "{" + id.ToUpper() + "}";
            if (templateId == GlassMapperHelper.GetSitecoreTemplateId<ArticlePage>().ToUpper())
            {
                return "Article";
            }
            else if (templateId == GlassMapperHelper.GetSitecoreTemplateId<LearnPage>().ToUpper())
            {
                return "Learn";
            }
            return "SeriesItemPageBase";
        }

        public ActionResult RenderSeries()
        {
            var datasource = _mvcContext.GetContextItem<Sitecore.Data.Items.Item>().ID.ToString();
            return View(new ArticleNavigationTilesViewModel() { Datasource = datasource });
        }
    }
}