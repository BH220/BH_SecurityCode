using BH_SecurityCode.Api.Interface;
using BH_SecurityCode.Api.Manager;
using BH_SecurityCode.Common.Configurations;
using BH_SecurityCode.Services;
using BH_SecurityCode.ViewModels;
using BH_SecurityCode.ViewModels.BankBook;
using BH_SecurityCode.ViewModels.BankCode;
using BH_SecurityCode.ViewModels.Card;
using BH_SecurityCode.ViewModels.IdCard;
using BH_SecurityCode.ViewModels.Site;
using BH_SecurityCode.Views;
using BH_SecurityCode.Views.BankCode;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using Wpf.Ui;
using INavigationService = BH_SecurityCode.Common.Navigation.INavigationService;
using NavigationService = BH_SecurityCode.Common.Navigation.NavigationService;

namespace BH_SecurityCode
{
    public static class DiService
    {
        public static ServiceProvider ServicesRegister()
        {
            var services = new ServiceCollection();

            // 서비스
            services.AddSingleton<IAppSettings, AppSettings>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<ISnackbarService, SnackbarService>();
            services.AddSingleton<MainViewModelConfiguration>();
            services.AddSingleton<IMessenger>(new WeakReferenceMessenger()); 

            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IClipboardService, ClipboardService>();

            // API 매니저 (로그인 외에는 API 연동 전 스텁)
            services.AddSingleton<IAuthManager, AuthManager>();
            services.AddSingleton<ICodeManager, CodeManager>();
            services.AddSingleton<IBankCodeManager, BankCodeManager>();
            services.AddSingleton<IBankBookManager, BankBookManager>();
            services.AddSingleton<ICardManager, CardManager>();
            services.AddSingleton<IIdCardManager, IdCardManager>();
            services.AddSingleton<IAccountManager, AccountManager>();
            services.AddSingleton<IImageManager, ImageManager>(); // 첨부 이미지 원본 내려받기 (공용)

            // ViewModel
            services.AddSingleton<MainViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<BankCodeListViewModel>();
            services.AddTransient<BankCodeEditViewModel>();
            services.AddTransient<BankCodeViewViewModel>();
            services.AddTransient<BankCodeSimpleViewViewModel>();
            services.AddTransient<BankBookListViewModel>();
            services.AddTransient<BankBookEditViewModel>();
            services.AddTransient<CardListViewModel>();
            services.AddTransient<CardEditViewModel>();
            services.AddTransient<IdCardListViewModel>();
            services.AddTransient<IdCardEditViewModel>();
            services.AddTransient<SiteListViewModel>();
            services.AddTransient<SiteEditViewModel>();

            services.AddSingleton<MainWindow>();
            services.AddTransient<LoginView>();
            services.AddTransient<BankCodeListView>();
            //services.AddTransient<BankCodeEditView>();
            //services.AddTransient<BankCodeViewView>();
            //services.AddTransient<BankCodeSimpleViewView>();
            //services.AddTransient<BankBookListView>();
            //services.AddTransient<BankBookEditView>();
            //services.AddTransient<CardListView>();
            //services.AddTransient<CardEditView>();
            //services.AddTransient<IdCardListView>();
            //services.AddTransient<IdCardEditView>();
            //services.AddTransient<SiteListView>();
            //services.AddTransient<SiteEditView>();


            return services.BuildServiceProvider();
        }
    }
}
