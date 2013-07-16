﻿using Caliburn.Micro;
using Sheepsteak.Echoes.Core;
using Sheepsteak.Echoes.UI.Framework;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sheepsteak.Echoes.UI.Features.Articles
{
    public class CommentsPageViewModel : Screen
    {
        private bool isBusy;
        private readonly EchoJsClient echoJsClient;
        private readonly IEventAggregator eventAggregator;

        public CommentsPageViewModel(
            EchoJsClient echoJsClient,
            IEventAggregator eventAggregator)
        {
            this.echoJsClient = echoJsClient;
            this.eventAggregator = eventAggregator;

            this.Comments = new BindableCollection<Comment>();
        }

        public int ArticleId { get; set; }

        public string ArticleTitle { get; set; }

        public BindableCollection<Comment> Comments { get; private set; }

        public bool IsBusy
        {
            get { return this.isBusy; }
            private set
            {
                this.isBusy = value;

                this.NotifyOfPropertyChange(() => this.IsBusy);
            }
        }

        public void OpenInBrowser()
        {
            //this.eventAggregator.RequestTask<WebBrowserTask>(task =>
            //{
            //    task.Uri = new Uri(this.Article.Url);
            //});
        }

        protected async override void OnInitialize()
        {
            base.OnInitialize();

            await this.GetComments();
        }

        private async Task GetComments()
        {
            this.IsBusy = true;

            IEnumerable<Comment> comments = null;
            bool showFailMessage = false;
            try
            {
                comments = await this.echoJsClient.GetCommentsForArticle(this.ArticleId);
            }
            catch (HttpRequestException)
            {
                showFailMessage = true;
            }
            catch (UnsupportedMediaTypeException e)
            {
                showFailMessage = true;
            }

            this.IsBusy = false;

            if (showFailMessage)
            {
                await Task.Delay(50);

                var showMessageBoxResult = new ShowMessageBoxResult(
                    "There was an error trying to get the comments.",
                    "Echoes");

                await showMessageBoxResult.ExecuteAsync();
            }
            else
            {
                foreach (var comment in comments)
                {
                    this.Comments.Add(comment);
                }
            }
        }
    }
}
