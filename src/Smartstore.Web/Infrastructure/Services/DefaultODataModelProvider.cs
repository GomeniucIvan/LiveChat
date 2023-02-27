using Microsoft.OData.ModelBuilder;
using Smartstore.Core.Common;
using Smartstore.Core.Configuration;
using Smartstore.Core.Content.Media;
using Smartstore.Core.Identity;
using Smartstore.Core.Localization;
using Smartstore.Core.Messaging;
using Smartstore.Core.Seo;
using Smartstore.Core.Stores;
using Smartstore.Web.Api.Models.Media;

namespace Smartstore.Web.Api
{
    internal class DefaultODataModelProvider : ODataModelProviderBase
    {
        public override void Build(ODataModelBuilder builder, int version)
        {
            builder.Namespace = string.Empty;

            builder.EntitySet<CustomerRoleMapping>("CustomerRoleMappings");
            builder.EntitySet<CustomerRole>("CustomerRoles");
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Download>("Downloads");
            builder.EntitySet<GenericAttribute>("GenericAttributes");
            builder.EntitySet<Language>("Languages");
            builder.EntitySet<LocalizedProperty>("LocalizedProperties");
            builder.EntitySet<Setting>("Settings");
            builder.EntitySet<Store>("Stores");
            builder.EntitySet<UrlRecord>("UrlRecords");

            // INFO: functions specified directly on the ODataModelBuilder (instead of entity type or collection)
            // are called unbound functions (like static operations on the service).

            BuildMediaFiles(builder);
            BuildMediaFolders(builder);
        }

        //todo api
        //public override Stream GetXmlCommentsStream(IApplicationContext appContext)
        //    => GetModuleXmlCommentsStream(appContext, Module.SystemName);

        private static void BuildMediaFiles(ODataModelBuilder builder)
        {
            // INFO: #2198 referenced entity set type must not differ to avoid InvalidOperationException in Microsoft.OData.Client.
            var set = builder.EntitySet<FileItemInfo>("MediaFiles");
            //var set = builder.EntitySet<MediaFile>("MediaFiles");
            var config = set.EntityType.Collection;

            //var infoSet = builder.EntitySet<FileItemInfo>("FileItemInfos");
            //infoSet.EntityType.HasKey(x => x.Id);
            //infoSet.EntityType.HasRequired(x => x.File).AutomaticallyExpand(true);
            //infoSet.HasRequiredBinding(x => x.File, set);

            //config.Action(nameof(MediaFilesController.GetFileByPath))
            //    .ReturnsFromEntitySet(set)
            //    .Parameter<string>("path")
            //    .Required();

            //config.Function(nameof(MediaFilesController.GetFilesByIds))
            //    .ReturnsCollectionFromEntitySet<FileItemInfo>(set.EntityType.Name)
            //    .CollectionParameter<int>("ids")
            //    .Required();

            //config.Function(nameof(MediaFilesController.DownloadFile))
            //    .Returns<StreamContent>()
            //    .Parameter<int>("id")
            //    .Required();

            //config.Action(nameof(MediaFilesController.SearchFiles))
            //    .ReturnsCollectionFromEntitySet<FileItemInfo>(set.EntityType.Name)
            //    .Parameter<MediaSearchQuery>("query")
            //    .Optional();

            //config.Action(nameof(MediaFilesController.CountFiles))
            //    .Returns<int>()
            //    .Parameter<MediaSearchQuery>("query")
            //    .Optional();

            //config.Action(nameof(MediaFilesController.CountFilesGrouped))
            //    .Returns<MediaCountResult>()
            //    .Parameter<MediaFilesFilter>("filter")
            //    .Optional();

            //config.Action(nameof(MediaFilesController.FileExists))
            //    .Returns<bool>()
            //    .Parameter<string>("path")
            //    .Required();

            //config.Action(nameof(MediaFilesController.CheckUniqueFileName))
            //    .Returns<CheckUniquenessResult>()
            //    .Parameter<string>("path")
            //    .Required();

            //var moveFile = set.EntityType
            //    .Action(nameof(MediaFilesController.MoveFile))
            //    .ReturnsFromEntitySet(set);
            //moveFile.Parameter<string>("destinationFileName")
            //    .Required();
            //moveFile.Parameter<DuplicateFileHandling>("duplicateFileHandling")
            //    .Optional();

            //var copyFile = set.EntityType
            //    .Action(nameof(MediaFilesController.CopyFile))
            //    .Returns<MediaFileOperationResult>();
            //copyFile.Parameter<string>("destinationFileName")
            //    .Required();
            //copyFile.Parameter<DuplicateFileHandling>("duplicateFileHandling")
            //    .Optional();

            //var deleteFile = set.EntityType
            //    .Action(nameof(MediaFilesController.DeleteFile));
            //deleteFile.Parameter<bool>("permanent")
            //    .Required();
            //deleteFile.Parameter<bool>("force")
            //    .HasDefaultValue(bool.FalseString)
            //    .Optional();

            //var saveFile = config
            //    .Action(nameof(MediaFilesController.SaveFile))
            //    .ReturnsFromEntitySet(set);
            //saveFile.Parameter<IFormFile>("file")
            //    .Required();
            //saveFile.Parameter<string>("path")
            //    .Optional();
            //saveFile.Parameter<bool>("isTransient")
            //    .HasDefaultValue(bool.TrueString)
            //    .Optional();
            //saveFile.Parameter<DuplicateFileHandling>("duplicateFileHandling")
            //    .Optional();
        }

        private static void BuildMediaFolders(ODataModelBuilder builder)
        {
            // INFO: #2198 referenced entity set type must not differ to avoid InvalidOperationException in Microsoft.OData.Client.
            //var set = builder.EntitySet<MediaFolder>("MediaFolders");
            var set = builder.EntitySet<FolderNodeInfo>("MediaFolders");
            set.EntityType.HasKey(x => x.Id);

            var config = set.EntityType.Collection;

            //config.Action(nameof(MediaFoldersController.FolderExists))
            //    .Returns<bool>()
            //    .Parameter<string>("path")
            //    .Required();

            //config.Action(nameof(MediaFoldersController.CheckUniqueFolderName))
            //    .Returns<CheckUniquenessResult>()
            //    .Parameter<string>("path")
            //    .Required();

            //config.Function(nameof(MediaFoldersController.GetRootNode))
            //    .ReturnsFromEntitySet<FolderNodeInfo>(set.EntityType.Name);

            //config.Action(nameof(MediaFoldersController.GetNodeByPath))
            //    .ReturnsFromEntitySet(set)
            //    .Parameter<string>("path")
            //    .Required();

            //config.Action(nameof(MediaFoldersController.CreateFolder))
            //    .ReturnsFromEntitySet(set)
            //    .Parameter<string>("path")
            //    .Required();

            //var moveFolder = config.Action(nameof(MediaFoldersController.MoveFolder))
            //    .ReturnsFromEntitySet(set);
            //moveFolder.Parameter<string>("path")
            //    .Required();
            //moveFolder.Parameter<string>("destinationPath")
            //    .Required();

            //var copyFolder = config
            //    .Action(nameof(MediaFoldersController.CopyFolder))
            //    .Returns<MediaFolderOperationResult>();
            //copyFolder.Parameter<string>("path")
            //    .Required();
            //copyFolder.Parameter<string>("destinationPath")
            //    .Required();
            //copyFolder.Parameter<DuplicateEntryHandling>("duplicateEntryHandling")
            //    .Optional();

            //var deleteFolder = config
            //    .Action(nameof(MediaFoldersController.DeleteFolder))
            //    .Returns<MediaFolderDeleteResult>();
            //deleteFolder.Parameter<string>("path")
            //    .Required();
            //deleteFolder.Parameter<FileHandling>("fileHandling")
            //    .Optional();
        }
    }
}