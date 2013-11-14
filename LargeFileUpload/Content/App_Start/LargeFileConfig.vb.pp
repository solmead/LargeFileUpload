imports System
Imports System.Web
Imports System.Web.Optimization

<assembly: WebActivatorEx.PreApplicationStartMethod(
    gettype($rootnamespace$.App_Start.LargeFileConfig), "PreStart")>

Namespace $rootnamespace$.App_Start 
    public class LargeFileConfig 
        public shared sub PreStart() 
			RouteTable.Routes.IgnoreRoute("{*allaxd}", New With {.allaxd = ".*\.axd(/.*)?"})
			RegisterBundles(BundleTable.Bundles)
        end sub
		Public Shared Sub RegisterBundles(ByVal bundles As BundleCollection)

            bundles.Add(New ScriptBundle("~/bundles/LargeFileUpload").Include(
                "~/Scripts/LargeFileUpload/Silverlight.js",
                "~/Scripts/LargeFileUpload/swfupload.js",
                "~/Scripts/LargeFileUpload/swfupload.speed.js",
                "~/Scripts/LargeFileUpload/SilverlightUpload.js",
                "~/Scripts/LargeFileUpload/FlashUpload.js",
                "~/Scripts/LargeFileUpload/LargeUpload.js"))

        End Sub

    End Class
End Namespace