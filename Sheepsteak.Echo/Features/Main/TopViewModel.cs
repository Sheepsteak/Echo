﻿using Caliburn.Micro;
using Sheepsteak.Echo.Core;
using Sheepsteak.Echo.Features.Articles;
using Sheepsteak.Echo.Framework;
using Sheepsteak.Echo.Resources;
using System.Linq;
using System.Threading.Tasks;

namespace Sheepsteak.Echo.Features.Main
{
    public class TopViewModel : Screen, IRefreshableScreen
    {
        private readonly ICacheService cacheService;
        private readonly EchoJsClient echoJsClient;
        private bool isBusy;
        private readonly INavigationService navigationService;
        private Article selectedArticle;

        public TopViewModel(
            INavigationService navigationService,
            ICacheService cacheService,
            EchoJsClient echoJsClient)
        {
            this.navigationService = navigationService;
            this.cacheService = cacheService;
            this.echoJsClient = echoJsClient;

            this.Articles = new BindableCollection<Article>();
            this.DisplayName = AppResources.TopPivotTitle;
        }

        public BindableCollection<Article> Articles { get; private set; }

        public bool IsBusy
        {
            get { return this.isBusy; }
            set
            {
                this.isBusy = value;
                this.NotifyOfPropertyChange(() => this.IsBusy);
            }
        }

        protected override async void OnActivate()
        {
            base.OnActivate();

            await this.RefreshArticles();
        }

        public void ArticleSelected(Article article)
        {
            this.cacheService.Articles[article.Id] = article;
            var uriBuilder = this.navigationService.UriFor<ArticlePageViewModel>();
            uriBuilder.WithParam(v => v.ArticleId, article.Id);
            this.navigationService.Navigate(uriBuilder.BuildUri());
        }

        public async Task RefreshArticles()
        {
            if (this.IsBusy)
            {
                return;
            }

            this.IsBusy = true;

            this.Articles.Clear();

            var articles = await this.echoJsClient.GetTopNews();

            this.Articles.AddRange(articles.ToList());

            this.IsBusy = false;
        }

    }
}
