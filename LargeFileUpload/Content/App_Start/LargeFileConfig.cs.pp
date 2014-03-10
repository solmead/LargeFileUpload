using System;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web;
using System.Web.Optimization;


[assembly: WebActivatorEx.PreApplicationStartMethod(
    typeof($rootnamespace$.App_Start.LargeFileConfig), "PreStart")]

namespace $rootnamespace$.App_Start {
    public static class LargeFileConfig {
        public static void PreStart() {
			RouteTable.Routes.IgnoreRoute("{*allaxd}", new { allaxd = @".*\.axd(/.*)?" });
			RegisterBundles(BundleTable.Bundles);
        }
		
		public static void RegisterBundles(BundleCollection bundles) {

            bundles.Add(new ScriptBundle("~/bundles/LargeFileUpload").Include(
                "~/Scripts/LargeFileUpload/Silverlight.js",
                "~/Scripts/LargeFileUpload/swfupload.js",
                "~/Scripts/LargeFileUpload/swfupload.speed.js",
                "~/Scripts/LargeFileUpload/jquery.swfupload.js",
                "~/Scripts/LargeFileUpload/SilverlightUpload.js",
                "~/Scripts/LargeFileUpload/FlashUpload.js",
                "~/Scripts/LargeFileUpload/LargeUpload.js"));

        }
    }
}