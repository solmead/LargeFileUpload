/*
* Copyright (C) 2009-2012 Solmead Productions
*
* == BEGIN LICENSE ==
*
* Licensed under the terms of any of the following licenses at your
* choice:
*
*  - GNU General Public License Version 2 or later (the "GPL")
*    http://www.gnu.org/licenses/gpl.html
*
*  - GNU Lesser General Public License Version 2.1 or later (the "LGPL")
*    http://www.gnu.org/licenses/lgpl.html
*
*  - Mozilla Public License Version 1.1 or later (the "MPL")
*    http://www.mozilla.org/MPL/MPL-1.1.html
*
* == END LICENSE ==
*/
//window.System = window.System || {};

Namespace.Register("System.FileUpload.Silverlight");

System.FileUploaders = [];

System.FileUpload.Silverlight.CreateFileUploader = function (Area, FileGUID, initializedCallBack, finishedCallBack) {
    var obj = {};

    var fileGUID = "not_set";
    var callbackWhenFinished = null;
    var BaseArea = null;

    formatUnits = function (baseNumber, unitDivisors, unitLabels, singleFractional) {
        var i, unit, unitDivisor, unitLabel;

        if (baseNumber === 0) {
            return "0 " + unitLabels[unitLabels.length - 1];
        }

        if (singleFractional) {
            unit = baseNumber;
            unitLabel = unitLabels.length >= unitDivisors.length ? unitLabels[unitDivisors.length - 1] : "";
            for (i = 0; i < unitDivisors.length; i++) {
                if (baseNumber >= unitDivisors[i]) {
                    unit = (baseNumber / unitDivisors[i]).toFixed(2);
                    unitLabel = unitLabels.length >= i ? " " + unitLabels[i] : "";
                    break;
                }
            }

            return unit + unitLabel;
        } else {
            var formattedStrings = [];
            var remainder = baseNumber;

            for (i = 0; i < unitDivisors.length; i++) {
                unitDivisor = unitDivisors[i];
                unitLabel = unitLabels.length > i ? " " + unitLabels[i] : "";

                unit = remainder / unitDivisor;
                if (i < unitDivisors.length - 1) {
                    unit = Math.floor(unit);
                } else {
                    unit = unit.toFixed(2);
                }
                if (unit > 0) {
                    remainder = remainder % unitDivisor;

                    formattedStrings.push(unit + unitLabel);
                }
            }

            return formattedStrings.join(" ");
        }
    };
    formatBPS = function (baseNumber) {
        var bpsUnits = [1073741824, 1048576, 1024, 1], bpsUnitLabels = ["Gbps", "Mbps", "Kbps", "bps"];
        return formatUnits(baseNumber, bpsUnits, bpsUnitLabels, true);

    };
    formatTime = function (baseNumber) {
        var timeUnits = [86400, 3600, 60, 1], timeUnitLabels = ["d", "h", "m", "s"];
        return formatUnits(baseNumber, timeUnits, timeUnitLabels, false);

    };
    formatBytes = function (baseNumber) {
        var sizeUnits = [1073741824, 1048576, 1024, 1], sizeUnitLabels = ["GB", "MB", "KB", "bytes"];
        return formatUnits(baseNumber, sizeUnits, sizeUnitLabels, true);

    };
    formatPercent = function (baseNumber) {
        return baseNumber.toFixed(2) + " %";
    };

    var uploaderIndex = 0;
    var slCtl = null;

    obj = {
        areaID: null,
        init: function (area, initializedCallBack, finishedcallback) {
            var id = $(area).attr("id");
            obj.areaID = id;
            fileGUID = FileGUID;
            callbackWhenFinished = finishedcallback;
            System.FileUploaders.push(obj);
            uploaderIndex = System.FileUploaders.length - 1;
            obj.CreateUploader(area);
            slCtl = document.getElementById(id);
//            var it = System.FileUploaders[uploaderIndex];
//            
            //            var i = 0;
            if (initializedCallBack) {
                initializedCallBack();
            }
        },
        StopUpload: function () {
            slCtl.Content.Page.StopUpload();
        },
        ShowDialog: function () {
            slCtl.Content.Page.ShowDialog();
        },
        CreateUploader: function (position) {
            BaseArea = $(position);
            var id = $(BaseArea).attr("id");
            obj.CreateSilverlightObject();



            $(BaseArea).before($("<div id='" + id + "_completedMessage' style='display:none; float:left;'/>"));
            $(BaseArea).before($("<input type='hidden' id='" + id + "_filename' name='" + id + "_filename'/>"));
            $(BaseArea).before($("<input type='hidden' id='" + id + "_guid' name='" + id + "_guid'/>"));
//            $(BaseArea).before($('<a href="#" id="PickFileButton" class="button">Pick a File</a>'));
//            $("#PickFileButton").button();
//            $("#PickFileButton").click(function (evt) {
//                evt.preventDefault();
//                obj.ShowDialog();
//            });


            obj.CreateDisplayArea();
            $(BaseArea).remove();
            $("#" + id + "progress-bar").hide();
            $("#" + id + "_uploading").hide();

            $("#" + id + "_uploaderarea").dialog({
                autoOpen: false,
                bgiframe: true,
                height: 140,
                width: 400,
                modal: false
            });
        },
        SilverLightLoaded: function (sender, args) {
            slCtl = sender.getHost();
        },
        CreateSilverlightObject: function () {
            var id = $(BaseArea).attr("id");
            var args = "";
            args = "UploadChunkSize=-1,MaximumUpload=-1,Filter=" + "All Files (*.*)|*.*" + ",JavascriptCompleteFunction=" + ("SilverComplete") + ",FileGUID=" + fileGUID + ",JavascriptStartFunction=" + ("SilverStart") + ",JavascriptStatFunction=" + ("SilverUpdate") + ",ID=" + id + ",JavascriptMessageFunction=SilverMessage";

            var silver = '    <object ID="' + id + '" data="data:application/x-silverlight-2," type="application/x-silverlight-2" width="105px" height="50px" style="">';
            silver = silver + '<param name="source" value="/ClientBin/FileUploader.xap"/>';
            silver = silver + '<param name="onError" value="System.onSilverlightError" />';
            silver = silver + '<param name="windowless" value="true" />';
            silver = silver + '<param name="background" value="Transparent" />';
            silver = silver + '<param name="minRuntimeVersion" value="5.0.61118.0" />';
            silver = silver + '<param name="autoUpgrade" value="true" />';
            silver = silver + '<param name="initParams" value="' + args + '" />';
            silver = silver + '<param name="onLoad" value="System.FileUploaders[' + uploaderIndex + '].SilverLightLoaded" />';
            silver = silver + '<a href="http://go.microsoft.com/fwlink/?LinkID=149156&v=5.0.61118.0" style="text-decoration:none">';
            silver = silver + '  <img src="http://go.microsoft.com/fwlink/?LinkId=161376" alt="Get Microsoft Silverlight" style="border-style:none"/>';
            silver = silver + '</a>';
            silver = silver + ' </object><iframe id="_sl_historyFrame" style="visibility:hidden;height:0px;width:0px;border:0px"></iframe>';


            $(BaseArea).before($("<div id='" + id + "_swf' style='float:left;'>" + silver + "</div>"));
        },
        CreateDisplayArea: function () {
            var id = $(BaseArea).attr("id");
            var container = $("<div id='" + id + "_uploaderarea' class='async-uploader'/>");
            container.append($("<div id='" + id + "progress-bar' style='display:none;'><div style='background:#ff0000;'>&nbsp;</div></div>"));
            container.append($("<div id='" + id + "_uploading' style='display:none;'>Uploading... <input type='button' value='Cancel' class='button' style='float:right; display:block;' />" +
                "<div style='display:none;'><span>Current Rate: </span><span id='" + id + "_MovingAverageSpeed'/></div>" +
                "<div style='display:block;'><span>Time Remaining: </span><span id='" + id + "_TimeRemaining'/></div>" +
                "<div style='display:none;'><span>Time Elapsed: </span><span id='" + id + "_TimeElapsed'></span></div>" +
                "<div style='display:none;'><span>Percent Uploaded: </span><span id='" + id + "_PercentUploaded'></span></div>" +
                "<div style='display:block;'><span>Amount of File uploaded: </span><span id='" + id + "_SizeUploaded'/></div>" +
                +"</div>"));
            $(BaseArea).before(container);
        },
        SilverStart: function (UI) {
            System.DebugWrite("SilverStart");
            $("#" + UI.ID + "_filename").val("");
            $("#" + UI.ID + "_guid").val("");
            $("#" + UI.ID + "progress-bar div").css("width", "0px");
            $("#" + UI.ID + "progress-bar").show();
            $("#" + UI.ID + "_uploading").show();
            $("#" + UI.ID + "_completedMessage").html("").hide();
            $("#" + UI.ID + "_uploaderarea").dialog("open");
            $('input[type="submit"]').attr("disabled", "disabled");
        },
        SilverComplete: function (UI) {
            System.DebugWrite("SilverComplete");

            var file = UI;
            $("#" + UI.ID + "_filename").val(file.Name);
            $("#" + UI.ID + "_guid").val("");
            $("#" + UI.ID + "_completedMessage").html("<span class='bright'>Success!</span> Uploaded <b>{0}</b> ({1} KB)."
                    .replace("{0}", file.Name)
                    .replace("{1}", Math.round(file.TotalBytes / 1024))
            );
            //alert("Complete");
            $("#" + UI.ID + "_uploaderarea").dialog("close");
            var clearup = function () {
                $("#" + UI.ID + "progress-bar").hide();
                $("#" + UI.ID + "_completedMessage").show();
                $("#" + UI.ID + "_uploading").hide();
            };
            if ($("#" + UI.ID + "_filename").val() != "") // Success
                $("#" + UI.ID + "progress-bar div").animate({ width: "100%" }, { duration: "fast", queue: false, complete: clearup });
            else // Fail
                clearup();

            $('input[type="submit"]').removeAttr("disabled");
            if (callbackWhenFinished) {
                callbackWhenFinished();
            }
        },
        SilverUpdate: function (UI) {
            var tdMovingAverageSpeed = document.getElementById("" + UI.ID + "_MovingAverageSpeed");
            var tdTimeRemaining = document.getElementById("" + UI.ID + "_TimeRemaining");
            var tdTimeElapsed = document.getElementById("" + UI.ID + "_TimeElapsed");
            var tdPercentUploaded = document.getElementById("" + UI.ID + "_PercentUploaded");
            var tdSizeUploaded = document.getElementById("" + UI.ID + "_SizeUploaded");

            var file = UI;
            var percent = 100 * file.bytesUploaded / file.TotalBytes;
            $("#" + UI.ID + "progress-bar div").animate({ width: percent + "%" }, { duration: 500, queue: false });
            //this.customSettings.tdCurrentSpeed.innerHTML = SWFUpload.speed.formatBPS(file.currentSpeed);
            //this.customSettings.tdAverageSpeed.innerHTML = SWFUpload.speed.formatBPS(file.averageSpeed);
            //tdMovingAverageSpeed.innerHTML = formatBPS(file.movingAverageSpeed);
            tdTimeRemaining.innerHTML = formatTime(file.timeRemaining);
            //tdTimeElapsed.innerHTML = formatTime(file.timeElapsed);
            //tdPercentUploaded.innerHTML = formatPercent(file.percentUploaded);
            tdSizeUploaded.innerHTML = formatBytes(file.bytesUploaded);
        },
        SilverMessage: function (UI) {
            System.DebugWrite(UI.Message);
        }
    };

    $(function () {
        obj.init(Area, initializedCallBack, finishedCallBack);
    });

    return obj;
};

function SilverStart(UI) {
    $.each(System.FileUploaders, function (i, item) {
        if (item.areaID == UI.ID) {
            item.SilverStart(UI);
        }
    });
}
function SilverUpdate(UI) {
    $.each(System.FileUploaders, function (i, item) {
        if (item.areaID == UI.ID) {
            item.SilverUpdate(UI);
        }
    });
}
function SilverMessage(UI) {
    
    System.DebugWrite(UI.Message);
}
function SilverComplete(UI) {
    $.each(System.FileUploaders, function (i, item) {
        if (item.areaID == UI.ID) {
            item.SilverComplete(UI);
        }
    });
}

