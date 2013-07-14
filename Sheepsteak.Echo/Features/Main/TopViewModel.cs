﻿using Caliburn.Micro;
using Sheepsteak.Echo.Core;
using Sheepsteak.Echo.Features.Articles;
using Sheepsteak.Echo.Framework;
using Sheepsteak.Echo.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sheepsteak.Echo.Features.Main
{
    public class TopViewModel : Screen, IRefreshableScreen
    {
        private const int offsetLimit = 5;
        private readonly ICacheService cacheService;
        private readonly EchoJsClient echoJsClient;
        private bool isRefreshing;
        private readonly INavigationService navigationService;
        private Article selectedArticle;
        private bool showFailureMessage;

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

        public bool IsRefreshing
        {
            get { return this.isRefreshing; }
            set
            {
                this.isRefreshing = value;
                this.NotifyOfPropertyChange(() => this.IsRefreshing);
            }
        }

        public Article SelectedArticle
        {
            get { return this.selectedArticle; }
            set
            {
                this.selectedArticle = value;

                if (this.selectedArticle != null)
                {
                    this.NavigateToArticlePage(this.selectedArticle);
                }

                this.NotifyOfPropertyChange(() => this.SelectedArticle);
            }
        }

        public bool ShowFailureMessage
        {
            get { return this.showFailureMessage; }
            private set
            {
                this.showFailureMessage = value;
                this.NotifyOfPropertyChange(() => this.ShowFailureMessage);
            }
        }

        public async Task LoadMore()
        {
            //if (this.Articles == null)
            //{
            //    return;
            //}

            //var articleCount = this.Articles.Count;

            //if (!this.IsRefreshing)
            //{

            //}
        }

        public async Task RefreshArticles()
        {
            if (this.IsRefreshing)
            {
                return;
            }

            this.ShowFailureMessage = false;
            this.IsRefreshing = true;

            this.Articles.Clear();

            IEnumerable<Article> articles = null;

            bool showFailMessage = false;
            try
            {
                articles = await this.echoJsClient.GetTopNews();
            }
            catch (HttpRequestException)
            {
                showFailMessage = true;
            }
            catch (UnsupportedMediaTypeException e)
            {
                showFailMessage = true;
            }

            this.IsRefreshing = false;

            if (showFailMessage)
            {
                await Task.Delay(50);

                var showMessageBoxResult = new ShowMessageBoxResult(
                    "There was an error trying to get the top articles.",
                    "Echo");

                await showMessageBoxResult.ExecuteAsync();
                this.ShowFailureMessage = true;
            }
            else
            {
                this.Articles.AddRange(articles.ToList());
            }
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);

            this.SelectedArticle = null;
        }

        protected async override void OnInitialize()
        {
            base.OnInitialize();

            await this.RefreshArticles();
        }

        private void NavigateToArticlePage(Article article)
        {
            this.cacheService.Articles[article.Id] = article;
            var uriBuilder = this.navigationService.UriFor<ArticlePageViewModel>();
            uriBuilder.WithParam(v => v.ArticleId, article.Id);
            this.navigationService.Navigate(uriBuilder.BuildUri());
        }
    }
}
