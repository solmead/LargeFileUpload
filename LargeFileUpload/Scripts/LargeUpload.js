
System = System || {};
System.FileUpload = System.FileUpload || {};
System.FileUpload.Flash = System.FileUpload.Flash || {};


System.FileUploader = function (Area, FileGUID, finishedCallBack) {
    System.FileUpload.Flash.CreateFileUploader(Area, FileGUID, null, finishedCallBack);
};

